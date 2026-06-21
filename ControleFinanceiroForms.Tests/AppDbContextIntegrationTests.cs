using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;
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
        var tx = new Transacao
        {
            Id = Guid.NewGuid(),
            Date = new DateTime(2026, 5, 10),
            Description = "Conta de luz",
            Amount = 320.50m
        };
        tx.GerarHash();

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
        var tx = new Transacao
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Today,
            Description = "Uber",
            Amount = 25.00m,
            CategoryId = categoria.Id
        };
        tx.GerarHash();

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
            Note = "Rebalanceamento janeiro"
        };

        // Act
        await context.Investimentos.AddAsync(investimento);
        await context.SaveChangesAsync();

        var retrieved = await context.Investimentos.FindAsync(investimento.Id);

        // Assert
        Assert.IsNotNull(retrieved, "Investimento should be retrievable after save.");
        Assert.AreEqual(investimento.TotalValue, retrieved.TotalValue);
        Assert.AreEqual(investimento.Note, retrieved.Note);
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

        var tx1 = new Transacao
        {
            Id = Guid.NewGuid(),
            Date = new DateTime(2026, 1, 1),
            Description = "Duplicate",
            Amount = 10.00m
        };
        tx1.GerarHash();

        var tx2 = new Transacao
        {
            Id = Guid.NewGuid(),
            Date = tx1.Date,
            Description = tx1.Description,
            Amount = tx1.Amount,
            ChaveExclusiva = tx1.ChaveExclusiva // same hash
        };

        // Act
        await context.Transacoes.AddAsync(tx1);
        await context.SaveChangesAsync();

        await context.Transacoes.AddAsync(tx2);

        // Assert
        await Assert.ThrowsExceptionAsync<DbUpdateException>(
            async () => await context.SaveChangesAsync(),
            "Saving a duplicate ChaveExclusiva must fail due to the unique index.");
    }
}
