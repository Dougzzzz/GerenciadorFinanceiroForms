using System.Diagnostics.CodeAnalysis;
using System.Windows;
using ControleFinanceiroForms.Features.Investments;

namespace ControleFinanceiroForms;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    public MainWindow(InvestmentsViewModel viewModel)
    {
        InitializeComponent();
        
        // Load data on startup
        Loaded += async (s, e) =>
        {
            if (viewModel.LoadInvestmentsCommand.CanExecute(null))
            {
                await viewModel.LoadInvestmentsCommand.ExecuteAsync(null);
            }
        };

        InvestmentsViewControl.DataContext = viewModel;
    }
}