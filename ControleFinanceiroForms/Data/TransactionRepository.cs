using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Data;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transacao>> GetAllAsync()
    {
        return await _context.Transacoes.ToListAsync();
    }

    public async Task<Transacao?> GetByIdAsync(Guid id)
    {
        return await _context.Transacoes.FindAsync(id);
    }

    public async Task AddAsync(Transacao transaction)
    {
        await _context.Transacoes.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Transacao> transactions)
    {
        await _context.Transacoes.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transacao transaction)
    {
        _context.Transacoes.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var transaction = await _context.Transacoes.FindAsync(id);
        if (transaction != null)
        {
            _context.Transacoes.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<string>> GetExistingHashesAsync(IEnumerable<string> hashes)
    {
        var hashList = hashes.ToList();
        return await _context.Transacoes
            .Where(t => hashList.Contains(t.ChaveExclusiva))
            .Select(t => t.ChaveExclusiva)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transacao>> GetByMonthAsync(int month, int year)
    {
        return await _context.Transacoes
            .Where(t => t.Date.Month == month && t.Date.Year == year)
            .ToListAsync();
    }
}
