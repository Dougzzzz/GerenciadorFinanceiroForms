using System;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class ContaRepositoryTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;
        
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsAllContas()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repo = new ContaRepository(context);

        await context.Contas.AddAsync(new Conta { Id = Guid.NewGuid(), Name = "Conta 1", Type = ContaType.Corrente });
        await context.Contas.AddAsync(new Conta { Id = Guid.NewGuid(), Name = "Conta 2", Type = ContaType.CartaoDeCredito });
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        Assert.AreEqual(2, result.Count());
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsCorrectConta()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repo = new ContaRepository(context);

        var id = Guid.NewGuid();
        var conta = new Conta { Id = id, Name = "Conta 1", Type = ContaType.Corrente };
        await context.Contas.AddAsync(conta);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByIdAsync(id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Conta 1", result.Name);
    }

    [TestMethod]
    public async Task AddAsync_AddsContaToDatabase()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repo = new ContaRepository(context);

        var conta = new Conta { Id = Guid.NewGuid(), Name = "New Conta", Type = ContaType.Corrente };

        // Act
        await repo.AddAsync(conta);

        // Assert
        var result = await context.Contas.FindAsync(conta.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual("New Conta", result.Name);
    }

    [TestMethod]
    public async Task UpdateAsync_UpdatesContaInDatabase()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repo = new ContaRepository(context);

        var conta = new Conta { Id = Guid.NewGuid(), Name = "Old Name", Type = ContaType.Corrente };
        await context.Contas.AddAsync(conta);
        await context.SaveChangesAsync();

        // Act
        conta.Name = "New Name";
        await repo.UpdateAsync(conta);

        // Assert
        var result = await context.Contas.FindAsync(conta.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual("New Name", result.Name);
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesContaFromDatabase()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repo = new ContaRepository(context);

        var conta = new Conta { Id = Guid.NewGuid(), Name = "Conta to Delete", Type = ContaType.Corrente };
        await context.Contas.AddAsync(conta);
        await context.SaveChangesAsync();

        // Act
        await repo.DeleteAsync(conta.Id);

        // Assert
        var result = await context.Contas.FindAsync(conta.Id);
        Assert.IsNull(result);
    }
}
