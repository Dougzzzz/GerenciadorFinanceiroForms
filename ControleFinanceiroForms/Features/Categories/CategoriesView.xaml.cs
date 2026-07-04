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

        var grid = sender as DataGrid;
        if (grid != null)
        {
            grid.RowEditEnding -= DataGrid_RowEditEnding;
            grid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: false);
            grid.RowEditEnding += DataGrid_RowEditEnding;
        }

        if (vm.UpdateCategoryCommand.CanExecute(categoria))
            await vm.UpdateCategoryCommand.ExecuteAsync(categoria);
    }
}
