using System.Windows.Controls;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.Transacoes;

public partial class TransacoesView : UserControl
{
    public TransacoesView()
    {
        InitializeComponent();
    }

    private async void DataGrid_RowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (DataContext is not TransacoesViewModel vm) return;
        if (e.Row.Item is not Transacao transacao) return;

        var grid = sender as DataGrid;
        if (grid != null)
        {
            grid.RowEditEnding -= DataGrid_RowEditEnding;
            grid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: false);
            grid.RowEditEnding += DataGrid_RowEditEnding;
        }

        if (vm.UpdateTransacaoCommand.CanExecute(transacao))
            await vm.UpdateTransacaoCommand.ExecuteAsync(transacao);
    }
}
