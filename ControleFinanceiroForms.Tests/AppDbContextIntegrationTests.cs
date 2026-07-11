using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;
using ControleFinanceiroForms.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Tests;

/// <summary>
/// Integration tests that verify <see cref="AppDbContext"/> correctly
/// saves and retrieves entities using an in-memory SQLite connection.
/// No mocking libraries are used (ADR-004 compliance).
/// </summary>
[TestClass]
public sealed class AppDbContextIntegrationTests
{
    /// <summary>
    /// Creates a fresh <see cref="AppDbContext"/> backed by an isolated
    /// in-memory SQLite database for each test method.
    /// </summary>
    private static AppDbContext CreateInMemoryContext()
    {
        // Each call produces a unique in-memory database name to ensure test isolation.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated(); // apply EF model schema in-memory
        return context;
    }

    // ─────────────────────────────────────────────────────────────
    // Integration Tests
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Saving a Categoria and reading it back must return an entity with
    /// the same Id and Name.
    /// </summary>
    [TestMethod]
    public async Task SaveAndRetrieve_Categoria_ReturnsCorrectEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var categoria = new Categoria
        {
            Id = Guid.NewGuid(),
            Name = "Alimentação",
            BudgetLimit = 1500.00m
        };

        // Act
        await context.Categorias.AddAsync(categoria);
        await context.SaveChangesAsync();

        var retrieved = await context.Categorias.FindAsync(categoria.Id);

