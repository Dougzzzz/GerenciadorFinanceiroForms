using System.Windows.Controls;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.Transacoes;

public partial class TransacoesView : UserControl
{
    public TransacoesView()
    {
        InitializeComponent();
    }

    private async void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            if (e.Row.Item is Transacao transacao && DataContext is TransacoesViewModel viewModel)
            {
                // Offload to background to let grid finish commit
                await Dispatcher.InvokeAsync(async () =>
                {
                    await viewModel.UpdateTransacaoAsync(transacao);
                });
            }
        }
    }
}
