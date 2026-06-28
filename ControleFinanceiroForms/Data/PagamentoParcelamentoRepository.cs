using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Data;

public class PagamentoParcelamentoRepository : IPagamentoParcelamentoRepository
{
    private readonly AppDbContext _context;

    public PagamentoParcelamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PagamentoParcelamento>> GetByParcelamentoIdAsync(Guid parcelamentoId)
    {
        return await _context.PagamentosParcelamento
            .Where(pp => pp.ParcelamentoId == parcelamentoId)
            .OrderByDescending(pp => pp.DataPagamento)
            .ToListAsync();
    }

    public async Task AddAsync(PagamentoParcelamento pagamento)
    {
        await _context.PagamentosParcelamento.AddAsync(pagamento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var pagamento = await _context.PagamentosParcelamento.FindAsync(id);
        if (pagamento != null)
        {
            _context.PagamentosParcelamento.Remove(pagamento);
            await _context.SaveChangesAsync();
        }
    }
}
