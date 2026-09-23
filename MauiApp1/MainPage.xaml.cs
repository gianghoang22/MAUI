using MauiApp1.ViewModels;
using MauiApp1.Services;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    private readonly LibraryViewModel viewModel;
    private bool? overviewIsWide;

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
        var inset = Controls.LayoutMetrics.PageInset(DashboardLayout.Width);
        DashboardLayout.Padding = new Thickness(inset, compact ? 16 : 28);
        DashboardLayout.RowSpacing = 20;
        DashboardHeader.Spacing = 20;
        HeroSurface.Padding = 24;
        WelcomeHeading.FontSize = compact ? 30 : spacious ? 38 : 34;
        HeroHeading.FontSize = compact ? 24 : 28;
        HeroArt.IsVisible = DashboardLayout.Width >= 1200 && DashboardLayout.Height >= 600;
        ArrangeOverview(DashboardLayout.Width >= 1000);
    }

    private void ArrangeOverview(bool wide)
    {
        if (overviewIsWide == wide) return;
        overviewIsWide = wide;
        OverviewLayout.ColumnDefinitions = wide
            ? [new ColumnDefinition(new GridLength(0.74, GridUnitType.Star)), new ColumnDefinition(new GridLength(0.26, GridUnitType.Star))]
            : [new ColumnDefinition(GridLength.Star)];
        OverviewLayout.RowDefinitions = wide
            ? [new RowDefinition(GridLength.Auto)]
            : [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)];
        Grid.SetColumn(StatsLayout, wide ? 1 : 0);
        Grid.SetRow(StatsLayout, wide ? 0 : 1);
        StatsLayout.ColumnDefinitions.Clear();
        StatsLayout.RowDefinitions.Clear();
        for (var index = 0; index < 3; index++)
        {
            if (wide) StatsLayout.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            else StatsLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var tile = (Border)StatsLayout.Children[index];
            Grid.SetColumn(tile, wide ? 0 : index);
            Grid.SetRow(tile, wide ? index : 0);
            tile.Padding = wide ? new Thickness(20, 8) : new Thickness(12, 16);
            if (tile.Content is not Grid content) continue;
            content.ColumnDefinitions = wide
                ? [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star)]
                : [new ColumnDefinition(GridLength.Star)];
            content.RowDefinitions = wide
                ? [new RowDefinition(GridLength.Auto)]
                : [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)];
            Grid.SetColumn((BindableObject)content.Children[1], wide ? 1 : 0);
            Grid.SetRow((BindableObject)content.Children[1], wide ? 0 : 1);
        }
    }
}
