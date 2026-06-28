using System.Windows.Controls;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.Categories;

public partial class CategoriesView : UserControl
{
    public CategoriesView()
    {
        InitializeComponent();
    }

    private async void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (DataContext is not CategoriesViewModel vm) return;
        if (e.Row.Item is not Categoria categoria) return;

        // Commit the edit to the binding source before persisting
        (sender as DataGrid)?.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: false);

        if (vm.UpdateCategoryCommand.CanExecute(categoria))
            await vm.UpdateCategoryCommand.ExecuteAsync(categoria);
    }
}
