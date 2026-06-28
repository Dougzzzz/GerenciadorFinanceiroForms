namespace ControleFinanceiroForms.Data.Entities;

/// <summary>
/// Represents a manual wealth/investment snapshot recorded by the user.
/// Each record captures the total portfolio value at a given point in time,
/// enabling the user to track net-worth evolution without linking to brokerage APIs.
/// </summary>
public class Investimento
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Timestamp when the snapshot was recorded.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Total portfolio value at <see cref="RecordedAt"/>.</summary>
    public decimal TotalValue { get; set; }

    /// <summary>Optional note (e.g., "After January rebalancing").</summary>
    public string? Note { get; set; }

    /// <summary>Institution or account (e.g., "Nubank", "XP").</summary>
    public string? Conta { get; set; }

    /// <summary>Asset class (e.g., "Renda Fixa", "Ações").</summary>
    public string? TipoInvestimento { get; set; }

    /// <summary>Type of investment operation.</summary>
    public OperacaoInvestimento TipoOperacao { get; set; } = OperacaoInvestimento.SnapshotTotal;
}
