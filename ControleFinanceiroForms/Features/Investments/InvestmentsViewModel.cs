using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.Investments;

public partial class InvestmentsViewModel : ObservableObject
{
    private readonly IInvestmentRepository _repository;

    [ObservableProperty]
    private ObservableCollection<Investimento> _investments = new();

    [ObservableProperty]
    private decimal? _newValue;

    [ObservableProperty]
    private string _newNote = string.Empty;

    [ObservableProperty]
    private string _newConta = string.Empty;

    [ObservableProperty]
    private string _newTipoInvestimento = string.Empty;

    [ObservableProperty]
    private OperacaoInvestimento _newTipoOperacao = OperacaoInvestimento.SnapshotTotal;

    [ObservableProperty]
    private string? _errorMessage;

    public InvestmentsViewModel(IInvestmentRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    private async Task LoadInvestmentsAsync()
    {
        var items = await _repository.GetAllAsync();
        Investments.Clear();
        foreach (var item in items)
        {
            Investments.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddInvestmentAsync()
    {
        ErrorMessage = null;

        if (!NewValue.HasValue)
        {
            ErrorMessage = "O valor total é obrigatório.";
            return;
        }

        if (NewValue.Value < 0)
        {
            ErrorMessage = "O valor total não pode ser negativo.";
            return;
        }

        var inv = new Investimento
        {
            Id = Guid.NewGuid(),
            RecordedAt = DateTime.Now,
            TotalValue = NewValue.Value,
            Note = string.IsNullOrWhiteSpace(NewNote) ? null : NewNote.Trim(),
            Conta = string.IsNullOrWhiteSpace(NewConta) ? null : NewConta.Trim(),
            TipoInvestimento = string.IsNullOrWhiteSpace(NewTipoInvestimento) ? null : NewTipoInvestimento.Trim(),
            TipoOperacao = NewTipoOperacao
        };

        try
        {
            await _repository.AddAsync(inv);
            Investments.Insert(0, inv); // Insert at top since list is descending order

            NewValue = null;
            NewNote = string.Empty;
            NewConta = string.Empty;
            NewTipoInvestimento = string.Empty;
            NewTipoOperacao = OperacaoInvestimento.SnapshotTotal;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao registrar investimento: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteInvestmentAsync(Investimento? investimento)
    {
        if (investimento == null) return;

        ErrorMessage = null;
        try
        {
            await _repository.DeleteAsync(investimento.Id);
            Investments.Remove(investimento);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao excluir investimento: {ex.Message}";
        }
    }
}
