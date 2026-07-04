using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControleFinanceiroForms.Features.Contas;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;
using ControleFinanceiroForms.Tests.Fakes;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class ContasViewModelTests
{
    private FakeContaRepository _repository = null!;
    private ContasViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new FakeContaRepository();
        _viewModel = new ContasViewModel(_repository);
    }

    [TestMethod]
    public async Task LoadContasAsync_LoadsItemsFromRepository()
    {
        // Arrange
        _repository.Data.Add(new Conta { Id = Guid.NewGuid(), Name = "Conta 1", Type = ContaType.Corrente });
        _repository.Data.Add(new Conta { Id = Guid.NewGuid(), Name = "Conta 2", Type = ContaType.CartaoDeCredito });

        // Act
        await _viewModel.LoadContasCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(2, _viewModel.Contas.Count);
        Assert.AreEqual("Conta 1", _viewModel.Contas[0].Name);
        Assert.AreEqual(ContaType.Corrente, _viewModel.Contas[0].Type);
        Assert.AreEqual("Conta 2", _viewModel.Contas[1].Name);
        Assert.AreEqual(ContaType.CartaoDeCredito, _viewModel.Contas[1].Type);
    }

    [TestMethod]
    public async Task AddContaAsync_WithValidData_SavesToRepositoryAndAddsToCollection()
    {
        // Arrange
        _viewModel.NewContaName = "Nubank";
        _viewModel.NewContaType = ContaType.CartaoDeCredito;

        // Act
        await _viewModel.AddContaCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
        Assert.AreEqual(1, _viewModel.Contas.Count);
        Assert.AreEqual("Nubank", _viewModel.Contas[0].Name);
        Assert.AreEqual(ContaType.CartaoDeCredito, _viewModel.Contas[0].Type);

        Assert.AreEqual(1, _repository.Data.Count);
        Assert.AreEqual("Nubank", _repository.Data[0].Name);

        // Verifica se os campos foram limpos
        Assert.AreEqual(string.Empty, _viewModel.NewContaName);
        Assert.AreEqual(ContaType.Corrente, _viewModel.NewContaType);
    }

    [TestMethod]
    public async Task AddContaAsync_WithEmptyName_SetsErrorMessage()
    {
        // Arrange
        _viewModel.NewContaName = "   ";
        
        // Act
        await _viewModel.AddContaCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual("O nome da conta é obrigatório.", _viewModel.ErrorMessage);
        Assert.AreEqual(0, _viewModel.Contas.Count);
        Assert.AreEqual(0, _repository.Data.Count);
    }

    [TestMethod]
    public async Task DeleteContaAsync_WithValidItem_DeletesFromRepositoryAndRemovesFromCollection()
    {
        // Arrange
        var conta = new Conta { Id = Guid.NewGuid(), Name = "Banco Inter", Type = ContaType.Corrente };
        _repository.Data.Add(conta);
        _viewModel.Contas.Add(conta);

        // Act
        await _viewModel.DeleteContaCommand.ExecuteAsync(conta);

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
        Assert.AreEqual(0, _viewModel.Contas.Count);
        Assert.AreEqual(0, _repository.Data.Count);
    }

    [TestMethod]
    public async Task DeleteContaAsync_WhenRepositoryThrows_SetsErrorMessage()
    {
        // Arrange
        var conta = new Conta { Id = Guid.NewGuid(), Name = "Banco Inter" };
        _repository.Data.Add(conta);
        _viewModel.Contas.Add(conta);
        _repository.ShouldThrowOnDelete = true;

        // Act
        await _viewModel.DeleteContaCommand.ExecuteAsync(conta);

        // Assert
        Assert.AreEqual("Não é possível excluir a conta pois ela está associada a transações existentes.", _viewModel.ErrorMessage);
        Assert.AreEqual(1, _viewModel.Contas.Count); // Item should not be removed
    }

    [TestMethod]
    public async Task UpdateContaAsync_WithValidData_UpdatesRepository()
    {
        // Arrange
        var conta = new Conta { Id = Guid.NewGuid(), Name = "Updated Name" };
        _repository.Data.Add(conta);
        
        // Act
        await _viewModel.UpdateContaCommand.ExecuteAsync(conta);

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
        Assert.AreEqual("Updated Name", _repository.Data[0].Name);
    }

    [TestMethod]
    public async Task UpdateContaAsync_WithEmptyName_SetsErrorMessage()
    {
        // Arrange
        var conta = new Conta { Id = Guid.NewGuid(), Name = "" };

        // Act
        await _viewModel.UpdateContaCommand.ExecuteAsync(conta);

        // Assert
        Assert.AreEqual("O nome da conta não pode estar vazio.", _viewModel.ErrorMessage);
    }
}
