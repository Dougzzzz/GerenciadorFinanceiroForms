using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Data;

public interface IInvestmentRepository
{
    Task<IEnumerable<Investimento>> GetAllAsync();
    Task<Investimento?> GetByIdAsync(Guid id);
    Task AddAsync(Investimento investimento);
    Task UpdateAsync(Investimento investimento);
    Task DeleteAsync(Guid id);
}
