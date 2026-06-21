using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Tests.Fakes;

public sealed class FakeInvestmentRepository : IInvestmentRepository
{
    private readonly List<Investimento> _store = new();

    public Task<IEnumerable<Investimento>> GetAllAsync()
        => Task.FromResult<IEnumerable<Investimento>>(_store.OrderByDescending(i => i.RecordedAt).ToList());

    public Task AddAsync(Investimento investimento)
    {
        _store.Add(investimento);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }
}
