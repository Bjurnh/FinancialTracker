using CsvHelper;
using CsvHelper.Configuration;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using System.Globalization;

namespace FinanceTracker.Infrastructure.Import;

public class CsvImportService
{
    private readonly ITransactionRepository _repo;

    public CsvImportService(ITransactionRepository repo) => _repo = repo;

    public async Task<int> ImportAsync(Stream csvStream)
    {
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null  // ignore missing optional columns
        });

        var records = csv.GetRecords<CsvTransactionRecord>().ToList();
        int imported = 0;

        foreach (var record in records)
        {
            if (!decimal.TryParse(record.Amount, out var amount)) continue;

            var transaction = new Transaction
            {
                Title = record.Title ?? "Imported",
                Amount = Math.Abs(amount),
                Type = amount < 0 ? TransactionType.Expense : TransactionType.Income,
                Category = record.Category ?? "Uncategorized",
                Date = DateTime.TryParse(record.Date, out var d) ? d : DateTime.UtcNow,
                IsSynced = false
            };

            await _repo.AddAsync(transaction);
            imported++;
        }

        return imported;
    }
}

// Maps to CSV columns — adjust names to match your bank's export format
public class CsvTransactionRecord
{
    public string? Date { get; set; }
    public string? Title { get; set; }
    public string? Amount { get; set; }
    public string? Category { get; set; }
}
