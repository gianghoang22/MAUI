using MauiApp1.ViewModels;
using MauiApp1.Services;

namespace MauiApp1.Views;

public partial class LearningPage : ContentPage
{
    private readonly LearningViewModel viewModel;
    private readonly EditorLifecycle lifecycle;
    private bool isActive;

    public LearningPage(LearningViewModel viewModel, EditorLifecycle lifecycle)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
        this.lifecycle = lifecycle;
        Loaded += (_, _) => { if (isActive) ConnectKeyboard(); };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        isActive = true;
        ConnectKeyboard();
        lifecycle.Enter(viewModel);
        await viewModel.RefreshAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        isActive = false;
        DisconnectKeyboard();
        await lifecycle.LeaveAsync(viewModel);
    }

    partial void ConnectKeyboard();
    partial void DisconnectKeyboard();
}
