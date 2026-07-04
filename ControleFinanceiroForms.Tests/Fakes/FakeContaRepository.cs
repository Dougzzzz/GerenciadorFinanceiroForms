using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;
using System;

namespace ControleFinanceiroForms.Tests.Fakes;

public class FakeContaRepository : IContaRepository
{
    public List<Conta> Data { get; } = new();
    public bool ShouldThrowOnDelete { get; set; } = false;

    public Task<IEnumerable<Conta>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Conta>>(Data.ToList());
    }

    public Task<Conta?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Data.FirstOrDefault(c => c.Id == id));
    }

    public Task AddAsync(Conta conta)
    {
        Data.Add(conta);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Conta conta)
    {
        var existing = Data.FirstOrDefault(c => c.Id == conta.Id);
        if (existing != null)
        {
            existing.Name = conta.Name;
            existing.Type = conta.Type;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        if (ShouldThrowOnDelete)
        {
            throw new Exception("Simulated delete error");
        }

        var existing = Data.FirstOrDefault(c => c.Id == id);
        if (existing != null)
        {
            Data.Remove(existing);
        }
        return Task.CompletedTask;
    }
}
