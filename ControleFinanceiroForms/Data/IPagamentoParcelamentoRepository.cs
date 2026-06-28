using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Data;

public interface IPagamentoParcelamentoRepository
{
    Task<IEnumerable<PagamentoParcelamento>> GetByParcelamentoIdAsync(Guid parcelamentoId);
    Task AddAsync(PagamentoParcelamento pagamento);
    Task DeleteAsync(Guid id);
}
