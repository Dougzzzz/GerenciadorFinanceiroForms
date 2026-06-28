using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Features.Investments;
using ControleFinanceiroForms.Features.ImportTransactions;
using ControleFinanceiroForms.Features.Categories;
using ControleFinanceiroForms.Features.Dashboard;

namespace ControleFinanceiroForms;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class App : Application
{
    private readonly IHost _host;
    private IServiceScope? _appScope;

    public App()
    {
        // ── Logging folder ─────────────────────────────────────────────
        var logsFolder = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsFolder);
        var logFilePath = Path.Combine(logsFolder, "controle-.log");

        // ── Serilog file logger ────────────────────────────────────────
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                // Add Serilog file sink via the MEL bridge
                logging.AddSerilog(Log.Logger, dispose: false);

                if (context.HostingEnvironment.IsDevelopment())
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                }
                else
                {
                    logging.SetMinimumLevel(LogLevel.Warning);
                }
            })
            .ConfigureServices((context, services) =>
            {
                // ── Database ───────────────────────────────────────────────
                var dbFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ControleFinanceiro");
                Directory.CreateDirectory(dbFolder);
                var dbPath = Path.Combine(dbFolder, "controle-financeiro.db");

                services.AddDbContext<AppDbContext>((serviceProvider, options) =>
                {
                    options.UseSqlite($"Data Source={dbPath}");

                    // Surface SQL queries at Debug level via the DI-provided ILoggerFactory.
                    // This depends on the logging setup above (task_09).
                    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                    options.UseLoggerFactory(loggerFactory);

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging();
                    }
                });

                // ── Services & Repositories ────────────────────────────────
                services.AddScoped<ITransactionRepository, TransactionRepository>();
                services.AddScoped<IInvestmentRepository, InvestmentRepository>();
                services.AddScoped<ICategoryRepository, CategoryRepository>();
                services.AddScoped<IParcelamentoRepository, ParcelamentoRepository>();
                services.AddScoped<IPagamentoParcelamentoRepository, PagamentoParcelamentoRepository>();
                services.AddScoped<ICsvParserService, CsvParserService>();
                services.AddScoped<IPdfParserService, PdfParserService>();
                services.AddScoped<IFilePickerService, WindowsFilePickerService>();

                // ── ViewModels ─────────────────────────────────────────────
                services.AddTransient<InvestmentsViewModel>();
                services.AddTransient<ImportTransactionsViewModel>();
                services.AddTransient<CategoriesViewModel>();
                services.AddTransient<DashboardViewModel>();

                // ── Presentation ───────────────────────────────────────────
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // ── Global exception handlers ─────────────────────────────────
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled exception (AppDomain) — app will terminate");
            Log.CloseAndFlush();
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Log.Error(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };

        try
        {
            await _host.StartAsync();

            _appScope = _host.Services.CreateScope();

            // Apply pending migrations on startup (idempotent)
            var db = _appScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

            var mainWindow = _appScope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Falha ao iniciar o aplicativo:\n{ex.Message}",
                "Erro de Inicialização",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            _appScope?.Dispose();
            await _host.StopAsync();
        }
        finally
        {
            _host.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
