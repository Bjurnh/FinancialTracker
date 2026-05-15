using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using System.Collections.ObjectModel;

namespace FinanceTracker.UI.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly ITransactionRepository _repository;

    [ObservableProperty]
    private ObservableCollection<Transaction> _transactions = [];

    [ObservableProperty]
    private string _newTitle = string.Empty;

    [ObservableProperty]
    private string _newAmount = string.Empty;

    [ObservableProperty]
    private string _newCategory = string.Empty;

    [ObservableProperty]
    private bool _isIncome = false;

    [ObservableProperty]
    private bool _isBusy = false;

    public TransactionsViewModel(ITransactionRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    public async Task LoadTransactionsAsync()
    {
        _isBusy = true;
        var results = await _repository.GetAllAsync();
        Transactions = new ObservableCollection<Transaction>(results);
        IsBusy = false;
    }

    [RelayCommand]
    public async Task AddTransactionAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTitle) || !decimal.TryParse(NewAmount, out var amount))
            return;

        var transaction = new Transaction
        {
            Title = NewTitle,
            Amount = amount,
            Category = NewCategory,
            Type = IsIncome ? TransactionType.Income : TransactionType.Expense
        };

        await _repository.AddAsync(transaction);

        // Reset form
        NewTitle = string.Empty;
        NewAmount = string.Empty;
        NewCategory = string.Empty;

        await LoadTransactionsAsync();
    }

    [RelayCommand]
    public async Task DeleteTransactionAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        await LoadTransactionsAsync();
    }
}
