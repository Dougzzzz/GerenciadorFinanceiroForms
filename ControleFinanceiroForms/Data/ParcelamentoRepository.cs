using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Data;

public class ParcelamentoRepository : IParcelamentoRepository
{
    private readonly AppDbContext _context;

    public ParcelamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Parcelamento>> GetAllAsync()
    {
        return await _context.Parcelamentos
            .Include(p => p.Pagamentos)
            .OrderByDescending(p => p.DataInicio)
            .ToListAsync();
    }

    public async Task<Parcelamento?> GetByIdAsync(Guid id)
    {
        return await _context.Parcelamentos
            .Include(p => p.Pagamentos)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Parcelamento parcelamento)
    {
        await _context.Parcelamentos.AddAsync(parcelamento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var parcelamento = await _context.Parcelamentos.FindAsync(id);
        if (parcelamento != null)
        {
            _context.Parcelamentos.Remove(parcelamento);
            await _context.SaveChangesAsync();
        }
    }
}
