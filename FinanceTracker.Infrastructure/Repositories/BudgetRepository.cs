using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _db;

    public BudgetRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Budget>> GetByMonthAsync(int month, int year)
        => await _db.Budgets
            .Where(b => b.Month == month && b.Year == year)
            .ToListAsync();

    public async Task<Budget?> GetByCategoryAndMonthAsync(string category, int month, int year)
        => await _db.Budgets
            .FirstOrDefaultAsync(b => b.Category == category
                                   && b.Month == month
                                   && b.Year == year);

    public async Task AddAsync(Budget budget)
    {
        await _db.Budgets.AddAsync(budget);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Budget budget)
    {
        _db.Budgets.Update(budget);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var budget = await _db.Budgets.FindAsync(id);
        if (budget is not null)
        {
            _db.Budgets.Remove(budget);
            await _db.SaveChangesAsync();
        }
    }
}
