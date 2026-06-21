using System.Collections.ObjectModel;
using ControleFinanceiroForms.Data.Entities;
using ControleFinanceiroForms.Features.Categories;
using ControleFinanceiroForms.Tests.Fakes;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class CategoriesViewModelTests
{
    private FakeCategoryRepository _repository = null!;
    private CategoriesViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new FakeCategoryRepository();
        _viewModel = new CategoriesViewModel(_repository);
    }

    [TestMethod]
    public async Task AddCategoryCommand_ValidInput_AddsToCollectionAndRepository()
    {
        // Arrange
        _viewModel.NewCategoryName = "Test Category";
        _viewModel.NewCategoryBudget = 500.00m;

        // Act
        await _viewModel.AddCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(1, _viewModel.Categories.Count);
        Assert.AreEqual("Test Category", _viewModel.Categories[0].Name);
        Assert.AreEqual(500.00m, _viewModel.Categories[0].BudgetLimit);

        var repoItems = await _repository.GetAllAsync();
        Assert.AreEqual(1, repoItems.Count());
        Assert.AreEqual("Test Category", repoItems.First().Name);
        
        // Ensure form is cleared after add
        Assert.AreEqual(string.Empty, _viewModel.NewCategoryName);
        Assert.IsNull(_viewModel.NewCategoryBudget);
    }

    [TestMethod]
    public async Task AddCategoryCommand_NegativeBudget_ShowsErrorAndDoesNotAdd()
    {
        // Arrange
        _viewModel.NewCategoryName = "Invalid Category";
        _viewModel.NewCategoryBudget = -10.00m;

        // Act
        await _viewModel.AddCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(0, _viewModel.Categories.Count, "Category should not be added when budget is negative.");
        var repoItems = await _repository.GetAllAsync();
        Assert.AreEqual(0, repoItems.Count(), "Repository should remain empty.");
        Assert.IsNotNull(_viewModel.ErrorMessage, "Error message should be set.");
    }

    [TestMethod]
    public async Task AddCategoryCommand_EmptyName_ShowsErrorAndDoesNotAdd()
    {
        // Arrange
        _viewModel.NewCategoryName = "   "; // whitespace
        _viewModel.NewCategoryBudget = 100.00m;

        // Act
        await _viewModel.AddCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(0, _viewModel.Categories.Count, "Category should not be added when name is empty.");
        var repoItems = await _repository.GetAllAsync();
        Assert.AreEqual(0, repoItems.Count(), "Repository should remain empty.");
        Assert.IsNotNull(_viewModel.ErrorMessage, "Error message should be set.");
    }
    
    [TestMethod]
    public async Task LoadCategoriesAsync_PopulatesCollectionFromRepository()
    {
        // Arrange
        await _repository.AddAsync(new Categoria { Name = "Cat 1", BudgetLimit = 100 });
        await _repository.AddAsync(new Categoria { Name = "Cat 2", BudgetLimit = 200 });
        
        // Act
        await _viewModel.LoadCategoriesCommand.ExecuteAsync(null);
        
        // Assert
        Assert.AreEqual(2, _viewModel.Categories.Count);
        Assert.AreEqual("Cat 1", _viewModel.Categories[0].Name);
        Assert.AreEqual("Cat 2", _viewModel.Categories[1].Name);
    }
}
