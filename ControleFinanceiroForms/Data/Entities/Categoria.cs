namespace ControleFinanceiroForms.Data.Entities;

/// <summary>
/// Represents a spending category (e.g., "Food", "Transport").
/// Categories define optional monthly budget limits used by the Dashboard.
/// </summary>
public class Categoria
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Optional monthly budget limit in the local currency.</summary>
    public decimal? BudgetLimit { get; set; }

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();

    public ICollection<MetaGasto> MetasGasto { get; set; } = new List<MetaGasto>();
}
