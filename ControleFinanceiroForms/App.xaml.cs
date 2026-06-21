using System.IO;
using System.Windows;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroForms.Data;

namespace ControleFinanceiroForms;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();

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

                // ── Presentation ───────────────────────────────────────────
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await _host.StartAsync();

            // Apply pending migrations on startup (idempotent)
            using var scope = _host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
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
            await _host.StopAsync();
        }
        finally
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
