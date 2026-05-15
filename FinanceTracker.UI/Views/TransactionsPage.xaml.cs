using FinanceTracker.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.UI.Views;

public partial class TransactionsPage : ContentPage
{
    private readonly TransactionsViewModel _viewModel;

    public TransactionsPage(TransactionsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTransactionsAsync();
    }
}
