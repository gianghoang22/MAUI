using System.Text.Json;
using MauiBasics.Data;
using MauiBasics.Domain;
using MauiBasics.ViewModels;
using Xunit;

namespace MauiBasics.Tests;

public sealed class TodoTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "MauiBasicsTests", Guid.NewGuid().ToString("N"));
    private string FilePath => Path.Combine(_directory, "todos.json");

    [Fact]
    public async Task NewInstallStartsEmpty()
    {
        Assert.Empty(await new JsonTodoRepository(FilePath).LoadAsync());
    }

    [Fact]
    public async Task AddToggleDeleteSurviveNewRepositoryInstances()
    {
        var service = new TodoService(new JsonTodoRepository(FilePath));
        var added = await service.AddAsync([], "  Học MAUI tiếng Việt  ");
        var item = Assert.Single(await new JsonTodoRepository(FilePath).LoadAsync());
        Assert.Equal("Học MAUI tiếng Việt", item.Title);
        Assert.NotEqual(Guid.Empty, item.Id);
        var completed = await service.ToggleAsync(added, item.Id);
        Assert.True(Assert.Single(await new JsonTodoRepository(FilePath).LoadAsync()).IsCompleted);
        var reopened = await service.ToggleAsync(completed, item.Id);
        Assert.False(Assert.Single(reopened).IsCompleted);
        await service.DeleteAsync(reopened, item.Id);
        Assert.Empty(await new JsonTodoRepository(FilePath).LoadAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public async Task BlankTitlesAreRejectedWithoutWriting(string title)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new TodoService(new JsonTodoRepository(FilePath)).AddAsync([], title));
        Assert.False(File.Exists(FilePath));
    }

    [Fact]
    public async Task TitleLengthBoundaryIsEnforced()
    {
        var service = new TodoService(new JsonTodoRepository(FilePath));
        var items = await service.AddAsync([], new string('a', 100));
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddAsync(items, new string('a', 101)));
        Assert.Single(await service.LoadAsync());
    }

    [Fact]
    public async Task SameTitlesRemainIndependentById()
    {
        var service = new TodoService(new JsonTodoRepository(FilePath));
        var items = await service.AddAsync([], "Đọc sách");
        items = await service.AddAsync(items, "Đọc sách");
        Assert.NotEqual(items[0].Id, items[1].Id);
        items = await service.DeleteAsync(items, items[0].Id);
        Assert.Single(items);
    }

    [Theory]
    [InlineData("{broken json")]
    [InlineData("null")]
    [InlineData("[null]")]
    [InlineData("[{\"Id\":\"00000000-0000-0000-0000-000000000000\",\"Title\":\"bad\"}]")]
    public async Task CorruptDataIsReportedAndPreserved(string contents)
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(FilePath, contents);
        var error = await Record.ExceptionAsync(() => new JsonTodoRepository(FilePath).LoadAsync());
        Assert.True(error is JsonException or InvalidDataException);
        Assert.Equal(contents, await File.ReadAllTextAsync(FilePath));
    }

    [Fact]
    public async Task FailedSaveKeepsInputAndExistingUiState()
    {
        var repository = new FakeRepository();
        var viewModel = new TasksViewModel(new TodoService(repository));
        await viewModel.LoadAsync();
        viewModel.NewTitle = "Chưa được lưu";
        repository.FailSave = true;
        await viewModel.AddAsync();
        Assert.Empty(viewModel.Items);
        Assert.Equal("Chưa được lưu", viewModel.NewTitle);
        Assert.True(viewModel.HasError);
        Assert.True(viewModel.CanEdit);
    }

    [Fact]
    public async Task RapidAddsAreGatedWhileSaveIsPending()
    {
        var repository = new FakeRepository { PendingSave = new() };
        var viewModel = new TasksViewModel(new TodoService(repository));
        await viewModel.LoadAsync();
        viewModel.NewTitle = "Chỉ thêm một lần";
        var first = viewModel.AddAsync();
        Assert.True(viewModel.IsBusy);
        Assert.False(viewModel.AddCommand.CanExecute(null));
        await viewModel.AddAsync();
        repository.PendingSave.SetResult();
        await first;
        Assert.Single(viewModel.Items);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task ReadFailureBlocksEditsUntilSuccessfulRetry()
    {
        var repository = new FakeRepository { FailLoad = true };
        var viewModel = new TasksViewModel(new TodoService(repository));
        await viewModel.LoadAsync();
        Assert.True(viewModel.NeedsReload);
        Assert.False(viewModel.CanEdit);
        viewModel.NewTitle = "Không được ghi đè tệp lỗi";
        await viewModel.AddAsync();
        Assert.Equal(0, repository.SaveCount);
        repository.FailLoad = false;
        await viewModel.LoadAsync();
        Assert.True(viewModel.CanEdit);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public async Task FailedToggleAndDeleteDoNotChangeExistingItem()
    {
        var repository = new FakeRepository();
        var viewModel = new TasksViewModel(new TodoService(repository));
        await viewModel.LoadAsync();
        viewModel.NewTitle = "Giữ nguyên";
        await viewModel.AddAsync();
        var id = Assert.Single(viewModel.Items).Id;
        repository.FailSave = true;
        await viewModel.ToggleAsync(id);
        Assert.False(Assert.Single(viewModel.Items).IsCompleted);
        await viewModel.DeleteAsync(id);
        Assert.Equal(id, Assert.Single(viewModel.Items).Id);
    }

    public void Dispose()
    {
        // Chỉ thư mục tạm có GUID do từng test tự tạo.
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }

    private sealed class FakeRepository : ITodoRepository
    {
        public bool FailSave { get; set; }
        public bool FailLoad { get; set; }
        public int SaveCount { get; private set; }
        public TaskCompletionSource? PendingSave { get; init; }
        private IReadOnlyList<TodoItem> _items = [];

        public Task<IReadOnlyList<TodoItem>> LoadAsync() => FailLoad
            ? Task.FromException<IReadOnlyList<TodoItem>>(new IOException("Test read failure"))
            : Task.FromResult(_items);

        public async Task SaveAsync(IReadOnlyList<TodoItem> items)
        {
            SaveCount++;
            if (FailSave) throw new IOException("Test write failure");
            if (PendingSave is not null) await PendingSave.Task;
            _items = items;
        }
    }
}
