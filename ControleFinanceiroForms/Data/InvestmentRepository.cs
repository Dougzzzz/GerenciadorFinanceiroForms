using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Data;

public class InvestmentRepository : IInvestmentRepository
{
    private readonly AppDbContext _context;

    public InvestmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Investimento>> GetAllAsync()
    {
        return await _context.Investimentos
            .OrderByDescending(i => i.RecordedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Investimento investimento)
    {
        await _context.Investimentos.AddAsync(investimento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var investimento = await _context.Investimentos.FindAsync(id);
        if (investimento != null)
        {
            _context.Investimentos.Remove(investimento);
            await _context.SaveChangesAsync();
        }
    }
}
