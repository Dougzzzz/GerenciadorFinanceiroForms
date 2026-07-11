using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ControleFinanceiroForms.E2ETests;

[TestClass]
public class TransacoesE2ETests : E2ETestBase
{
    [TestMethod]
    public void AddTransacaoManual_ApareceNoGrid()
    {
        // 1. Navegar para a aba Transações
        var mainTab = FindElement("MainTabControl")?.AsTab();
        Assert.IsNotNull(mainTab, "MainTabControl não encontrado.");
        mainTab.SelectTabItem(2); // Transações
        System.Threading.Thread.Sleep(1000);

        // 2. Clicar no botão "+ Nova Transação"
        var btnNova = FindElement("BtnNovaTransacao")?.AsButton();
        Assert.IsNotNull(btnNova, "Botão Nova Transação não encontrado.");
        btnNova.Invoke();

        // 3. Preencher formulário
        var txtDesc = FindElement("TxtDescricao")?.AsTextBox();
        Assert.IsNotNull(txtDesc, "Campo de descrição não encontrado.");
        txtDesc.Text = "Compra de Teste E2E";

        var txtValor = FindElement("TxtValor")?.AsTextBox();
        Assert.IsNotNull(txtValor, "Campo de valor não encontrado.");
        txtValor.Text = "120,50";

        var btnSalvar = FindElement("BtnSalvarTransacao")?.AsButton();
        Assert.IsNotNull(btnSalvar, "Botão salvar não encontrado.");
        btnSalvar.Invoke();

        // 4. Verificar se a transação aparece no DataGrid
        var grid = FindElement("DgTransacoes")?.AsDataGridView();
        Assert.IsNotNull(grid, "DataGrid de Transações não encontrado.");
        
        // Retry grid loading
        bool found = false;
        FlaUI.Core.Tools.Retry.WhileFalse(() =>
        {
            foreach (var row in grid.Rows)
            {
                var cellDesc = row.Cells[1]?.AsLabel()?.Text;
                if (cellDesc == "Compra de Teste E2E")
                {
                    found = true;
                    return true;
                }
            }
            return false;
        }, System.TimeSpan.FromSeconds(3));

        Assert.IsTrue(found, "A transação recém adicionada não foi encontrada no DataGrid.");
    }
}
