using System.ComponentModel;
using MauiBasics.ViewModels;

namespace MauiBasics.Views;

public partial class LearnPage : ContentPage
{
    private readonly LearnViewModel _viewModel;

    public LearnPage(LearnViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.PropertyChanged += OnViewModelChanged;
    }

    protected override void OnDisappearing()
    {
        _viewModel.PropertyChanged -= OnViewModelChanged;
        base.OnDisappearing();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LearnViewModel.CounterDescription))
            SemanticScreenReader.Default.Announce(_viewModel.CounterDescription);
    }
}
