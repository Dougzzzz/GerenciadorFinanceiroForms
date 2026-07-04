using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;
using System.Windows;

namespace ControleFinanceiroForms.Features.Transacoes;

public partial class TransacoesViewModel : ObservableObject
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IContaRepository _contaRepository;

    [ObservableProperty]
    private ObservableCollection<Transacao> _transacoes = new();

    [ObservableProperty]
    private ObservableCollection<Categoria> _categorias = new();

    [ObservableProperty]
    private ObservableCollection<Conta> _contas = new();

    public TransacoesViewModel(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IContaRepository contaRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _contaRepository = contaRepository;
    }

    [RelayCommand]
    public async Task LoadTransacoesAsync()
    {
        var categorias = await _categoryRepository.GetAllAsync();
        Categorias.Clear();
        foreach (var c in categorias) Categorias.Add(c);

        var contas = await _contaRepository.GetAllAsync();
        Contas.Clear();
        foreach (var c in contas) Contas.Add(c);

        var transacoes = await _transactionRepository.GetAllAsync();
        Transacoes.Clear();
        foreach (var t in transacoes.OrderByDescending(x => x.Date))
        {
            Transacoes.Add(t);
        }
    }

    public async Task UpdateTransacaoAsync(Transacao transacao)
    {
        if (transacao.Id == Guid.Empty)
        {
            await _transactionRepository.AddAsync(transacao);
        }
        else
        {
            // Recalculate hash since values might have changed
            transacao.GerarHash();
            await _transactionRepository.UpdateAsync(transacao);
        }
    }

    [RelayCommand]
    private async Task DeleteTransacaoAsync(Transacao? transacao)
    {
        if (transacao == null) return;
        
        var result = MessageBox.Show($"Deseja realmente excluir a transação '{transacao.Description}'?", 
                                     "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            await _transactionRepository.DeleteAsync(transacao.Id);
            Transacoes.Remove(transacao);
        }
    }
}
