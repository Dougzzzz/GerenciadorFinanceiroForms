using System;
using System.Collections.Generic;

namespace ControleFinanceiroForms.Data.Entities;

/// <summary>
/// Represents a purchase or debt installment plan.
/// </summary>
public class Parcelamento
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Descricao { get; set; } = string.Empty;

    public decimal ValorTotal { get; set; }

    public int? NumeroParcelas { get; set; }

    public DateTime DataInicio { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public ICollection<PagamentoParcelamento> Pagamentos { get; set; } = new List<PagamentoParcelamento>();
}
