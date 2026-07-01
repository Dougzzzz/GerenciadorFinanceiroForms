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
        // Entity may come from a different DbContext (Transient lifetime),
        // so attach it and mark as modified (review-002 issue 005).
        var entry = _context.Entry(category);
        if (entry.State == EntityState.Detached)
        {
            _context.Categorias.Attach(category);
            entry.State = EntityState.Modified;
        }
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
