using System;

namespace ControleFinanceiroForms.Data.Entities;

/// <summary>
/// Represents a payment amortizing an installment plan.
/// </summary>
public class PagamentoParcelamento
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ParcelamentoId { get; set; }

    public decimal ValorPago { get; set; }

    public DateTime DataPagamento { get; set; }

    public string? Nota { get; set; }
}
