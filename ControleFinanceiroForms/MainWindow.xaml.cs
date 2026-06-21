using System.Diagnostics.CodeAnalysis;
using System.Windows;
using ControleFinanceiroForms.Features.ImportTransactions;

namespace ControleFinanceiroForms;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    public MainWindow(ImportTransactionsViewModel viewModel)
    {
        InitializeComponent();
        
        // Load data on startup
        Loaded += async (s, e) =>
        {
            if (viewModel.LoadCategoriesCommand.CanExecute(null))
            {
                await viewModel.LoadCategoriesCommand.ExecuteAsync(null);
            }
        };

        ImportTransactionsViewControl.DataContext = viewModel;
    }
}