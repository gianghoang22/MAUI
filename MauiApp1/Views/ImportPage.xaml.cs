using MauiApp1.ViewModels;
using MauiApp1.Services;

namespace MauiApp1.Views;

public partial class ImportPage : ContentPage, IQueryAttributable
{
    private readonly ImportViewModel viewModel;

    public ImportPage(ImportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("deckId", out var value)) viewModel.SetId(value?.ToString());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.RefreshAsync();
    }
}
