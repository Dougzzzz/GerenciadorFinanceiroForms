using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;
using ControleFinanceiroForms.Features.Transacoes;
using ControleFinanceiroForms.Tests.Fakes;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class TransacoesViewModelTests
{
    private FakeTransactionRepository _transactionRepo = null!;
    private FakeCategoryRepository _categoryRepo = null!;
    private TransacoesViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _transactionRepo = new FakeTransactionRepository();
        _categoryRepo = new FakeCategoryRepository();
        _viewModel = new TransacoesViewModel(_transactionRepo, _categoryRepo);
    }

    // ─────────────────────────────────────────────────────────────
    // Regression: Bug 1 — Categories must appear for transaction editing
    // ─────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadTransacoesAsync_PopulatesCategoriesCollection()
    {
        // Arrange
        var cat1 = new Categoria { Id = Guid.NewGuid(), Name = "Alimentação", BudgetLimit = 500m };
        var cat2 = new Categoria { Id = Guid.NewGuid(), Name = "Transporte", BudgetLimit = 200m };
        await _categoryRepo.AddAsync(cat1);
        await _categoryRepo.AddAsync(cat2);

        // Act
        await _viewModel.LoadTransacoesCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(2, _viewModel.Categories.Count, "Categories must be loaded for editing.");
        Assert.IsTrue(_viewModel.Categories.Any(c => c.Name == "Alimentação"));
        Assert.IsTrue(_viewModel.Categories.Any(c => c.Name == "Transporte"));
    }

    [TestMethod]
    public async Task LoadTransacoesAsync_PopulatesTransacoesForCurrentMonth()
    {
        var today = DateTime.Today;
        _viewModel.StartDate = new DateTime(today.Year, today.Month, 1);
        _viewModel.EndDate = _viewModel.StartDate.AddMonths(1).AddDays(-1);

        var tx1 = Transacao.Create(today, "Supermercado", -100m);
        var tx2 = Transacao.Create(today, "Restaurante", -50m);
        var txOtherMonth = Transacao.Create(today.AddMonths(-1), "Compra Antiga", -300m);

        await _transactionRepo.AddAsync(tx1);
        await _transactionRepo.AddAsync(tx2);
        await _transactionRepo.AddAsync(txOtherMonth);

        // Act
        await _viewModel.LoadTransacoesCommand.ExecuteAsync(null);

        // Assert — only current month transactions should appear
        Assert.AreEqual(2, _viewModel.Transacoes.Count, "Only current month transactions should be loaded.");
    }

    // ─────────────────────────────────────────────────────────────
    // Regression: Bug 4 — Deleting transaction must not throw FK error
    // ─────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task DeleteTransacaoAsync_RemovesFromCollectionAndRepository()
    {
        // Arrange
        var today = DateTime.Today;
        var cat = new Categoria { Id = Guid.NewGuid(), Name = "Lazer" };
        await _categoryRepo.AddAsync(cat);

        var tx = Transacao.Create(today, "Cinema", -30m, cat.Id);
        await _transactionRepo.AddAsync(tx);
        _viewModel.Transacoes.Add(tx);

        // Act
        await _viewModel.DeleteTransacaoCommand.ExecuteAsync(tx);

        // Assert
        Assert.AreEqual(0, _viewModel.Transacoes.Count, "Transaction should be removed from the collection.");
        Assert.AreEqual(0, _transactionRepo.Count, "Transaction should be removed from the repository.");
        Assert.IsNull(_viewModel.ErrorMessage, "No error should be set on successful delete.");
    }

    [TestMethod]
    public async Task DeleteTransacaoAsync_NullTransacao_DoesNothing()
    {
        // Act
        await _viewModel.DeleteTransacaoCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task DeleteTransacaoAsync_RepositoryThrows_SetsErrorMessage()
    {
        // Arrange
        var faultyTransRepo = new FaultyTransactionRepository();
        var vm = new TransacoesViewModel(faultyTransRepo, _categoryRepo);
        var tx = Transacao.Create(DateTime.Today, "Test", -10m);
        vm.Transacoes.Add(tx);

        // Act
        await vm.DeleteTransacaoCommand.ExecuteAsync(tx);

        // Assert — should set error, not crash
        Assert.IsNotNull(vm.ErrorMessage);
        Assert.IsTrue(vm.ErrorMessage.Contains("Erro ao excluir"));
        // Transaction stays in collection because delete failed
        Assert.AreEqual(1, vm.Transacoes.Count);
    }

    // ─────────────────────────────────────────────────────────────
    // Regression: Update category on a transaction
    // ─────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task UpdateTransacaoAsync_UpdatesCategoryInRepository()
    {
        // Arrange
        var cat = new Categoria { Id = Guid.NewGuid(), Name = "Alimentação" };
        await _categoryRepo.AddAsync(cat);

        var tx = Transacao.Create(DateTime.Today, "Compra", -50m);
        await _transactionRepo.AddAsync(tx);

        // Act — assign category
        tx.CategoryId = cat.Id;
        await _viewModel.UpdateTransacaoCommand.ExecuteAsync(tx);

        // Assert
        var updated = await _transactionRepo.GetByIdAsync(tx.Id);
        Assert.IsNotNull(updated);
        Assert.AreEqual(cat.Id, updated.CategoryId);
        Assert.IsNull(_viewModel.ErrorMessage);
    }

    // ─────────────────────────────────────────────────────────────
    // Test helper — faulty repository for error path
    // ─────────────────────────────────────────────────────────────

    private class FaultyTransactionRepository : ITransactionRepository
    {
        public Task<IEnumerable<Transacao>> GetAllAsync() => Task.FromResult(Enumerable.Empty<Transacao>());
        public Task<Transacao?> GetByIdAsync(Guid id) => Task.FromResult<Transacao?>(null);
        public Task AddAsync(Transacao transaction) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<Transacao> transactions) => Task.CompletedTask;
        public Task UpdateAsync(Transacao transaction) => throw new InvalidOperationException("FK constraint failed");
        public Task DeleteAsync(Guid id) => throw new InvalidOperationException("FOREIGN KEY constraint failed");
        public Task<IEnumerable<string>> GetExistingHashesAsync(IEnumerable<string> hashes) => Task.FromResult(Enumerable.Empty<string>());
        public Task<IEnumerable<Transacao>> GetByMonthAsync(int month, int year) => Task.FromResult(Enumerable.Empty<Transacao>());
        public Task<IEnumerable<Transacao>> GetByDateRangeAsync(DateTime start, DateTime end) => Task.FromResult(Enumerable.Empty<Transacao>());
    }
}
