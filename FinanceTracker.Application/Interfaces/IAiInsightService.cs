using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Interfaces;

public interface IAiInsightService
{
    Task<string> GetSpendingSummaryAsync(int month, int year);
    Task<string> AskQuestionAsync(string question, int month, int year);
    Task<string> SuggestCategoryAsync(string transactionTitle);
}
