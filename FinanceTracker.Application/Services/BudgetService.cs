using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Services;

public class BudgetService
{
    private readonly IBudgetRepository _budgetRepo;
    private readonly ITransactionRepository _transactionRepo;

    public BudgetService(IBudgetRepository budgetRepo, ITransactionRepository transactionRepo)
    {
        _budgetRepo = budgetRepo;
        _transactionRepo = transactionRepo;
    }

    // Recalculate Spent for all budgets in a given month
    public async Task RecalculateSpentAsync(int month, int year)
    {
        var budgets = await _budgetRepo.GetByMonthAsync(month, year);
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddTicks(-1);
        var transactions = await _transactionRepo.GetByDateRangeAsync(from, to);

        foreach (var budget in budgets)
        {
            budget.Spent = transactions
                .Where(t => t.Category == budget.Category
                         && t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            await _budgetRepo.UpdateAsync(budget);
        }
    }

    // Roll over unused budget from previous month
    public async Task ApplyRolloverAsync(int month, int year)
    {
        var prevMonth = month == 1 ? 12 : month - 1;
        var prevYear = month == 1 ? year - 1 : year;

        var previousBudgets = await _budgetRepo.GetByMonthAsync(prevMonth, prevYear);

        foreach (var prev in previousBudgets.Where(b => b.RolloverUnused))
        {
            var unused = prev.Remaining > 0 ? prev.Remaining : 0;
            if (unused == 0) continue;

            var current = await _budgetRepo.GetByCategoryAndMonthAsync(prev.Category, month, year);
            if (current is not null)
            {
                current.RolloverAmount += unused;
                await _budgetRepo.UpdateAsync(current);
            }
        }
    }

    // Get budget health summary for the month
    public async Task<IEnumerable<BudgetSummary>> GetSummaryAsync(int month, int year)
    {
        var budgets = await _budgetRepo.GetByMonthAsync(month, year);
        return budgets.Select(b => new BudgetSummary
        {
            Category = b.Category,
            Limit = b.Limit,
            Spent = b.Spent,
            Remaining = b.Remaining,
            UsagePercent = b.UsagePercent,
            IsOverBudget = b.Remaining < 0
        });
    }
}

public record BudgetSummary
{
    public string Category { get; init; } = string.Empty;
    public decimal Limit { get; init; }
    public decimal Spent { get; init; }
    public decimal Remaining { get; init; }
    public decimal UsagePercent { get; init; }
    public bool IsOverBudget { get; init; }
}

