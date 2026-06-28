using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Tests.Fakes;

public sealed class FakePagamentoParcelamentoRepository : IPagamentoParcelamentoRepository
{
    private readonly List<PagamentoParcelamento> _store = new();

    public Task<IEnumerable<PagamentoParcelamento>> GetByParcelamentoIdAsync(Guid parcelamentoId)
        => Task.FromResult<IEnumerable<PagamentoParcelamento>>(_store.Where(pp => pp.ParcelamentoId == parcelamentoId).OrderByDescending(pp => pp.DataPagamento).ToList());

    public Task AddAsync(PagamentoParcelamento pagamento)
    {
        _store.Add(pagamento);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.RemoveAll(pp => pp.Id == id);
        return Task.CompletedTask;
    }
}
