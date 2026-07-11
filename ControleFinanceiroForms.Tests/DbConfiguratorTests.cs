using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControleFinanceiroForms.Data;
using System;
using System.IO;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class DbConfiguratorTests
{
    [TestMethod]
    public void GetDatabasePath_WithE2EArgument_ReturnsOverriddenPath()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var expectedPath = Path.Combine(tempDir, "test.db");
        var args = new string[] { "--some-arg", "--e2e-db-path", expectedPath };

        // Act
        var result = DbConfigurator.GetDatabasePath(args);

        // Assert
        Assert.AreEqual(expectedPath, result);
        Assert.IsTrue(Directory.Exists(tempDir));

        // Cleanup
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
    }

    [TestMethod]
    public void GetDatabasePath_WithoutArgument_ReturnsDefaultPath()
    {
        // Arrange
        var args = new string[] { "--some-arg" };
        var defaultFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ControleFinanceiro");
        var expectedPath = Path.Combine(defaultFolder, "controle-financeiro.db");

        // Act
        var result = DbConfigurator.GetDatabasePath(args);

        // Assert
        Assert.AreEqual(expectedPath, result);
        Assert.IsTrue(Directory.Exists(defaultFolder));
    }

    [TestMethod]
    public void GetDatabasePath_ArgumentPresentButNoValue_ReturnsDefaultPath()
    {
        // Arrange
        // The flag is the last argument, so there is no value following it
        var args = new string[] { "--some-arg", "--e2e-db-path" };
        var defaultFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ControleFinanceiro");
        var expectedPath = Path.Combine(defaultFolder, "controle-financeiro.db");

        // Act
        var result = DbConfigurator.GetDatabasePath(args);

        // Assert
        Assert.AreEqual(expectedPath, result);
    }
}
