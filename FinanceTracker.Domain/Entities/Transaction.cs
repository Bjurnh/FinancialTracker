using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = [];
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public bool IsRecurring { get; set; }
        public RecurrenceInterval? RecurrenceInterval { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSynced { get; set; } = false;
    }
}
