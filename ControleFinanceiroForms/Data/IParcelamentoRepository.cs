using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Data;

public interface IParcelamentoRepository
{
    Task<IEnumerable<Parcelamento>> GetAllAsync();
    Task<Parcelamento?> GetByIdAsync(Guid id);
    Task AddAsync(Parcelamento parcelamento);
    Task DeleteAsync(Guid id);
}
