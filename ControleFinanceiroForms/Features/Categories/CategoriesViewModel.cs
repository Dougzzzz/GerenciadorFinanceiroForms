using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Features.Categories;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryRepository _repository;

    [ObservableProperty]
    private ObservableCollection<Categoria> _categories = new();

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private decimal? _newCategoryBudget;

    [ObservableProperty]
    private string? _errorMessage;

    public CategoriesViewModel(ICategoryRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        var items = await _repository.GetAllAsync();
        Categories.Clear();
        foreach (var item in items)
        {
            Categories.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            ErrorMessage = "O nome da categoria é obrigatório.";
            return;
        }

        if (NewCategoryBudget.HasValue && NewCategoryBudget.Value < 0)
        {
            ErrorMessage = "O limite de orçamento não pode ser negativo.";
            return;
        }

        var newCategory = new Categoria
        {
            Id = Guid.NewGuid(),
            Name = NewCategoryName.Trim(),
            BudgetLimit = NewCategoryBudget
        };

        await _repository.AddAsync(newCategory);
        Categories.Add(newCategory);

        // Clear form
        NewCategoryName = string.Empty;
        NewCategoryBudget = null;
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(Categoria? category)
    {
        if (category == null) return;
        
        ErrorMessage = null;
        try
        {
            await _repository.DeleteAsync(category.Id);
            Categories.Remove(category);
        }
        catch (Exception)
        {
            ErrorMessage = "Não é possível excluir a categoria pois ela está associada a transações existentes.";
        }
    }

    [RelayCommand]
    private async Task UpdateCategoryAsync(Categoria? category)
    {
        if (category == null) return;

        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(category.Name))
        {
            ErrorMessage = "O nome da categoria não pode estar vazio.";
            return;
        }

        if (category.BudgetLimit.HasValue && category.BudgetLimit.Value < 0)
        {
            ErrorMessage = "O limite de orçamento não pode ser negativo.";
            return;
        }
        try
        {
            await _repository.UpdateAsync(category);
        }
        catch (DbUpdateException)
        {
            ErrorMessage = "Ocorreu um erro ao atualizar a categoria.";
            await LoadCategoriesAsync();
        }
        catch (InvalidOperationException)
        {
            ErrorMessage = "Ocorreu um erro ao atualizar a categoria.";
            await LoadCategoriesAsync();
        }
    }
}
