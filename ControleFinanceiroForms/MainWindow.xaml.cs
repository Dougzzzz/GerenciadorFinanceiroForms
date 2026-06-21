using System.Diagnostics.CodeAnalysis;
using System.Windows;
using ControleFinanceiroForms.Features.Investments;
using ControleFinanceiroForms.Features.Categories;

namespace ControleFinanceiroForms;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    public MainWindow(InvestmentsViewModel investmentsViewModel, CategoriesViewModel categoriesViewModel)
    {
        InitializeComponent();
        
        // Load data on startup
        Loaded += async (s, e) =>
        {
            if (investmentsViewModel.LoadInvestmentsCommand.CanExecute(null))
            {
                await investmentsViewModel.LoadInvestmentsCommand.ExecuteAsync(null);
            }
            if (categoriesViewModel.LoadCategoriesCommand.CanExecute(null))
            {
                await categoriesViewModel.LoadCategoriesCommand.ExecuteAsync(null);
            }
        };

        InvestmentsViewControl.DataContext = investmentsViewModel;
        CategoriesViewControl.DataContext = categoriesViewModel;
    }
}