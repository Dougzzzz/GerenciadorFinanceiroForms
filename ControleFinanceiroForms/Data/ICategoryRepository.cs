using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Data;

public interface ICategoryRepository
{
    Task<IEnumerable<Categoria>> GetAllAsync();
    Task<Categoria?> GetByIdAsync(Guid id);
    Task AddAsync(Categoria category);
    Task UpdateAsync(Categoria category);
    Task DeleteAsync(Guid id);
}
