using System.ComponentModel;
using MauiBasics.ViewModels;

namespace MauiBasics.Views;

public partial class TasksPage : ContentPage
{
    private readonly TasksViewModel _viewModel;
    private bool _confirmingDelete;

    public TasksPage(TasksViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.PropertyChanged += OnViewModelChanged;
        await _viewModel.LoadAsync();
    }

    protected override void OnDisappearing()
    {
        _viewModel.PropertyChanged -= OnViewModelChanged;
        base.OnDisappearing();
    }

    // Hộp thoại là trách nhiệm của View; quy tắc xóa/lưu nằm ở phía service.
    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_confirmingDelete || !_viewModel.CanEdit || sender is not Button { BindingContext: TaskRow row }) return;
        _confirmingDelete = true;
        try
        {
            if (await DisplayAlertAsync("Xóa việc này?", row.Title, "Xóa", "Giữ lại"))
                await _viewModel.DeleteAsync(row.Id);
        }
        finally { _confirmingDelete = false; }
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TasksViewModel.ErrorMessage) && _viewModel.HasError)
            SemanticScreenReader.Default.Announce(_viewModel.ErrorMessage);
        else if (e.PropertyName == nameof(TasksViewModel.StatusMessage))
            SemanticScreenReader.Default.Announce(_viewModel.StatusMessage);
    }
}
