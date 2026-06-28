using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.Parcelamentos;

public partial class ParcelamentoViewModel : ObservableObject
{
    private readonly Parcelamento _model;

    public Parcelamento Model => _model;

    public Guid Id => _model.Id;
    public string Descricao => _model.Descricao;
    public decimal ValorTotal => _model.ValorTotal;
    public int? NumeroParcelas => _model.NumeroParcelas;
    public DateTime DataInicio => _model.DataInicio;
    public DateTime CriadoEm => _model.CriadoEm;

    public ParcelamentoViewModel(Parcelamento model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public decimal ValorPago => _model.Pagamentos.Sum(p => p.ValorPago);

    public decimal SaldoRestante => Math.Max(0, _model.ValorTotal - ValorPago);

    public double PercentualQuitado
    {
        get
        {
            if (_model.ValorTotal <= 0) return 100;
            var pct = (double)(ValorPago / _model.ValorTotal) * 100;
            return Math.Min(100, Math.Max(0, pct));
        }
    }

    public string StatusLabel => SaldoRestante > 0 ? "Em aberto" : "Quitado";

    public decimal? ValorMedioParcela
    {
        get
        {
            if (!_model.NumeroParcelas.HasValue || _model.NumeroParcelas.Value <= 0)
                return null;
            return _model.ValorTotal / _model.NumeroParcelas.Value;
        }
    }

    public string FormattedProgress => $"R$ {ValorPago:N2} de R$ {ValorTotal:N2} ({PercentualQuitado:F1}%)";

    public string FormattedParcelas => _model.NumeroParcelas.HasValue 
        ? $"{_model.NumeroParcelas}x (Média: R$ {ValorMedioParcela:N2}/mês)" 
        : "N/A";

    public void NotifyCalculatedPropertiesChanged()
    {
        OnPropertyChanged(nameof(ValorPago));
        OnPropertyChanged(nameof(SaldoRestante));
        OnPropertyChanged(nameof(PercentualQuitado));
        OnPropertyChanged(nameof(StatusLabel));
        OnPropertyChanged(nameof(ValorMedioParcela));
        OnPropertyChanged(nameof(FormattedProgress));
        OnPropertyChanged(nameof(FormattedParcelas));
    }
}
