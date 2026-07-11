using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControleFinanceiroForms.Features.Dashboard;
using ControleFinanceiroForms.Tests.Fakes;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class DashboardViewModelTests
{
    private FakeCategoryRepository _categoryRepository = null!;
    private FakeTransactionRepository _transactionRepository = null!;
    private DashboardViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _categoryRepository = new FakeCategoryRepository();
        _transactionRepository = new FakeTransactionRepository();
        _viewModel = new DashboardViewModel(_categoryRepository, _transactionRepository);
    }

    [TestMethod]
    public async Task LoadDashboardAsync_CorrectlyAggregatesTransactionsAndCalculatesBudgets()
    {
        // Arrange
        // 1. Criar Categorias com limites
        var catComida = new Categoria { Id = Guid.NewGuid(), Name = "Comida", BudgetLimit = 500m };
        var catLazer = new Categoria { Id = Guid.NewGuid(), Name = "Lazer", BudgetLimit = 200m };
        await _categoryRepository.AddAsync(catComida);
        await _categoryRepository.AddAsync(catLazer);

        var today = DateTime.Today;
        var otherMonth = today.AddMonths(-1);

        // 2. Criar 3 transações para "Comida" no mês atual (incluindo offsets positivo/negativo)
        // Gasto 1: -100
        // Gasto 2: -150
        // Reembolso (Offset positivo): +50
        // Líquido esperado: -200 (Gasto real = 200)
        var tx1 = Transacao.Create(today, "Supermercado A", -100m, catComida.Id);
        var tx2 = Transacao.Create(today, "Restaurante B", -150m, catComida.Id);
        var tx3 = Transacao.Create(today, "Reembolso Comida", 50m, catComida.Id);

        // 3. Transação para "Lazer" no mês atual (-50)
        var txLazer = Transacao.Create(today, "Cinema", -50m, catLazer.Id);

        // 4. Transação sem categoria no mês atual (-80)
        var txUncategorized = Transacao.Create(today, "Avulso", -80m, null);

        // 5. Transação no mês anterior (deve ser ignorada no cálculo mensal)
        var txOtherMonth = Transacao.Create(otherMonth, "Compra Antiga", -300m, catComida.Id);

        await _transactionRepository.AddAsync(tx1);
        await _transactionRepository.AddAsync(tx2);
        await _transactionRepository.AddAsync(tx3);
        await _transactionRepository.AddAsync(txLazer);
        await _transactionRepository.AddAsync(txUncategorized);
        await _transactionRepository.AddAsync(txOtherMonth);

        // Act
        await _viewModel.LoadDashboardAsync();

        // Assert
        // Total Orçado: Comida (500) + Lazer (200) = 700
        Assert.AreEqual(700m, _viewModel.TotalBudget);

        // Total Gasto: Comida (200) + Lazer (50) + Sem Categoria (80) = 330
        Assert.AreEqual(330m, _viewModel.TotalSpent);

        // Saldo Restante: 700 - 330 = 370
        Assert.AreEqual(370m, _viewModel.RemainingBudget);

        // Progresso Geral: (330 / 700) * 100 = 47.14% (aprox 47.14)
        Assert.AreEqual(47.14, Math.Round(_viewModel.GeneralProgress, 2));
        Assert.IsFalse(_viewModel.IsGeneralOverBudget);

        // Verificar os itens detalhados por categoria
        Assert.AreEqual(3, _viewModel.DashboardItems.Count, "Deve incluir Comida, Lazer e Sem Categoria.");

        var comidaItem = _viewModel.DashboardItems.First(i => i.CategoryName == "Comida");
        Assert.AreEqual(200m, comidaItem.ActualSpent);
        Assert.AreEqual(500m, comidaItem.BudgetLimit);
        Assert.AreEqual(40.0, comidaItem.ProgressPercentage);
        Assert.IsFalse(comidaItem.IsOverBudget);

        var lazerItem = _viewModel.DashboardItems.First(i => i.CategoryName == "Lazer");
        Assert.AreEqual(50m, lazerItem.ActualSpent);
        Assert.AreEqual(200m, lazerItem.BudgetLimit);
        Assert.AreEqual(25.0, lazerItem.ProgressPercentage);
        Assert.IsFalse(lazerItem.IsOverBudget);

        var semCatItem = _viewModel.DashboardItems.First(i => i.CategoryName == "Sem Categoria");
        Assert.AreEqual(80m, semCatItem.ActualSpent);
        Assert.AreEqual(0m, semCatItem.BudgetLimit);
        Assert.AreEqual(0.0, semCatItem.ProgressPercentage);
    }

    [TestMethod]
    public async Task LoadDashboardAsync_OverBudget_SetsIsOverBudgetFlagsCorrectly()
    {
        // Arrange
        var cat = new Categoria { Id = Guid.NewGuid(), Name = "Festa", BudgetLimit = 100m };
        await _categoryRepository.AddAsync(cat);

        // Gasto excede o limite (150 > 100)
        var tx = Transacao.Create(DateTime.Today, "Balada", -150m, cat.Id);
        await _transactionRepository.AddAsync(tx);

        // Act
        await _viewModel.LoadDashboardAsync();

        // Assert
        Assert.AreEqual(100m, _viewModel.TotalBudget);
        Assert.AreEqual(150m, _viewModel.TotalSpent);
        Assert.AreEqual(-50m, _viewModel.RemainingBudget);
        Assert.IsTrue(_viewModel.IsGeneralOverBudget);

        var item = _viewModel.DashboardItems.First();
        Assert.IsTrue(item.IsOverBudget);
        Assert.AreEqual("#e02020", item.StatusColor);
    }

    // ─────────────────────────────────────────────────────────────
    // Regression: Bug 3 — Dashboard transaction grids must be populated
    // ─────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadDashboardAsync_PopulatesCreditAndCheckingTransactionCollections()
    {
        // Arrange
        var cat = new Categoria { Id = Guid.NewGuid(), Name = "Geral", BudgetLimit = 1000m };
        await _categoryRepository.AddAsync(cat);

        var today = DateTime.Today;

        // Checking transaction
        var txChecking = Transacao.Create(today, "Salário", 5000m, cat.Id, AccountType.Checking);
        // Credit card transaction
        var txCredit = Transacao.Create(today, "Compra Online", -200m, cat.Id, AccountType.CreditCard);

        await _transactionRepository.AddAsync(txChecking);
        await _transactionRepository.AddAsync(txCredit);

        // Act
        await _viewModel.LoadDashboardAsync();

        // Assert
        Assert.AreEqual(1, _viewModel.CheckingTransactions.Count,
            "Checking transactions collection must be populated.");
        Assert.AreEqual(1, _viewModel.CreditTransactions.Count,
            "Credit transactions collection must be populated.");

        Assert.AreEqual("Salário", _viewModel.CheckingTransactions[0].Description);
        Assert.AreEqual("Compra Online", _viewModel.CreditTransactions[0].Description);
    }
}
