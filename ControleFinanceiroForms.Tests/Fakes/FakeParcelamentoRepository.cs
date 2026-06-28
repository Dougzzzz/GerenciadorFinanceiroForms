using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Tests.Fakes;

public sealed class FakeParcelamentoRepository : IParcelamentoRepository
{
    private readonly List<Parcelamento> _store = new();

    public Task<IEnumerable<Parcelamento>> GetAllAsync()
        => Task.FromResult<IEnumerable<Parcelamento>>(_store.OrderByDescending(p => p.DataInicio).ToList());

    public Task<Parcelamento?> GetByIdAsync(Guid id)
        => Task.FromResult(_store.FirstOrDefault(p => p.Id == id));

    public Task AddAsync(Parcelamento parcelamento)
    {
        _store.Add(parcelamento);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
}
