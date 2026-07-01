using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Tests.Fakes;

/// <summary>
/// Manual in-memory stub for <see cref="ITransactionRepository"/>.
/// Used exclusively in unit tests — no mocking libraries involved (ADR-004).
/// </summary>
/// <remarks>
/// Mirrors the real SQLite unique-index behaviour on <see cref="Transacao.ChaveExclusiva"/>:
/// <see cref="AddAsync"/> and <see cref="AddRangeAsync"/> throw
/// <see cref="InvalidOperationException"/> when a duplicate non-empty hash is detected,
/// preventing false-negative tests that pass against the fake but fail in production.
/// </remarks>
public sealed class FakeTransactionRepository : ITransactionRepository
{
    private readonly List<Transacao> _store = new();

    public Task<IEnumerable<Transacao>> GetAllAsync()
        => Task.FromResult<IEnumerable<Transacao>>(_store.ToList());

    public Task<Transacao?> GetByIdAsync(Guid id)
        => Task.FromResult(_store.FirstOrDefault(t => t.Id == id));

    public Task AddAsync(Transacao transaction)
    {
        if (!string.IsNullOrEmpty(transaction.ChaveExclusiva)
            && _store.Any(t => t.ChaveExclusiva == transaction.ChaveExclusiva))
        {
            throw new InvalidOperationException(
                $"Duplicate ChaveExclusiva detected: {transaction.ChaveExclusiva}");
        }

        _store.Add(transaction);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<Transacao> transactions)
    {
        foreach (var transaction in transactions)
        {
            if (!string.IsNullOrEmpty(transaction.ChaveExclusiva)
                && _store.Any(t => t.ChaveExclusiva == transaction.ChaveExclusiva))
            {
                throw new InvalidOperationException(
                    $"Duplicate ChaveExclusiva detected: {transaction.ChaveExclusiva}");
            }

            _store.Add(transaction);
        }

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

    public Task<IEnumerable<string>> GetExistingHashesAsync(IEnumerable<string> hashes)
    {
        var hashSet = new HashSet<string>(hashes);
        var existing = _store
            .Select(t => t.ChaveExclusiva)
            .Where(h => hashSet.Contains(h))
            .ToList();
        return Task.FromResult<IEnumerable<string>>(existing);
    }

    public Task<IEnumerable<Transacao>> GetByMonthAsync(int month, int year)
    {
        var filtered = _store
            .Where(t => t.Date.Month == month && t.Date.Year == year)
            .ToList();
        return Task.FromResult<IEnumerable<Transacao>>(filtered);
    }

    // ── Test-helper properties ──────────────────────────────────────
    public int Count => _store.Count;
    public IReadOnlyList<Transacao> All => _store.AsReadOnly();
}
