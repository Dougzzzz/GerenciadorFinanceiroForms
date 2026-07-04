using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Features.Contas;

public partial class ContasViewModel : ObservableObject
{
    private readonly IContaRepository _repository;

    [ObservableProperty]
    private ObservableCollection<Conta> _contas = new();

    [ObservableProperty]
    private string _newContaName = string.Empty;

    [ObservableProperty]
    private ContaType _newContaType = ContaType.Corrente;

    [ObservableProperty]
    private string? _errorMessage;

    // Available types for ComboBox
    public ObservableCollection<ContaType> AvailableTypes { get; } = new ObservableCollection<ContaType>
    {
        ContaType.Corrente,
        ContaType.CartaoDeCredito
    };

    public ContasViewModel(IContaRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    private async Task LoadContasAsync()
    {
        var items = await _repository.GetAllAsync();
        Contas.Clear();
        foreach (var item in items)
        {
            Contas.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddContaAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(NewContaName))
        {
            ErrorMessage = "O nome da conta é obrigatório.";
            return;
        }

        var newConta = new Conta
        {
            Id = Guid.NewGuid(),
            Name = NewContaName.Trim(),
            Type = NewContaType
        };

        await _repository.AddAsync(newConta);
        Contas.Add(newConta);

        // Clear form
        NewContaName = string.Empty;
        NewContaType = ContaType.Corrente;
    }

    [RelayCommand]
    private async Task DeleteContaAsync(Conta? conta)
    {
        if (conta == null) return;
        
        ErrorMessage = null;
        try
        {
            await _repository.DeleteAsync(conta.Id);
            Contas.Remove(conta);
        }
        catch (Exception)
        {
            ErrorMessage = "Não é possível excluir a conta pois ela está associada a transações existentes.";
        }
    }

    [RelayCommand]
    private async Task UpdateContaAsync(Conta? conta)
    {
        if (conta == null) return;

        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(conta.Name))
        {
            ErrorMessage = "O nome da conta não pode estar vazio.";
            return;
        }

        try
        {
            await _repository.UpdateAsync(conta);
        }
        catch (DbUpdateException)
        {
            ErrorMessage = "Ocorreu um erro ao atualizar a conta.";
            await LoadContasAsync();
        }
        catch (InvalidOperationException)
        {
            ErrorMessage = "Ocorreu um erro ao atualizar a conta.";
            await LoadContasAsync();
        }
    }
}
