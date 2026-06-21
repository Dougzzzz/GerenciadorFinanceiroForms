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
        
        // Act
        await _viewModel.AddInvestmentCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(1, _viewModel.Investments.Count);
        Assert.AreEqual(10000.50m, _viewModel.Investments[0].TotalValue);
        Assert.AreEqual("Bonus", _viewModel.Investments[0].Note);

        var repoItems = await _repository.GetAllAsync();
        Assert.AreEqual(1, repoItems.Count());
        Assert.AreEqual(10000.50m, repoItems.First().TotalValue);
        
        // Form cleared
        Assert.IsNull(_viewModel.NewValue);
        Assert.AreEqual(string.Empty, _viewModel.NewNote);
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
