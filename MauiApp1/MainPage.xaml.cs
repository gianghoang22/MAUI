using MauiApp1.Controls;
using MauiApp1.ViewModels;

namespace MauiApp1;

public partial class MainPage : ContentPage, IQueryAttributable
{
    private readonly LibraryViewModel viewModel;

    public MainPage(LibraryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("create", out var create) && create?.ToString() == "class")
            viewModel.ShowCreate = true;
        if (query.TryGetValue("search", out var search))
            viewModel.Search = Uri.UnescapeDataString(search?.ToString() ?? "");
        query.Remove("create");
        query.Remove("search");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.RefreshAsync();
    }

    private void OnDashboardSizeChanged(object? sender, EventArgs arguments)
    {
        DashboardLayout.Padding = new Thickness(LayoutMetrics.PageInset(DashboardLayout.Width), 20);
    }
}
