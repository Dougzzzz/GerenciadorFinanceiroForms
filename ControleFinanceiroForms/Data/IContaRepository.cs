using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Data;

public interface IContaRepository
{
    Task<IEnumerable<Conta>> GetAllAsync();
    Task<Conta?> GetByIdAsync(Guid id);
    Task AddAsync(Conta conta);
    Task UpdateAsync(Conta conta);
    Task DeleteAsync(Guid id);
}
