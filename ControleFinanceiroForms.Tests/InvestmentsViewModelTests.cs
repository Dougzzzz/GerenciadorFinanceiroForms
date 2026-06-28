using ControleFinanceiroForms.Data.Entities;
using ControleFinanceiroForms.Features.Investments;
using ControleFinanceiroForms.Tests.Fakes;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class InvestmentsViewModelTests
{
    private FakeInvestmentRepository _repository = null!;
    private InvestmentsViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new FakeInvestmentRepository();
        _viewModel = new InvestmentsViewModel(_repository);
    }

    [TestMethod]
    public async Task AddInvestmentCommand_ValidInput_AddsToCollectionAndRepository()
    {
        // Arrange
        _viewModel.NewValue = 10000.50m;
        _viewModel.NewNote = "Bonus";
        _viewModel.NewConta = "XP Investimentos";
        _viewModel.NewTipoInvestimento = "Ações";
        _viewModel.NewTipoOperacao = OperacaoInvestimento.Aporte;
        
        // Act
        await _viewModel.AddInvestmentCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(1, _viewModel.Investments.Count);
        Assert.AreEqual(10000.50m, _viewModel.Investments[0].TotalValue);
        Assert.AreEqual("Bonus", _viewModel.Investments[0].Note);
        Assert.AreEqual("XP Investimentos", _viewModel.Investments[0].Conta);
        Assert.AreEqual("Ações", _viewModel.Investments[0].TipoInvestimento);
        Assert.AreEqual(OperacaoInvestimento.Aporte, _viewModel.Investments[0].TipoOperacao);

        var repoItems = await _repository.GetAllAsync();
        Assert.AreEqual(1, repoItems.Count());
        Assert.AreEqual(10000.50m, repoItems.First().TotalValue);
        Assert.AreEqual("XP Investimentos", repoItems.First().Conta);
        Assert.AreEqual("Ações", repoItems.First().TipoInvestimento);
        Assert.AreEqual(OperacaoInvestimento.Aporte, repoItems.First().TipoOperacao);
        
        // Form cleared
        Assert.IsNull(_viewModel.NewValue);
        Assert.AreEqual(string.Empty, _viewModel.NewNote);
        Assert.AreEqual(string.Empty, _viewModel.NewConta);
        Assert.AreEqual(string.Empty, _viewModel.NewTipoInvestimento);
        Assert.AreEqual(OperacaoInvestimento.SnapshotTotal, _viewModel.NewTipoOperacao);
        Assert.IsNull(_viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task AddInvestmentCommand_NegativeValue_ShowsErrorAndDoesNotAdd()
    {
        // Arrange
        _viewModel.NewValue = -50m;
        
        // Act
        await _viewModel.AddInvestmentCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(0, _viewModel.Investments.Count);
        Assert.IsNotNull(_viewModel.ErrorMessage);
    }
    
    [TestMethod]
    public async Task AddInvestmentCommand_NullValue_ShowsErrorAndDoesNotAdd()
    {
        // Arrange
        _viewModel.NewValue = null;
        
        // Act
        await _viewModel.AddInvestmentCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(0, _viewModel.Investments.Count);
        Assert.IsNotNull(_viewModel.ErrorMessage);
    }
}
