using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Data;

/// <summary>
/// Defines the persistence contract for <see cref="Transacao"/> entities.
/// Implementations are injected into feature-slice Handlers via DI.
/// Tests use <see cref="FakeTransactionRepository"/> as a manual stub.
/// </summary>
public interface ITransactionRepository
{
    Task<IEnumerable<Transacao>> GetAllAsync();
    Task<Transacao?> GetByIdAsync(Guid id);
    Task AddAsync(Transacao transaction);
    Task AddRangeAsync(IEnumerable<Transacao> transactions);
    Task UpdateAsync(Transacao transaction);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<string>> GetExistingHashesAsync(IEnumerable<string> hashes);
    Task<IEnumerable<Transacao>> GetByMonthAsync(int month, int year);
}
