using MauiApp1.ViewModels;
using MauiApp1.Services;

namespace MauiApp1.Views;

public partial class CardEditorPage : ContentPage, IQueryAttributable
{
    private readonly CardEditorViewModel viewModel;
    private readonly EditorLifecycle lifecycle;

    public CardEditorPage(CardEditorViewModel viewModel, EditorLifecycle lifecycle)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
        this.lifecycle = lifecycle;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("deckId", out var deck)) return;
        query.TryGetValue("cardId", out var card);
        viewModel.SetIds(deck?.ToString(), card?.ToString());
    }

    private void OnVietnameseCompleted(object? sender, EventArgs arguments)
    {
        if (!viewModel.IsBusy) EnglishField.Focus();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        lifecycle.Enter(viewModel);
        await viewModel.RefreshAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await lifecycle.LeaveAsync(viewModel);
    }
}
