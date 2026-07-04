using System.Windows.Controls;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.Contas;

public partial class ContasView : UserControl
{
    public ContasView()
    {
        InitializeComponent();
    }

    private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            var viewModel = DataContext as ContasViewModel;
            var conta = e.Row.Item as Conta;
            
            if (viewModel != null && conta != null)
            {
                // Execute update after the UI commit completes
                Dispatcher.BeginInvoke(new Action(() => 
                {
                    if (viewModel.UpdateContaCommand.CanExecute(conta))
                    {
                        viewModel.UpdateContaCommand.Execute(conta);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}
