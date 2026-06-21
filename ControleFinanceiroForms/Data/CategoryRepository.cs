using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Data;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Categoria>> GetAllAsync()
    {
        return await _context.Categorias.ToListAsync();
    }

    public async Task<Categoria?> GetByIdAsync(Guid id)
    {
        return await _context.Categorias.FindAsync(id);
    }

    public async Task AddAsync(Categoria category)
    {
        await _context.Categorias.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Categoria category)
    {
        _context.Categorias.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _context.Categorias.FindAsync(id);
        if (category != null)
        {
            _context.Categorias.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
