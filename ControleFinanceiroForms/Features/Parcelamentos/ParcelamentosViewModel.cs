using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.Parcelamentos;

public partial class ParcelamentosViewModel : ObservableObject
{
    private readonly IParcelamentoRepository _parcelamentoRepository;
    private readonly IPagamentoParcelamentoRepository _pagamentoRepository;

    private readonly ObservableCollection<ParcelamentoViewModel> _allParcelamentos = new();

    [ObservableProperty]
    private ObservableCollection<ParcelamentoViewModel> _visibleParcelamentos = new();

    // ── Form Novo Parcelamento ─────────────────────────────────────
    [ObservableProperty]
    private string _newDescricao = string.Empty;

    [ObservableProperty]
    private decimal? _newValorTotal;

    [ObservableProperty]
    private int? _newNumeroParcelas;

    [ObservableProperty]
    private DateTime _newDataInicio = DateTime.Today;

    // ── Form Novo Pagamento ────────────────────────────────────────
    [ObservableProperty]
    private decimal? _newValorPago;

    [ObservableProperty]
    private DateTime _newDataPagamento = DateTime.Today;

    [ObservableProperty]
    private string _newPagamentoNota = string.Empty;

    // ── Selecionado e Filtro ───────────────────────────────────────
    [ObservableProperty]
    private ParcelamentoViewModel? _selectedParcelamento;

    [ObservableProperty]
    private ObservableCollection<PagamentoParcelamento> _selectedParcelamentoPagamentos = new();

    [ObservableProperty]
    private bool _filtroSomenteEmAberto;

    partial void OnSelectedParcelamentoChanged(ParcelamentoViewModel? value)
    {
        SelectedParcelamentoPagamentos.Clear();
        if (value != null)
        {
            foreach (var pag in value.Model.Pagamentos)
            {
                SelectedParcelamentoPagamentos.Add(pag);
            }
        }
    }

    [ObservableProperty]
    private string? _errorMessage;

    public ParcelamentosViewModel(
        IParcelamentoRepository parcelamentoRepository,
        IPagamentoParcelamentoRepository pagamentoRepository)
    {
        _parcelamentoRepository = parcelamentoRepository;
        _pagamentoRepository = pagamentoRepository;
    }

    [RelayCommand]
    public async Task LoadParcelamentosAsync()
    {
        ErrorMessage = null;
        try
        {
            var raw = await _parcelamentoRepository.GetAllAsync();
            _allParcelamentos.Clear();
            foreach (var item in raw)
            {
                _allParcelamentos.Add(new ParcelamentoViewModel(item));
            }
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar parcelamentos: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddParcelamentoAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(NewDescricao))
        {
            ErrorMessage = "A descrição do parcelamento é obrigatória.";
            return;
        }

        if (!NewValorTotal.HasValue)
        {
            ErrorMessage = "O valor total é obrigatório.";
            return;
        }

        if (NewValorTotal.Value <= 0)
        {
            ErrorMessage = "O valor total deve ser maior que zero.";
            return;
        }

        if (NewNumeroParcelas.HasValue && NewNumeroParcelas.Value <= 0)
        {
            ErrorMessage = "O número de parcelas deve ser maior que zero.";
            return;
        }

        var p = new Parcelamento
        {
            Id = Guid.NewGuid(),
            Descricao = NewDescricao.Trim(),
            ValorTotal = NewValorTotal.Value,
            NumeroParcelas = NewNumeroParcelas,
            DataInicio = NewDataInicio,
            CriadoEm = DateTime.Now
        };

        try
        {
            await _parcelamentoRepository.AddAsync(p);
            var vm = new ParcelamentoViewModel(p);
            _allParcelamentos.Insert(0, vm);
            ApplyFilter();

            // Clear form
            NewDescricao = string.Empty;
            NewValorTotal = null;
            NewNumeroParcelas = null;
            NewDataInicio = DateTime.Today;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao salvar parcelamento: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteParcelamentoAsync(ParcelamentoViewModel? vm)
    {
        if (vm == null) return;
        ErrorMessage = null;

        try
        {
            await _parcelamentoRepository.DeleteAsync(vm.Id);
            _allParcelamentos.Remove(vm);
            if (SelectedParcelamento == vm)
            {
                SelectedParcelamento = null;
            }
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao excluir parcelamento: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddPagamentoAsync()
    {
        ErrorMessage = null;

        if (SelectedParcelamento == null)
        {
            ErrorMessage = "Selecione um parcelamento para registrar o pagamento.";
            return;
        }

        if (!NewValorPago.HasValue)
        {
            ErrorMessage = "O valor pago é obrigatório.";
            return;
        }

        if (NewValorPago.Value <= 0)
        {
            ErrorMessage = "O valor pago deve ser maior que zero.";
            return;
        }

        var pg = new PagamentoParcelamento
        {
            Id = Guid.NewGuid(),
            ParcelamentoId = SelectedParcelamento.Id,
            ValorPago = NewValorPago.Value,
            DataPagamento = NewDataPagamento,
            Nota = string.IsNullOrWhiteSpace(NewPagamentoNota) ? null : NewPagamentoNota.Trim()
        };

        try
        {
            await _pagamentoRepository.AddAsync(pg);
            SelectedParcelamento.Model.Pagamentos.Add(pg);
            SelectedParcelamentoPagamentos.Add(pg);
            SelectedParcelamento.NotifyCalculatedPropertiesChanged();

            // Re-apply filter if status changed and only in-progress is checked
            ApplyFilter();

            // Reset form
            NewValorPago = null;
            NewDataPagamento = DateTime.Today;
            NewPagamentoNota = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao registrar pagamento: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeletePagamentoAsync(PagamentoParcelamento? pg)
    {
        if (pg == null || SelectedParcelamento == null) return;
        ErrorMessage = null;

        try
        {
            await _pagamentoRepository.DeleteAsync(pg.Id);
            SelectedParcelamento.Model.Pagamentos.Remove(pg);
            SelectedParcelamentoPagamentos.Remove(pg);
            SelectedParcelamento.NotifyCalculatedPropertiesChanged();
            
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao excluir pagamento: {ex.Message}";
        }
    }

    partial void OnFiltroSomenteEmAbertoChanged(bool value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (FiltroSomenteEmAberto)
        {
            VisibleParcelamentos = new ObservableCollection<ParcelamentoViewModel>(
                _allParcelamentos.Where(p => p.SaldoRestante > 0)
            );
        }
        else
        {
            VisibleParcelamentos = new ObservableCollection<ParcelamentoViewModel>(_allParcelamentos);
        }
    }
}
