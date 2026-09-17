using MauiApp1.ViewModels;
using MauiApp1.Services;

namespace MauiApp1.Views;

public partial class LearningPage : ContentPage
{
    private readonly LearningViewModel viewModel;
    private readonly EditorLifecycle lifecycle;

    public LearningPage(LearningViewModel viewModel, EditorLifecycle lifecycle)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
        this.lifecycle = lifecycle;
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
