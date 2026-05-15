using FinanceTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Interfaces;

public interface IBudgetRepository
{
    Task<IEnumerable<Budget>> GetByMonthAsync(int month, int year);
    Task<Budget?> GetByCategoryAndMonthAsync(string category, int month, int year);
    Task AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(Guid id);
}
