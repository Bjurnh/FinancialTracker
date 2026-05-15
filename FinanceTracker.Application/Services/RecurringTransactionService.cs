using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Services;

public class RecurringTransactionService
{
    private readonly ITransactionRepository _repo;

    public RecurringTransactionService(ITransactionRepository repo) => _repo = repo;

    // Call this on app startup to auto-generate due recurring transactions
    public async Task ProcessDueRecurrencesAsync()
    {
        var all = await _repo.GetAllAsync();
        var recurring = all.Where(t => t.IsRecurring && t.RecurrenceInterval.HasValue);

        foreach (var source in recurring)
        {
            var nextDue = GetNextDueDate(source.Date, source.RecurrenceInterval!.Value);

            if (nextDue.Date > DateTime.UtcNow.Date) continue;

            // Check if already generated for this period
            var alreadyExists = all.Any(t =>
                t.Title == source.Title &&
                t.Category == source.Category &&
                t.Amount == source.Amount &&
                t.Date.Date == nextDue.Date);

            if (alreadyExists) continue;

            var generated = new Transaction
            {
                Title = source.Title,
                Amount = source.Amount,
                Type = source.Type,
                Category = source.Category,
                Tags = source.Tags,
                Date = nextDue,
                IsRecurring = false, // generated copy is not a template
                IsSynced = false
            };

            await _repo.AddAsync(generated);
        }
    }

    private static DateTime GetNextDueDate(DateTime lastDate, RecurrenceInterval interval)
        => interval switch
        {
            RecurrenceInterval.Daily => lastDate.AddDays(1),
            RecurrenceInterval.Weekly => lastDate.AddDays(7),
            RecurrenceInterval.Monthly => lastDate.AddMonths(1),
            RecurrenceInterval.Yearly => lastDate.AddYears(1),
            _ => lastDate.AddMonths(1)
        };
}
