using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using ControleFinanceiroForms.Features.Investments;
using ControleFinanceiroForms.Features.ImportTransactions;
using ControleFinanceiroForms.Features.Categories;
using ControleFinanceiroForms.Features.Dashboard;

namespace ControleFinanceiroForms;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly CategoriesViewModel _categoriesViewModel;
    private readonly ImportTransactionsViewModel _importViewModel;
    private readonly InvestmentsViewModel _investmentsViewModel;

    public MainWindow(
        InvestmentsViewModel investmentsViewModel, 
        ImportTransactionsViewModel importViewModel, 
        CategoriesViewModel categoriesViewModel,
        DashboardViewModel dashboardViewModel)
    {
        InitializeComponent();

        _investmentsViewModel = investmentsViewModel;
        _importViewModel = importViewModel;
        _categoriesViewModel = categoriesViewModel;
        _dashboardViewModel = dashboardViewModel;

        InvestmentsViewControl.DataContext = investmentsViewModel;
        ImportTransactionsViewControl.DataContext = importViewModel;
        CategoriesViewControl.DataContext = categoriesViewModel;
        DashboardViewControl.DataContext = dashboardViewModel;

        // Load default dashboard on startup
        Loaded += async (s, e) =>
        {
            if (_dashboardViewModel.LoadDashboardCommand.CanExecute(null))
            {
                await _dashboardViewModel.LoadDashboardCommand.ExecuteAsync(null);
            }
        };
    }

    private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl tabControl)
        {
            switch (tabControl.SelectedIndex)
            {
                case 0:
                    if (_dashboardViewModel.LoadDashboardCommand.CanExecute(null))
                        await _dashboardViewModel.LoadDashboardCommand.ExecuteAsync(null);
                    break;
                case 1:
                    if (_categoriesViewModel.LoadCategoriesCommand.CanExecute(null))
                        await _categoriesViewModel.LoadCategoriesCommand.ExecuteAsync(null);
                    break;
                case 2:
                    if (_importViewModel.LoadCategoriesCommand.CanExecute(null))
                        await _importViewModel.LoadCategoriesCommand.ExecuteAsync(null);
                    break;
                case 3:
                    if (_investmentsViewModel.LoadInvestmentsCommand.CanExecute(null))
                        await _investmentsViewModel.LoadInvestmentsCommand.ExecuteAsync(null);
                    break;
            }
        }
    }

    private void OpenLogsButton_Click(object sender, RoutedEventArgs e)
    {
        var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsPath); // ensure it exists
        Process.Start(new ProcessStartInfo
        {
            FileName = logsPath,
            UseShellExecute = true
        });
    }
}