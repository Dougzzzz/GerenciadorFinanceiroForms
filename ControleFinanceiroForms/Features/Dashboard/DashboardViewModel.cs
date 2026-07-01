using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.Dashboard;

public class DashboardItem
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetLimit { get; set; }
    public decimal ActualSpent { get; set; }
    public double ProgressPercentage => BudgetLimit > 0 ? Math.Min((double)(ActualSpent / BudgetLimit) * 100, 100) : 0;
    public bool IsOverBudget => BudgetLimit > 0 && ActualSpent > BudgetLimit;
    public string StatusColor => IsOverBudget ? "#e02020" : "#7132f5"; // Vermelho vs Roxo Kraken
    public string FormattedProgress => BudgetLimit > 0 
        ? $"R$ {ActualSpent:N2} de R$ {BudgetLimit:N2}"
        : $"R$ {ActualSpent:N2} (Sem limite)";
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;

    [ObservableProperty]
    private decimal _totalBudget;

    [ObservableProperty]
    private decimal _totalSpent;

    [ObservableProperty]
    private decimal _remainingBudget;

    [ObservableProperty]
    private double _generalProgress;

    [ObservableProperty]
    private bool _isGeneralOverBudget;

    public ObservableCollection<DashboardItem> DashboardItems { get; } = new();

    public DashboardViewModel(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository)
    {
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        var today = DateTime.Today;
        var currentMonth = today.Month;
        var currentYear = today.Year;

        // Filtrar transações no banco de dados (review-002 issue 004)
        var currentTransactions = await _transactionRepository.GetByMonthAsync(currentMonth, currentYear);

        var items = new List<DashboardItem>();
        decimal totalBudget = 0;
        decimal totalSpent = 0;

        foreach (var category in categories)
        {
            var categoryTransactions = currentTransactions
                .Where(t => t.CategoryId == category.Id)
                .ToList();

            // Soma das despesas como offsets positivos e negativos:
            // Lançamentos negativos são despesas, positivos são créditos/offsets.
            // Para exibição de gastos, o saldo líquido negativo é convertido em valor positivo (gasto).
            var netAmount = categoryTransactions.Sum(t => t.Amount);
            var spent = netAmount < 0 ? -netAmount : 0;

            var limit = category.BudgetLimit ?? 0;
            if (category.BudgetLimit.HasValue)
            {
                totalBudget += category.BudgetLimit.Value;
            }

            totalSpent += spent;

            items.Add(new DashboardItem
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                BudgetLimit = limit,
                ActualSpent = spent
            });
        }

        // Agrupar transações sem categoria (CategoryId == null) que são despesas (netAmount < 0)
        var uncategorizedTransactions = currentTransactions
            .Where(t => t.CategoryId == null)
            .ToList();

        if (uncategorizedTransactions.Any())
        {
            var netAmount = uncategorizedTransactions.Sum(t => t.Amount);
            var spent = netAmount < 0 ? -netAmount : 0;

            if (spent > 0)
            {
                totalSpent += spent;
                items.Add(new DashboardItem
                {
                    CategoryId = Guid.Empty,
                    CategoryName = "Sem Categoria",
                    BudgetLimit = 0,
                    ActualSpent = spent
                });
            }
        }

        DashboardItems.Clear();
        foreach (var item in items.OrderByDescending(i => i.ActualSpent))
        {
            DashboardItems.Add(item);
        }

        TotalBudget = totalBudget;
        TotalSpent = totalSpent;
        RemainingBudget = totalBudget - totalSpent;
        GeneralProgress = totalBudget > 0 ? Math.Min((double)(totalSpent / totalBudget) * 100, 100) : 0;
        IsGeneralOverBudget = totalBudget > 0 && totalSpent > totalBudget;
    }
}
