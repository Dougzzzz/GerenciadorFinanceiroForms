using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using ControleFinanceiroForms.Features.Investments;
using ControleFinanceiroForms.Features.ImportTransactions;
using ControleFinanceiroForms.Features.Categories;
using ControleFinanceiroForms.Features.Contas;
using ControleFinanceiroForms.Features.Dashboard;
using ControleFinanceiroForms.Features.Parcelamentos;

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
    private readonly ContasViewModel _contasViewModel;
    private readonly ParcelamentosViewModel _parcelamentosViewModel;
    private readonly InvestmentsViewModel _investmentsViewModel;
    private readonly ControleFinanceiroForms.Features.Transacoes.TransacoesViewModel _transacoesViewModel;
    private bool _initialized;

    public MainWindow(
        InvestmentsViewModel investmentsViewModel, 
        ImportTransactionsViewModel importViewModel, 
        CategoriesViewModel categoriesViewModel,
        ContasViewModel contasViewModel,
        DashboardViewModel dashboardViewModel,
        ParcelamentosViewModel parcelamentosViewModel,
        ControleFinanceiroForms.Features.Transacoes.TransacoesViewModel transacoesViewModel)
    {
        InitializeComponent();

        _investmentsViewModel = investmentsViewModel;
        _importViewModel = importViewModel;
        _categoriesViewModel = categoriesViewModel;
        _contasViewModel = contasViewModel;
        _dashboardViewModel = dashboardViewModel;
        _parcelamentosViewModel = parcelamentosViewModel;
        _transacoesViewModel = transacoesViewModel;

        InvestmentsViewControl.DataContext = investmentsViewModel;
        ImportTransactionsViewControl.DataContext = importViewModel;
        CategoriesViewControl.DataContext = categoriesViewModel;
        ContasViewControl.DataContext = contasViewModel;
        DashboardViewControl.DataContext = dashboardViewModel;
        ParcelamentosViewControl.DataContext = parcelamentosViewModel;
        TransacoesViewControl.DataContext = transacoesViewModel;

        // Load default dashboard on startup (review-002 issue 003: guard against double-load)
        Loaded += async (s, e) =>
        {
            if (!_initialized)
            {
                _initialized = true;
                if (_dashboardViewModel.LoadDashboardCommand.CanExecute(null))
                {
                    await _dashboardViewModel.LoadDashboardCommand.ExecuteAsync(null);
                }
            }
        };
    }

    private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Skip events fired during initial render (review-002 issue 003)
        if (!_initialized) return;

        if (e.Source is TabControl tabControl)
        {
            switch (tabControl.SelectedIndex)
            {
                case 0:
                    if (_dashboardViewModel.LoadDashboardCommand.CanExecute(null))
                        await _dashboardViewModel.LoadDashboardCommand.ExecuteAsync(null);
                    break;
                case 1:
                    if (_transacoesViewModel.LoadTransacoesCommand.CanExecute(null))
                        await _transacoesViewModel.LoadTransacoesCommand.ExecuteAsync(null);
                    break;
                case 2:
                    if (_categoriesViewModel.LoadCategoriesCommand.CanExecute(null))
                        await _categoriesViewModel.LoadCategoriesCommand.ExecuteAsync(null);
                    break;
                case 3:
                    if (_contasViewModel.LoadContasCommand.CanExecute(null))
                        await _contasViewModel.LoadContasCommand.ExecuteAsync(null);
                    break;
                case 4:
                    if (_importViewModel.LoadInitialDataCommand.CanExecute(null))
                        await _importViewModel.LoadInitialDataCommand.ExecuteAsync(null);
                    break;
                case 5:
                    if (_parcelamentosViewModel.LoadParcelamentosCommand.CanExecute(null))
                        await _parcelamentosViewModel.LoadParcelamentosCommand.ExecuteAsync(null);
                    break;
                case 6:
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

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        App.ToggleTheme();
    }
}