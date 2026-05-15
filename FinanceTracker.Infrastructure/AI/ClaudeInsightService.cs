using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Infrastructure.AI;

public class ClaudeInsightService : IAiInsightService
{
    private readonly HttpClient _http;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IBudgetRepository _budgetRepo;

    // Never hardcode the key — read from config
    private const string ApiKey = "YOUR_ANTHROPIC_API_KEY";
    private const string Model = "claude-sonnet-4-20250514";
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public ClaudeInsightService(
        HttpClient http,
        ITransactionRepository transactionRepo,
        IBudgetRepository budgetRepo)
    {
        _http = http;
        _transactionRepo = transactionRepo;
        _budgetRepo = budgetRepo;

        _http.DefaultRequestHeaders.Add("x-api-key", ApiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<string> GetSpendingSummaryAsync(int month, int year)
    {
        var context = await BuildFinancialContextAsync(month, year);

        var prompt = $"""
            You are a personal finance assistant. Analyze this spending data and give a
            concise 3-4 sentence summary of the user's financial health this month.
            Highlight top spending categories, budget status, and one actionable tip.

            Financial data:
            {context}
            """;

        return await CallClaudeAsync(prompt);
    }

    public async Task<string> AskQuestionAsync(string question, int month, int year)
    {
        var context = await BuildFinancialContextAsync(month, year);

        var prompt = $"""
            You are a personal finance assistant. Answer this question about the user's
            finances concisely and helpfully. Use only the data provided.

            Financial data:
            {context}

            Question: {question}
            """;

        return await CallClaudeAsync(prompt);
    }

    public async Task<string> SuggestCategoryAsync(string transactionTitle)
    {
        var prompt = $"""
            You are a transaction categorizer. Given this transaction title, respond with
            ONLY a single category name (1-2 words). No explanation, no punctuation.
            Common categories: Groceries, Food, Transport, Utilities, Subscriptions,
            Health, Shopping, Income, Entertainment, Housing.

            Transaction: {transactionTitle}
            """;

        return await CallClaudeAsync(prompt);
    }

    // ── Private helpers ────────────────────────────────────────────

    private async Task<string> BuildFinancialContextAsync(int month, int year)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddTicks(-1);

        var transactions = await _transactionRepo.GetByDateRangeAsync(from, to);
        var budgets = await _budgetRepo.GetByMonthAsync(month, year);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpense = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var byCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => $"{g.Key}: ${g.Sum(t => t.Amount):F2}")
            .ToList();

        var budgetStatus = budgets
            .Select(b => $"{b.Category}: spent ${b.Spent:F2} of ${b.Limit:F2} limit")
            .ToList();

        return $"""
            Month: {from:MMMM yyyy}
            Total income:  ${totalIncome:F2}
            Total expenses: ${totalExpense:F2}
            Net: ${totalIncome - totalExpense:F2}

            Spending by category:
            {string.Join("\n", byCategory)}

            Budget status:
            {string.Join("\n", budgetStatus)}
            """;
    }

    private async Task<string> CallClaudeAsync(string userMessage)
    {
        var payload = new
        {
            model = Model,
            max_tokens = 1024,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(ApiUrl, content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        return result
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "No response from AI.";
    }
}