        // Assert
        Assert.IsNotNull(retrieved, "Category should be retrievable after save.");
        Assert.AreEqual(categoria.Name, retrieved.Name);
        Assert.AreEqual(categoria.BudgetLimit, retrieved.BudgetLimit);
    }

    /// <summary>
    /// Saving a Transacao and reading it back must return an entity with
    /// the same Id and ChaveExclusiva.
    /// </summary>
    [TestMethod]
    public async Task SaveAndRetrieve_Transacao_ReturnsCorrectEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var tx = Transacao.Create(new DateTime(2026, 5, 10), "Conta de luz", 320.50m);
        tx.Id = Guid.NewGuid();

        // Act
        await context.Transacoes.AddAsync(tx);
        await context.SaveChangesAsync();

        var retrieved = await context.Transacoes.FindAsync(tx.Id);

        // Assert
        Assert.IsNotNull(retrieved, "Transaction should be retrievable after save.");
        Assert.AreEqual(tx.ChaveExclusiva, retrieved.ChaveExclusiva,
            "ChaveExclusiva must be persisted and retrieved correctly.");
        Assert.AreEqual(tx.Amount, retrieved.Amount);
    }

    /// <summary>
    /// Saving a Transacao associated with a Categoria must allow navigation
    /// property loading via Include.
    /// </summary>
    [TestMethod]
    public async Task SaveAndRetrieve_Transacao_WithCategoria_ReturnsRelatedEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var categoria = new Categoria { Id = Guid.NewGuid(), Name = "Transporte" };
        var tx = Transacao.Create(DateTime.Today, "Uber", 25.00m, categoria.Id);
        tx.Id = Guid.NewGuid();

        // Act
        await context.Categorias.AddAsync(categoria);
        await context.Transacoes.AddAsync(tx);
        await context.SaveChangesAsync();

        var retrieved = await context.Transacoes
            .Include(t => t.Categoria)
            .FirstOrDefaultAsync(t => t.Id == tx.Id);

        // Assert
        Assert.IsNotNull(retrieved, "Transaction with category should be retrievable.");
        Assert.IsNotNull(retrieved.Categoria, "Navigation property 'Categoria' should be loaded.");
        Assert.AreEqual("Transporte", retrieved.Categoria.Name);
    }

    /// <summary>
    /// Saving an Investimento and reading it back must return an entity with
    /// the same Id and TotalValue.
    /// </summary>
    [TestMethod]
    public async Task SaveAndRetrieve_Investimento_ReturnsCorrectEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var investimento = new Investimento
        {
            Id = Guid.NewGuid(),
            RecordedAt = DateTime.UtcNow,
            TotalValue = 150_000.00m,
            Note = "Rebalanceamento janeiro",
            Conta = "XP Investimentos",
            TipoInvestimento = "Ações",
            TipoOperacao = OperacaoInvestimento.Aporte
        };

        // Act
        await context.Investimentos.AddAsync(investimento);
        await context.SaveChangesAsync();

        var retrieved = await context.Investimentos.FindAsync(investimento.Id);

        // Assert
        Assert.IsNotNull(retrieved, "Investimento should be retrievable after save.");
        Assert.AreEqual(investimento.TotalValue, retrieved.TotalValue);
        Assert.AreEqual(investimento.Note, retrieved.Note);
        Assert.AreEqual(investimento.Conta, retrieved.Conta);
        Assert.AreEqual(investimento.TipoInvestimento, retrieved.TipoInvestimento);
        Assert.AreEqual(investimento.TipoOperacao, retrieved.TipoOperacao);
    }

    [TestMethod]
    public async Task SaveAndUpdate_Investimento_UpdatesCorrectly()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repo = new InvestmentRepository(context);
        var investimento = new Investimento
        {
            Id = Guid.NewGuid(),
            RecordedAt = DateTime.UtcNow,
            TotalValue = 50_000.00m,
            Note = "Original Note",
            Conta = "Nubank",
            TipoInvestimento = "Renda Fixa",
            TipoOperacao = OperacaoInvestimento.SnapshotTotal
        };
        await repo.AddAsync(investimento);

        // Act
        investimento.TotalValue = 55_000.00m;
        investimento.Note = "Updated Note";
        investimento.Conta = "Inter";
        investimento.TipoInvestimento = "LCI";
        investimento.TipoOperacao = OperacaoInvestimento.Aporte;
        await repo.UpdateAsync(investimento);

        var retrieved = await repo.GetByIdAsync(investimento.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(55_000.00m, retrieved.TotalValue);
        Assert.AreEqual("Updated Note", retrieved.Note);
        Assert.AreEqual("Inter", retrieved.Conta);
        Assert.AreEqual("LCI", retrieved.TipoInvestimento);
        Assert.AreEqual(OperacaoInvestimento.Aporte, retrieved.TipoOperacao);
    }

    /// <summary>
    /// Attempting to save two Transacao with the same ChaveExclusiva
    /// must throw a DbUpdateException (unique index enforced at DB level).
    /// </summary>
    [TestMethod]
    public async Task Save_DuplicateChaveExclusiva_ThrowsDbUpdateException()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var tx1 = Transacao.Create(new DateTime(2026, 1, 1), "Duplicate", 10.00m);
        tx1.Id = Guid.NewGuid();

        // Create a second transaction with the same hash (manually override to simulate duplicate)
        var tx2 = Transacao.Create(new DateTime(2026, 1, 1), "Duplicate", 10.00m);
        tx2.Id = Guid.NewGuid();
        // tx2 already has the same ChaveExclusiva as tx1 (identical fields)

        // Act
        await context.Transacoes.AddAsync(tx1);
        await context.SaveChangesAsync();

        await context.Transacoes.AddAsync(tx2);

        // Assert
        await Assert.ThrowsExceptionAsync<DbUpdateException>(
            async () => await context.SaveChangesAsync(),
            "Saving a duplicate ChaveExclusiva must fail due to the unique index.");
    }

    [TestMethod]
    public async Task SaveAndRetrieve_Transacao_WithAccountType_ReturnsCorrectEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var tx = Transacao.Create(new DateTime(2026, 5, 10), "Conta de luz", 320.50m, null, AccountType.CreditCard);
        tx.Id = Guid.NewGuid();

        // Act
        await context.Transacoes.AddAsync(tx);
        await context.SaveChangesAsync();

        var retrieved = await context.Transacoes.FindAsync(tx.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(AccountType.CreditCard, retrieved.AccountType);
    }

    // ─────────────────────────────────────────────────────────────
    // Regression tests — Issue 003 (MetaGasto unique constraint)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempting to save two MetaGasto with the same (CategoryId, Month, Year)
    /// must throw a DbUpdateException.
    /// Regression guard for issue_003.
    /// </summary>
    [TestMethod]
    public async Task Save_DuplicateMetaGasto_SameCategoryMonthYear_ThrowsDbUpdateException()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var categoria = new Categoria { Id = Guid.NewGuid(), Name = "Alimentação" };
        await context.Categorias.AddAsync(categoria);
        await context.SaveChangesAsync();

        var meta1 = new MetaGasto
        {
            Id = Guid.NewGuid(),
            CategoryId = categoria.Id,
            Month = 6,
            Year = 2026,
            TargetAmount = 1000.00m
        };
        var meta2 = new MetaGasto
        {
            Id = Guid.NewGuid(),
            CategoryId = categoria.Id,
            Month = 6,    // same month
            Year = 2026,  // same year
            TargetAmount = 2000.00m
        };

        // Act
        await context.MetasGasto.AddAsync(meta1);
        await context.SaveChangesAsync();

        await context.MetasGasto.AddAsync(meta2);

        // Assert
        await Assert.ThrowsExceptionAsync<DbUpdateException>(
            async () => await context.SaveChangesAsync(),
            "Saving two MetaGasto with the same (CategoryId, Month, Year) must fail due to the unique index.");
    }

    // ─────────────────────────────────────────────────────────────
    // Regression tests — Issue 004 (FakeTransactionRepository uniqueness)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// FakeTransactionRepository.AddAsync must throw when a transaction with the
    /// same non-empty ChaveExclusiva is already stored.
    /// Regression guard for issue_004.
    /// </summary>
    [TestMethod]
    public async Task FakeRepo_AddAsync_DuplicateChaveExclusiva_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = new FakeTransactionRepository();
        var tx1 = Transacao.Create(new DateTime(2026, 1, 1), "Same Transaction", 50.00m);
        var tx2 = Transacao.Create(new DateTime(2026, 1, 1), "Same Transaction", 50.00m);

        await repo.AddAsync(tx1);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await repo.AddAsync(tx2),
            "FakeTransactionRepository must reject duplicate ChaveExclusiva like the real SQLite unique index.");
    }

    /// <summary>
    /// FakeTransactionRepository.AddRangeAsync must throw when any transaction in the
    /// batch has a ChaveExclusiva that already exists in the store.
    /// Regression guard for issue_004.
    /// </summary>
    [TestMethod]
    public async Task FakeRepo_AddRangeAsync_DuplicateChaveExclusiva_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = new FakeTransactionRepository();
        var tx1 = Transacao.Create(new DateTime(2026, 2, 1), "Batch Duplicate", 75.00m);
        await repo.AddAsync(tx1);

        var duplicateBatch = new[]
        {
            Transacao.Create(new DateTime(2026, 3, 1), "Unique in batch", 10.00m),
            Transacao.Create(new DateTime(2026, 2, 1), "Batch Duplicate", 75.00m) // duplicate
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await repo.AddRangeAsync(duplicateBatch),
            "FakeTransactionRepository.AddRangeAsync must detect and reject duplicates.");
    }

    [TestMethod]
    public async Task SaveAndRetrieve_Parcelamento_ReturnsCorrectEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var parcelamento = new Parcelamento
        {
            Id = Guid.NewGuid(),
            Descricao = "Notebook Gamer",
            ValorTotal = 6500.00m,
            NumeroParcelas = 10,
            DataInicio = new DateTime(2026, 6, 1),
            CriadoEm = DateTime.Now
        };

        // Act
        await context.Parcelamentos.AddAsync(parcelamento);
        await context.SaveChangesAsync();

        var retrieved = await context.Parcelamentos.FindAsync(parcelamento.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(parcelamento.Descricao, retrieved.Descricao);
        Assert.AreEqual(parcelamento.ValorTotal, retrieved.ValorTotal);
        Assert.AreEqual(parcelamento.NumeroParcelas, retrieved.NumeroParcelas);
        Assert.AreEqual(parcelamento.DataInicio, retrieved.DataInicio);
    }

    [TestMethod]
    public async Task SaveAndRetrieve_PagamentoParcelamento_ReturnsCorrectEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var parcelamento = new Parcelamento
        {
            Id = Guid.NewGuid(),
            Descricao = "Dívida Teste",
            ValorTotal = 1000.00m,
            DataInicio = DateTime.Today
        };
        var pagamento = new PagamentoParcelamento
        {
            Id = Guid.NewGuid(),
            ParcelamentoId = parcelamento.Id,
            ValorPago = 100.00m,
            DataPagamento = DateTime.Today,
            Nota = "Primeira parcela"
        };

        // Act
        await context.Parcelamentos.AddAsync(parcelamento);
        await context.PagamentosParcelamento.AddAsync(pagamento);
        await context.SaveChangesAsync();

        var retrieved = await context.PagamentosParcelamento.FindAsync(pagamento.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(pagamento.ParcelamentoId, retrieved.ParcelamentoId);
        Assert.AreEqual(pagamento.ValorPago, retrieved.ValorPago);
        Assert.AreEqual(pagamento.DataPagamento, retrieved.DataPagamento);
        Assert.AreEqual(pagamento.Nota, retrieved.Nota);
    }

    [TestMethod]
    public async Task Delete_Parcelamento_CascadeDeletesPagamentos()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repoParcelamento = new ParcelamentoRepository(context);
        var repoPagamento = new PagamentoParcelamentoRepository(context);

        var parcelamento = new Parcelamento
        {
            Id = Guid.NewGuid(),
            Descricao = "Dívida Cascade",
            ValorTotal = 1000.00m,
            DataInicio = DateTime.Today
        };
        await repoParcelamento.AddAsync(parcelamento);

        var pag1 = new PagamentoParcelamento
        {
            Id = Guid.NewGuid(),
            ParcelamentoId = parcelamento.Id,
            ValorPago = 100.00m,
            DataPagamento = DateTime.Today
        };
        var pag2 = new PagamentoParcelamento
        {
            Id = Guid.NewGuid(),
            ParcelamentoId = parcelamento.Id,
            ValorPago = 200.00m,
            DataPagamento = DateTime.Today
        };
        await repoPagamento.AddAsync(pag1);
        await repoPagamento.AddAsync(pag2);

        // Act
        await repoParcelamento.DeleteAsync(parcelamento.Id);

        var retrievedParcelamento = await repoParcelamento.GetByIdAsync(parcelamento.Id);
        var retrievedPagamentos = await repoPagamento.GetByParcelamentoIdAsync(parcelamento.Id);

        // Assert
        Assert.IsNull(retrievedParcelamento);
        Assert.AreEqual(0, retrievedPagamentos.Count(), "Pagamentos should be cascade deleted by EF.");
    }

    // ─────────────────────────────────────────────────────────────
    // Regression tests — Bug 4 (Delete transaction FK constraint)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Deleting a transaction that references a category must succeed
    /// without throwing a FOREIGN KEY constraint error.
    /// Regression guard for SQLite FK constraint on transaction delete.
    /// </summary>
    [TestMethod]
    public async Task DeleteTransacao_WithCategoria_DoesNotThrowFKConstraint()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var txRepo = new TransactionRepository(context);

        var categoria = new Categoria { Id = Guid.NewGuid(), Name = "Transporte" };
        await context.Categorias.AddAsync(categoria);
        await context.SaveChangesAsync();

        var tx = Transacao.Create(DateTime.Today, "Uber", -25.00m, categoria.Id);
        await txRepo.AddAsync(tx);

        // Act — should NOT throw FK constraint
        await txRepo.DeleteAsync(tx.Id);

        // Assert
        var retrieved = await txRepo.GetByIdAsync(tx.Id);
        Assert.IsNull(retrieved, "Transaction should be deleted successfully.");

        // Category should still exist
        var cat = await context.Categorias.FindAsync(categoria.Id);
        Assert.IsNotNull(cat, "Category should not be affected by transaction deletion.");
    }

    // ─────────────────────────────────────────────────────────────
    // Regression tests — Bug 2 (Delete category with linked transactions)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Deleting a category that has linked transactions must set their
    /// CategoryId to null (dissociate) and succeed without FK errors.
    /// Regression guard for the category delete bug.
    /// </summary>
    [TestMethod]
    public async Task DeleteCategoria_WithLinkedTransactions_SetsTransactionCategoryIdToNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var catRepo = new CategoryRepository(context);
        var txRepo = new TransactionRepository(context);

        var categoria = new Categoria { Id = Guid.NewGuid(), Name = "Alimentação" };
        await catRepo.AddAsync(categoria);

        var tx1 = Transacao.Create(DateTime.Today, "Supermercado", -100.00m, categoria.Id);
        var tx2 = Transacao.Create(DateTime.Today, "Restaurante", -50.00m, categoria.Id);
        await txRepo.AddAsync(tx1);
        await txRepo.AddAsync(tx2);

        // Act — should NOT throw FK constraint
        await catRepo.DeleteAsync(categoria.Id);

        // Assert
        var retrievedCat = await catRepo.GetByIdAsync(categoria.Id);
        Assert.IsNull(retrievedCat, "Category should be deleted.");

        // Transactions should still exist with null CategoryId
        var remainingTx1 = await txRepo.GetByIdAsync(tx1.Id);
        var remainingTx2 = await txRepo.GetByIdAsync(tx2.Id);
        Assert.IsNotNull(remainingTx1, "Transaction 1 should still exist.");
        Assert.IsNotNull(remainingTx2, "Transaction 2 should still exist.");
        Assert.IsNull(remainingTx1.CategoryId, "Transaction 1 CategoryId should be null after category deletion.");
        Assert.IsNull(remainingTx2.CategoryId, "Transaction 2 CategoryId should be null after category deletion.");
    }

    /// <summary>
    /// Deleting a category that has linked MetasGasto must cascade-delete
    /// the MetasGasto records and succeed without FK errors.
    /// </summary>
    [TestMethod]
    public async Task DeleteCategoria_WithLinkedMetasGasto_CascadeDeletes()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var catRepo = new CategoryRepository(context);

        var categoria = new Categoria { Id = Guid.NewGuid(), Name = "Lazer" };
        await catRepo.AddAsync(categoria);

        var meta = new MetaGasto
        {
            Id = Guid.NewGuid(),
            CategoryId = categoria.Id,
            Month = 7,
            Year = 2026,
            TargetAmount = 300.00m
        };
        await context.MetasGasto.AddAsync(meta);
        await context.SaveChangesAsync();

        // Act
        await catRepo.DeleteAsync(categoria.Id);

        // Assert
        var retrievedCat = await catRepo.GetByIdAsync(categoria.Id);
        Assert.IsNull(retrievedCat, "Category should be deleted.");

        var retrievedMeta = await context.MetasGasto.FindAsync(meta.Id);
        Assert.IsNull(retrievedMeta, "MetaGasto should be cascade-deleted.");
    }
}

