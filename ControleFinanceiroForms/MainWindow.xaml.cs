using System.Diagnostics.CodeAnalysis;
using System.Windows;
using ControleFinanceiroForms.Features.Categories;

namespace ControleFinanceiroForms;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    public MainWindow(CategoriesViewModel categoriesViewModel)
    {
        InitializeComponent();
        
        // Load data on startup
        Loaded += async (s, e) =>
        {
            if (categoriesViewModel.LoadCategoriesCommand.CanExecute(null))
            {
                await categoriesViewModel.LoadCategoriesCommand.ExecuteAsync(null);
            }
        };

        CategoriesViewControl.DataContext = categoriesViewModel;
    }
}