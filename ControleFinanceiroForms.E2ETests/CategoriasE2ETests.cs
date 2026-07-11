using System.Threading;
using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ControleFinanceiroForms.E2ETests;

[TestClass]
[TestCategory("E2E")]
public class CategoriasE2ETests : E2ETestBase
{
    [TestMethod]
    public void AddCategoria_ApareceNoGrid()
    {
        var mainTab = FindElement("MainTabControl")?.AsTab();
        Assert.IsNotNull(mainTab, "MainTabControl não encontrado.");
        mainTab.SelectTabItem(1); // Categorias
        System.Threading.Thread.Sleep(1000);

        var txtNome = FindElement("TxtNovaCategoriaNome")?.AsTextBox();
        Assert.IsNotNull(txtNome, "TextBox de nome não encontrado.");
        txtNome.Text = "Categoria Teste E2E";

        var txtLimite = FindElement("TxtNovaCategoriaLimite")?.AsTextBox();
        Assert.IsNotNull(txtLimite, "TextBox de limite não encontrado.");
        txtLimite.Text = "500";

        var btnAdicionar = FindElement("BtnAdicionarCategoria")?.AsButton();
        Assert.IsNotNull(btnAdicionar, "Botão adicionar categoria não encontrado.");
        btnAdicionar.Invoke();

        Thread.Sleep(500);

        var grid = FindElement("DgCategorias")?.AsDataGridView();
        Assert.IsNotNull(grid, "DataGrid de Categorias não encontrado.");
        
        bool found = false;
        FlaUI.Core.Tools.Retry.WhileFalse(() =>
        {
            foreach (var row in grid.Rows)
            {
                var cellNome = row.Cells[0]?.AsTextBox()?.Text ?? row.Cells[0]?.AsLabel()?.Text ?? row.Cells[0]?.Name;
                if (cellNome != null && cellNome.Contains("Categoria Teste E2E"))
                {
                    found = true;
                    return true;
                }
            }
            return false;
        }, System.TimeSpan.FromSeconds(3));

        Assert.IsTrue(found, "A categoria recém adicionada não foi encontrada no DataGrid.");
    }
}
