using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControleFinanceiroForms;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class ThemeTests
{
    [TestMethod]
    public void ToggleTheme_ChangesIsDarkModeProperty()
    {
        // Skip or run conditionally if Application.Current is null
        // because WPF Application context might not be available in standard test runner.
        if (Application.Current == null)
        {
            Assert.Inconclusive("WPF Application context is not available during this test.");
            return;
        }

        var initialState = App.IsDarkMode;
        
        App.ToggleTheme();
        
        var newState = App.IsDarkMode;

        Assert.AreNotEqual(initialState, newState);
        
        // Restore
        App.ToggleTheme();
    }
}
