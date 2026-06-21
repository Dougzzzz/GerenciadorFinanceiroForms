using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiroForms.Tests;

/// <summary>
/// Smoke tests that verify the Microsoft.Extensions.Logging integration
/// established in App.xaml.cs is correctly wired into the DI container.
/// </summary>
[TestClass]
public sealed class LoggingTests
{
    private IHost _host = null!;

    /// <summary>
    /// Builds a host mirroring the production App.xaml.cs configuration
    /// (without the WPF-specific services) for headless unit testing.
    /// </summary>
    [TestInitialize]
    public void BuildHost()
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
            .ConfigureServices((_, services) =>
            {
                // Mirrors the real host; no WPF window needed for logging tests.
                services.AddTransient<LoggingTests>();
            })
            .Build();
    }

    [TestCleanup]
    public void DisposeHost() => _host?.Dispose();

    // ─────────────────────────────────────────────────────────────
    // Unit Tests
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// ILogger{T} must be resolvable from the DI container for any T.
    /// </summary>
    [TestMethod]
    public void ILoggerT_IsResolvable_WithoutException()
    {
        // Arrange / Act
        var logger = _host.Services.GetService<ILogger<LoggingTests>>();

        // Assert
        Assert.IsNotNull(logger, "ILogger<LoggingTests> should be registered in the DI container.");
    }

    /// <summary>
    /// ILoggerFactory must be resolvable — required by EF Core query logging (task_02).
    /// </summary>
    [TestMethod]
    public void ILoggerFactory_IsResolvable_WithoutException()
    {
        // Arrange / Act
        var loggerFactory = _host.Services.GetService<ILoggerFactory>();

        // Assert
        Assert.IsNotNull(loggerFactory, "ILoggerFactory should be registered in the DI container.");
    }

    // ─────────────────────────────────────────────────────────────
    // Integration Tests
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// The full IHost must start without errors when Console logging is configured.
    /// </summary>
    [TestMethod]
    public async Task Host_StartsCleanly_WithConsoleLogging()
    {
        // Act
        await _host.StartAsync();

        // Assert — no exception thrown means successful start
        Assert.IsNotNull(_host.Services, "Host services should be available after start.");

        await _host.StopAsync();
    }
}
