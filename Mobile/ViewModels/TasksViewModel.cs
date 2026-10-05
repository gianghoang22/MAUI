using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using MauiBasics.Domain;

namespace MauiBasics.ViewModels;

public sealed class TasksViewModel : ObservableObject
{
    private readonly TodoService _service;
    private IReadOnlyList<TodoItem> _items = [];
    private string _newTitle = "";
    private string _errorMessage = "";
    private string _statusMessage = "Mỗi việc nhỏ là một bước tiến.";
    private bool _isBusy;
    private bool _isLoaded;

    public TasksViewModel(TodoService service)
    {
        _service = service;
        AddCommand = new Command(async () => await AddAsync(), () => CanEdit);
        ToggleCommand = new Command<Guid>(async id => await ToggleAsync(id), _ => CanEdit);
        RetryCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
    }

    public ObservableCollection<TaskRow> Items { get; } = [];
    public Command AddCommand { get; }
    public Command<Guid> ToggleCommand { get; }
    public Command RetryCommand { get; }

    public string NewTitle
    {
        get => _newTitle;
        set { if (SetProperty(ref _newTitle, value)) ErrorMessage = ""; }
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        private set { if (SetProperty(ref _errorMessage, value)) OnPropertyChanged(nameof(HasError)); }
    }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool HasError => ErrorMessage.Length > 0;
    public bool IsBusy => _isBusy;
    public bool CanEdit => _isLoaded && !IsBusy;
    public bool NeedsReload => !_isLoaded && !IsBusy;
    public string Summary => $"{_items.Count(item => item.IsCompleted)}/{_items.Count} việc đã hoàn thành";

    public async Task LoadAsync()
    {
        if (_isLoaded || IsBusy) return;
        await RunAsync(async () =>
        {
            Apply(await _service.LoadAsync());
            _isLoaded = true;
            StatusMessage = "Thay đổi được lưu tự động trên máy.";
        });
    }

    public async Task AddAsync()
    {
        if (!CanEdit) return;
        await RunAsync(async () =>
        {
            Apply(await _service.AddAsync(_items, NewTitle));
            NewTitle = "";
            StatusMessage = "Đã thêm và lưu việc mới.";
        });
    }

    public async Task ToggleAsync(Guid id)
    {
        if (!CanEdit) return;
        await RunAsync(async () =>
        {
            Apply(await _service.ToggleAsync(_items, id));
            StatusMessage = "Đã lưu trạng thái công việc.";
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        if (!CanEdit) return;
        await RunAsync(async () =>
        {
            Apply(await _service.DeleteAsync(_items, id));
            StatusMessage = "Đã xóa và lưu danh sách.";
        });
    }

    private void Apply(IReadOnlyList<TodoItem> items)
    {
        _items = items;
        Items.Clear();
        foreach (var item in items) Items.Add(new TaskRow(item, ToggleCommand));
        OnPropertyChanged(nameof(Summary));
    }

    // Mọi callback async từ Command đều đi qua đây: có loading và xử lý lỗi.
    private async Task RunAsync(Func<Task> operation)
    {
        SetBusy(true);
        ErrorMessage = "";
        try { await operation(); }
        catch (ArgumentException exception) { ErrorMessage = exception.Message; }
        catch (Exception exception) when (exception is JsonException or InvalidDataException)
        {
            ErrorMessage = "Không đọc được dữ liệu đã lưu. Tệp gốc vẫn được giữ nguyên. Hãy thử tải lại hoặc xem hướng dẫn khôi phục trong README.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ErrorMessage = "Không thể đọc hoặc lưu trên máy. Kiểm tra dung lượng trống rồi thử lại; thay đổi chưa được áp dụng.";
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            ErrorMessage = "Có lỗi khi xử lý công việc. Bạn hãy thử lại.";
        }
        finally { SetBusy(false); }
    }

    private void SetBusy(bool value)
    {
        _isBusy = value;
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(NeedsReload));
        AddCommand.ChangeCanExecute();
        ToggleCommand.ChangeCanExecute();
        RetryCommand.ChangeCanExecute();
    }
}
