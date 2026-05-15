using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Domain.Entities;

public class Budget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Category { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public int Month { get; set; }   // 1–12
    public int Year { get; set; }
    public bool RolloverUnused { get; set; } = false;
    public decimal RolloverAmount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal Remaining => Limit + RolloverAmount - Spent;
    public decimal UsagePercent => Limit > 0 ? (Spent / Limit) * 100 : 0;
}
