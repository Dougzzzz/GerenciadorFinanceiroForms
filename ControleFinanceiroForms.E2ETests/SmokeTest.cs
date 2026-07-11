using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ControleFinanceiroForms.E2ETests;

[TestClass]
[TestCategory("E2E")]
public class SmokeTest : E2ETestBase
{
    [TestMethod]
    public void AppLaunches_AndMainWindowAppears()
    {
        // Assert
        Assert.IsNotNull(App);
        Assert.IsNotNull(MainWindow);
        Assert.AreEqual("Controle Financeiro", MainWindow.Title);
    }
}
