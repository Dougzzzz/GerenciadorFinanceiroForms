namespace ControleFinanceiroForms.Data.Entities;

/// <summary>
/// Represents a monthly spending goal (budget target) for a <see cref="Categoria"/>.
/// Each record stores the target amount and the reference month/year.
/// </summary>
public class MetaGasto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Foreign key to the associated spending category.</summary>
    public Guid CategoryId { get; set; }

    public Categoria? Categoria { get; set; }

    /// <summary>Target spending limit for the month in local currency.</summary>
    public decimal TargetAmount { get; set; }

    /// <summary>Reference month (1–12).</summary>
    public int Month { get; set; }

    /// <summary>Reference year (e.g., 2026).</summary>
    public int Year { get; set; }
}
