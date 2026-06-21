using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Tests.Fakes;

/// <summary>
/// Manual in-memory stub for <see cref="ITransactionRepository"/>.
/// Used exclusively in unit tests — no mocking libraries involved (ADR-004).
/// </summary>
public sealed class FakeTransactionRepository : ITransactionRepository
{
    private readonly List<Transacao> _store = new();

    public Task<IEnumerable<Transacao>> GetAllAsync()
        => Task.FromResult<IEnumerable<Transacao>>(_store.ToList());

    public Task<Transacao?> GetByIdAsync(Guid id)
        => Task.FromResult(_store.FirstOrDefault(t => t.Id == id));

    public Task AddAsync(Transacao transaction)
    {
        _store.Add(transaction);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<Transacao> transactions)
    {
        _store.AddRange(transactions);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Transacao transaction)
    {
        var index = _store.FindIndex(t => t.Id == transaction.Id);
        if (index >= 0)
            _store[index] = transaction;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    // ── Test-helper properties ──────────────────────────────────────
    public int Count => _store.Count;
    public IReadOnlyList<Transacao> All => _store.AsReadOnly();
}
