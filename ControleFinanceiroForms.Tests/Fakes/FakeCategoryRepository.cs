using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Tests.Fakes;

public sealed class FakeCategoryRepository : ICategoryRepository
{
    private readonly List<Categoria> _store = new();

    public Task<IEnumerable<Categoria>> GetAllAsync()
        => Task.FromResult<IEnumerable<Categoria>>(_store.ToList());

    public Task<Categoria?> GetByIdAsync(Guid id)
        => Task.FromResult(_store.FirstOrDefault(c => c.Id == id));

    public Task AddAsync(Categoria category)
    {
        _store.Add(category);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Categoria category)
    {
        var index = _store.FindIndex(c => c.Id == category.Id);
        if (index >= 0)
        {
            _store[index] = category;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}
