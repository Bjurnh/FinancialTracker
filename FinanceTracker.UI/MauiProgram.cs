using FinanceTracker.Application.Interfaces;
using FinanceTracker.Application.Services;
using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Import;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.UI.ViewModels;
using FinanceTracker.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "finance.db");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
        builder.Services.AddScoped<BudgetService>();
        builder.Services.AddScoped<RecurringTransactionService>();
        builder.Services.AddScoped<CsvImportService>();

        builder.Services.AddTransient<TransactionsViewModel>();
        builder.Services.AddTransient<TransactionsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Run startup tasks synchronously — safe here because we're
        // not on the UI thread yet during MauiApp initialization
        Task.Run(async () =>
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            var recurringService = scope.ServiceProvider.GetRequiredService<RecurringTransactionService>();
            await recurringService.ProcessDueRecurrencesAsync();
        }).GetAwaiter().GetResult();

        return app;
    }
}