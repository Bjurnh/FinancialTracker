using FinanceTracker.Application.Interfaces;
using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;

    public TransactionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync() => await _db.Transactions.OrderByDescending(t => t.Date).ToListAsync();

    public async Task<Transaction> GetByIdAsync(Guid id) => await _db.Transactions.FindAsync(id);
    public async Task<IEnumerable<Transaction>> GetByCategoryAsync(string category)
      => await _db.Transactions
        .Where(t => t.Category == category)
        .OrderByDescending(t => t.Date)
        .ToListAsync();
    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to)
      => await _db.Transactions
        .Where(t => t.Date >= from && t.Date <= to)
        .OrderByDescending(t => t.Date)
        .ToListAsync();
    public async Task AddAsync(Transaction transaction)
    {
        await _db.Transactions.AddAsync(transaction);
        await _db.SaveChangesAsync();
    }
    public async Task UpdateAsync(Transaction transaction)
    {
        _db.Transactions.Update(transaction);
        await _db.SaveChangesAsync();
    }
    public async Task DeleteAsync(Guid id)
    {
        var transaction = await _db.Transactions.FindAsync(id);
        if (transaction is not null)
        {
            _db.Transactions.Remove(transaction);
            await _db.SaveChangesAsync();
        }
    }
    public async Task<decimal> GetTotalByTypeAsync(TransactionType type, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Transactions.Where(t => t.Type == type);

        if (from.HasValue)
            query = query.Where(t => t.Date >= from.Value);
        if (to.HasValue) 
            query = query.Where(t => t.Date <= to.Value);

        return await query.SumAsync(t => t.Amount);
    }
}