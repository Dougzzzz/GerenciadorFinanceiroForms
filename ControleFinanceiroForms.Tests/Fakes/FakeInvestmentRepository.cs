using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Tests.Fakes;

public sealed class FakeInvestmentRepository : IInvestmentRepository
{
    private readonly List<Investimento> _store = new();

    public Task<IEnumerable<Investimento>> GetAllAsync()
        => Task.FromResult<IEnumerable<Investimento>>(_store.OrderByDescending(i => i.RecordedAt).ToList());

    public Task<Investimento?> GetByIdAsync(Guid id)
        => Task.FromResult(_store.FirstOrDefault(i => i.Id == id));

    public Task AddAsync(Investimento investimento)
    {
        _store.Add(investimento);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Investimento investimento)
    {
        var index = _store.FindIndex(i => i.Id == investimento.Id);
        if (index >= 0)
        {
            _store[index] = investimento;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }
}
