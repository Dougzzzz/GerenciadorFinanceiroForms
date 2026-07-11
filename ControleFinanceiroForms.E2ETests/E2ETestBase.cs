using System;
using System.IO;
using System.Threading;
using FlaUI.Core;
using FlaUI.UIA3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ControleFinanceiroForms.E2ETests;

public class E2ETestBase
{
    protected Application? App { get; private set; }
    protected UIA3Automation? Automation { get; private set; }
    protected FlaUI.Core.AutomationElements.Window? MainWindow { get; private set; }
    
    private string _tempDbPath = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        // Define a unique temporary database for this test run
        _tempDbPath = Path.Combine(Path.GetTempPath(), $"e2e_test_{Guid.NewGuid()}.db");
        
        // Ensure path to the main application executable
        var baseDir = AppContext.BaseDirectory;
        // Navigation from E2ETests bin -> main project bin
        // Typical path: E2ETests/bin/Debug/net9.0-windows/ -> ../../../../ControleFinanceiroForms/bin/Debug/net9.0-windows/
        var appPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "ControleFinanceiroForms", "bin", "Debug", "net9.0-windows", "ControleFinanceiroForms.exe"));
        
        if (!File.Exists(appPath))
        {
            Assert.Fail($"Application executable not found at: {appPath}");
        }

        // Launch app with the override flag
        App = Application.Launch(appPath, $"--e2e-db-path \"{_tempDbPath}\"");
        Automation = new UIA3Automation();
        
        // Wait for the main window to be available
        MainWindow = App.GetMainWindow(Automation, TimeSpan.FromSeconds(10));
        Assert.IsNotNull(MainWindow, "Main window did not appear within 10 seconds.");
    }

    [TestCleanup]
    public void Teardown()
    {
        if (App != null)
        {
            App.Close();
            App.Dispose();
        }

        if (Automation != null)
        {
            Automation.Dispose();
        }

        // Slight delay to allow processes to unlock the file
        Thread.Sleep(500);

        if (File.Exists(_tempDbPath))
        {
            try
            {
                File.Delete(_tempDbPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not delete temporary database at {_tempDbPath}. Error: {ex.Message}");
            }
        }
    }

    protected FlaUI.Core.AutomationElements.AutomationElement? FindElement(string idOrName, TimeSpan timeout = default)
    {
        if (timeout == default) timeout = TimeSpan.FromSeconds(5);
        return FlaUI.Core.Tools.Retry.WhileNull(
            () => MainWindow?.FindFirstDescendant(cf => cf.ByAutomationId(idOrName)) 
               ?? MainWindow?.FindFirstDescendant(cf => cf.ByName(idOrName)),
            timeout).Result;
    }
}
