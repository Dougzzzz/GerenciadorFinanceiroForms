using System.Threading;
using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ControleFinanceiroForms.E2ETests;

[TestClass]
public class ParcelamentosE2ETests : E2ETestBase
{
    [TestMethod]
    public void AddParcelamentoEPagar_AtualizaGrid()
    {
        var mainTab = FindElement("MainTabControl")?.AsTab();
        Assert.IsNotNull(mainTab, "MainTabControl não encontrado.");
        mainTab.SelectTabItem(4); // Parcelamentos
        System.Threading.Thread.Sleep(1000);

        // 1. Criar novo Parcelamento
        var txtDesc = FindElement("TxtNovaDescricaoParcelamento")?.AsTextBox();
        Assert.IsNotNull(txtDesc);
        txtDesc.Text = "TV Nova E2E";

        var txtValorTotal = FindElement("TxtNovoValorParcelamento")?.AsTextBox();
        Assert.IsNotNull(txtValorTotal);
        txtValorTotal.Text = "2000";

        var txtParcelas = FindElement("TxtNovoNumeroParcelas")?.AsTextBox();
        Assert.IsNotNull(txtParcelas);
        txtParcelas.Text = "10";

        var btnAdd = FindElement("BtnAdicionarParcelamento")?.AsButton();
        Assert.IsNotNull(btnAdd);
        btnAdd.Invoke();

        Thread.Sleep(500);

        // 2. Selecionar o parcelamento recém criado no DataGrid
        var grid = FindElement("DgParcelamentos")?.AsDataGridView();
        Assert.IsNotNull(grid);
        
        DataGridViewRow? targetRow = FlaUI.Core.Tools.Retry.WhileNull(() =>
        {
            foreach (var row in grid.Rows)
            {
                var cellDesc = row.Cells[0]?.AsLabel()?.Text ?? row.Cells[0]?.Name;
                if (cellDesc != null && cellDesc.Contains("TV Nova E2E"))
                {
                    return row;
                }
            }
            return null;
        }, System.TimeSpan.FromSeconds(3)).Result;
        Assert.IsNotNull(targetRow, "Parcelamento não encontrado no grid.");
        targetRow.Patterns.SelectionItem.Pattern.Select(); // Ensure selection is processed by ViewModel

        Thread.Sleep(500);

        // 3. Fazer um pagamento
        var txtPagamento = FindElement("TxtNovoValorPagamento")?.AsTextBox();
        Assert.IsNotNull(txtPagamento, "TextBox de pagamento não encontrado. (Pode estar collapsed se a seleção falhou)");
        txtPagamento.Text = "200";

        var btnPagar = FindElement("BtnPagarParcelamento")?.AsButton();
        Assert.IsNotNull(btnPagar);
        btnPagar.Invoke();

        Thread.Sleep(500);

        // 4. Verificar se amortizou (Saldo Restante deve diminuir para 1800)
        // Recarregar row
        bool foundAmortization = false;
        FlaUI.Core.Tools.Retry.WhileFalse(() =>
        {
            foreach (var row in grid.Rows)
            {
                var cellDesc = row.Cells[0]?.AsLabel()?.Text ?? row.Cells[0]?.Name;
                if (cellDesc != null && cellDesc.Contains("TV Nova E2E"))
                {
                    var cellRestante = row.Cells[2]?.AsLabel()?.Text ?? row.Cells[2]?.Name;
                    if (cellRestante != null && (cellRestante.Contains("1800") || cellRestante.Contains("1.800")))
                    {
                        foundAmortization = true;
                        return true;
                    }
                }
            }
            return false;
        }, System.TimeSpan.FromSeconds(3));

        Assert.IsTrue(foundAmortization, "Amortização não refletiu no DataGrid.");
    }
}
