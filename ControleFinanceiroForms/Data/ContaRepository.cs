using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Data;

public class ContaRepository : IContaRepository
{
    private readonly AppDbContext _context;

    public ContaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Conta>> GetAllAsync()
    {
        return await _context.Contas.ToListAsync();
    }

    public async Task<Conta?> GetByIdAsync(Guid id)
    {
        return await _context.Contas.FindAsync(id);
    }

    public async Task AddAsync(Conta conta)
    {
        await _context.Contas.AddAsync(conta);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Conta conta)
    {
        _context.Contas.Update(conta);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var conta = await _context.Contas.FindAsync(id);
        if (conta != null)
        {
            _context.Contas.Remove(conta);
            await _context.SaveChangesAsync();
        }
    }
}
