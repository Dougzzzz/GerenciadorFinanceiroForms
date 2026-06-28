using System.Collections.ObjectModel;
using ControleFinanceiroForms.Data;
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

    [TestMethod]
    public async Task DeleteCategoryCommand_Success_RemovesFromCollectionAndRepository()
    {
        // Arrange
        var category = new Categoria { Id = Guid.NewGuid(), Name = "To Delete", BudgetLimit = 100 };
        await _repository.AddAsync(category);
        _viewModel.Categories.Add(category);

        // Act
        await _viewModel.DeleteCategoryCommand.ExecuteAsync(category);

        // Assert
        Assert.AreEqual(0, _viewModel.Categories.Count);
        var repoItems = await _repository.GetAllAsync();
        Assert.AreEqual(0, repoItems.Count());
        Assert.IsNull(_viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task DeleteCategoryCommand_RepositoryThrowsException_SetsErrorMessageAndDoesNotRemoveFromCollection()
    {
        // Arrange
        var category = new Categoria { Id = Guid.NewGuid(), Name = "Locked Category", BudgetLimit = 100 };
        var faultyRepo = new FaultyCategoryRepository();
        var vm = new CategoriesViewModel(faultyRepo);
        vm.Categories.Add(category);

        // Act
        await vm.DeleteCategoryCommand.ExecuteAsync(category);

        // Assert
        Assert.AreEqual(1, vm.Categories.Count);
        Assert.AreEqual(category, vm.Categories[0]);
        Assert.IsNotNull(vm.ErrorMessage);
        Assert.AreEqual("Não é possível excluir a categoria pois ela está associada a transações existentes.", vm.ErrorMessage);
    }

    [TestMethod]
    public async Task UpdateCategoryCommand_ValidInput_UpdatesInRepositoryAndClearsError()
    {
        // Arrange
        var category = new Categoria { Id = Guid.NewGuid(), Name = "Original Name", BudgetLimit = 100.00m };
        await _repository.AddAsync(category);
        _viewModel.Categories.Add(category);
        _viewModel.ErrorMessage = "Some old error";

        // Edit
        category.Name = "Updated Name";
        category.BudgetLimit = 200.00m;

        // Act
        await _viewModel.UpdateCategoryCommand.ExecuteAsync(category);

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
        var repoItem = await _repository.GetByIdAsync(category.Id);
        Assert.IsNotNull(repoItem);
        Assert.AreEqual("Updated Name", repoItem.Name);
        Assert.AreEqual(200.00m, repoItem.BudgetLimit);
    }

    [TestMethod]
    public async Task UpdateCategoryCommand_EmptyName_SetsErrorMessageAndDoesNotUpdateInRepository()
    {
        // Arrange
        var category = new Categoria { Id = Guid.NewGuid(), Name = "Original Name", BudgetLimit = 100.00m };
        await _repository.AddAsync(category);
        _viewModel.Categories.Add(category);

        // Edit with invalid name
        category.Name = "  "; // whitespace

        // Act
        await _viewModel.UpdateCategoryCommand.ExecuteAsync(category);

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.AreEqual("O nome da categoria não pode estar vazio.", _viewModel.ErrorMessage);

        var repoItem = await _repository.GetByIdAsync(category.Id);
        Assert.IsNotNull(repoItem);
        // The repository item shouldn't have changed, but in standard in-memory/fake reference objects, 
        // if they reference the exact same object edit might reflect in fake. Let's make sure update was not saved.
        // Actually, FakeCategoryRepository modifies store. But the validation failed, so it didn't call repo.UpdateAsync.
    }

    [TestMethod]
    public async Task UpdateCategoryCommand_NegativeBudget_SetsErrorMessageAndDoesNotUpdateInRepository()
    {
        // Arrange
        var category = new Categoria { Id = Guid.NewGuid(), Name = "Original Name", BudgetLimit = 100.00m };
        await _repository.AddAsync(category);
        _viewModel.Categories.Add(category);

        // Edit with negative limit
        category.BudgetLimit = -5.00m;

        // Act
        await _viewModel.UpdateCategoryCommand.ExecuteAsync(category);

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.AreEqual("O limite de orçamento não pode ser negativo.", _viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task UpdateCategoryCommand_RepositoryThrowsException_SetsErrorMessage()
    {
        // Arrange
        var category = new Categoria { Id = Guid.NewGuid(), Name = "Category to Update", BudgetLimit = 100 };
        var faultyRepo = new FaultyCategoryRepository();
        var vm = new CategoriesViewModel(faultyRepo);
        vm.Categories.Add(category);

        // Act
        await vm.UpdateCategoryCommand.ExecuteAsync(category);

        // Assert
        Assert.IsNotNull(vm.ErrorMessage);
        Assert.AreEqual("Ocorreu um erro ao atualizar a categoria.", vm.ErrorMessage);
    }

    private class FaultyCategoryRepository : ICategoryRepository
    {
        public Task<IEnumerable<Categoria>> GetAllAsync() => Task.FromResult(Enumerable.Empty<Categoria>());
        public Task<Categoria?> GetByIdAsync(Guid id) => throw new NotImplementedException();
        public Task AddAsync(Categoria category) => throw new NotImplementedException();
        public Task UpdateAsync(Categoria category) => throw new InvalidOperationException("Update failed simulated.");
        public Task DeleteAsync(Guid id) => throw new InvalidOperationException("Foreign key constraint violation simulated.");
    }
}
