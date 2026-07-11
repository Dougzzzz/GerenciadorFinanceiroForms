using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.Transacoes;

public partial class TransacoesViewModel : ObservableObject
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    [ObservableProperty]
    private ObservableCollection<Transacao> _transacoes = new();

    [ObservableProperty]
    private ObservableCollection<Categoria> _categories = new();

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private int _selectedMonth;

    [ObservableProperty]
    private int _selectedYear;

    public TransacoesViewModel(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;

        var today = DateTime.Today;
        _selectedMonth = today.Month;
        _selectedYear = today.Year;
    }

    [RelayCommand]
    public async Task LoadTransacoesAsync()
    {
        ErrorMessage = null;

        var categorias = await _categoryRepository.GetAllAsync();
        Categories.Clear();
        foreach (var cat in categorias)
        {
            Categories.Add(cat);
        }

        var items = await _transactionRepository.GetByMonthAsync(SelectedMonth, SelectedYear);
        Transacoes.Clear();
        foreach (var item in items.OrderByDescending(t => t.Date))
        {
            Transacoes.Add(item);
        }
    }

    [RelayCommand]
    private async Task DeleteTransacaoAsync(Transacao? transacao)
    {
        if (transacao == null) return;

        ErrorMessage = null;
        try
        {
            await _transactionRepository.DeleteAsync(transacao.Id);
            Transacoes.Remove(transacao);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao excluir transação: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateTransacaoAsync(Transacao? transacao)
    {
        if (transacao == null) return;

        ErrorMessage = null;
        try
        {
            await _transactionRepository.UpdateAsync(transacao);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao atualizar transação: {ex.Message}";
            await LoadTransacoesAsync();
        }
    }
}
