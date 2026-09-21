using MauiApp1.ViewModels;
using MauiApp1.Services;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    private readonly LibraryViewModel viewModel;

    public MainPage(LibraryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.RefreshAsync();
    }

    private void OnDashboardSizeChanged(object? sender, EventArgs arguments)
    {
        if (DashboardHeader is null || DashboardLayout.Width <= 0) return;
        var compact = DashboardLayout.Width < 600;
        var spacious = DashboardLayout.Width >= 1200;
        var inset = compact ? 16 : spacious ? 40 : 24;
        DashboardLayout.Padding = new Thickness(inset, compact ? 12 : 24);
        DashboardLayout.RowSpacing = compact ? 12 : 20;
        DashboardHeader.Spacing = compact ? 16 : 20;
        HeroSurface.Padding = compact ? 20 : 24;
        WelcomeHeading.FontSize = compact ? 26 : spacious ? 34 : 30;
        HeroHeading.FontSize = compact ? 24 : 28;
        HeroArt.IsVisible = DashboardLayout.Width >= 800 && DashboardLayout.Height >= 600;
    }
}
