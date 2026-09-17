using MauiApp1.ViewModels;
using MauiApp1.Services;

namespace MauiApp1.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel viewModel;

    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.RefreshAsync();
    }
}
