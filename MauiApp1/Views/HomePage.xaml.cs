using MauiApp1.Controls;
using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.RefreshAsync();
    }

    private void OnLayoutSizeChanged(object? sender, EventArgs arguments)
    {
        HomeLayout.Padding = new Thickness(LayoutMetrics.PageInset(HomeLayout.Width), 20);
        ResumeSurface.WidthRequest = Math.Max(0, Math.Min(620, HomeLayout.Width - HomeLayout.Padding.HorizontalThickness));
        ResumeArt.IsVisible = HomeLayout.Width >= 600;
    }
}
