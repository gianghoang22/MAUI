namespace MauiBasics.Domain;

// Gom ba thao tác nhỏ vào một service để ví dụ không có quá nhiều lớp UseCase.
public sealed class TodoService(ITodoRepository repository)
{
    public const int MaxTitleLength = 100;

    public Task<IReadOnlyList<TodoItem>> LoadAsync() => repository.LoadAsync();

    public async Task<IReadOnlyList<TodoItem>> AddAsync(IReadOnlyList<TodoItem> items, string title)
    {
        var cleaned = title.Trim();
        if (cleaned.Length == 0)
            throw new ArgumentException("Bạn hãy nhập tên việc cần làm.");
        if (cleaned.Length > MaxTitleLength)
            throw new ArgumentException($"Tên việc tối đa {MaxTitleLength} ký tự.");

        TodoItem[] updated = [new(Guid.NewGuid(), cleaned, false), .. items];
        await repository.SaveAsync(updated);
        return updated; // Chỉ trả trạng thái mới sau khi lưu thành công.
    }

    public async Task<IReadOnlyList<TodoItem>> ToggleAsync(IReadOnlyList<TodoItem> items, Guid id)
    {
        if (!items.Any(item => item.Id == id)) return items;
        var updated = items.Select(item => item.Id == id
            ? item with { IsCompleted = !item.IsCompleted } : item).ToArray();
        await repository.SaveAsync(updated);
        return updated;
    }

    public async Task<IReadOnlyList<TodoItem>> DeleteAsync(IReadOnlyList<TodoItem> items, Guid id)
    {
        if (!items.Any(item => item.Id == id)) return items;
        var updated = items.Where(item => item.Id != id).ToArray();
        await repository.SaveAsync(updated);
        return updated;
    }
}
