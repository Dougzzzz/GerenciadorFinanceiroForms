using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Data;

public interface IInvestmentRepository
{
    Task<IEnumerable<Investimento>> GetAllAsync();
    Task AddAsync(Investimento investimento);
    Task DeleteAsync(Guid id);
}
