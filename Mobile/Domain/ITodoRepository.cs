namespace MauiBasics.Domain;

public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> LoadAsync();
    Task SaveAsync(IReadOnlyList<TodoItem> items);
}
