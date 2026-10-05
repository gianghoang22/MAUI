using System.Text.Json;
using System.Text.Json.Serialization;
using MauiBasics.Domain;

namespace MauiBasics.Data;

public sealed class JsonTodoRepository(string filePath) : ITodoRepository
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<IReadOnlyList<TodoItem>> LoadAsync()
    {
        await _gate.WaitAsync();
        try
        {
            // Chỉ tệp chưa tồn tại mới là danh sách mới; tệp lỗi không bị ghi đè.
            FileStream stream;
            try { stream = File.OpenRead(filePath); }
            catch (FileNotFoundException) { return []; }
            catch (DirectoryNotFoundException) { return []; }

            await using (stream)
            {
                var items = await JsonSerializer.DeserializeAsync(stream, TodoJsonContext.Default.TodoItemArray);
                if (items is null || items.Any(item => item is null || item.Id == Guid.Empty ||
                        string.IsNullOrWhiteSpace(item.Title) || item.Title.Length > TodoService.MaxTitleLength) ||
                    items.Select(item => item.Id).Distinct().Count() != items.Length)
                    throw new InvalidDataException("Dữ liệu công việc không hợp lệ.");
                return items;
            }
        }
        finally { _gate.Release(); }
    }

    public async Task SaveAsync(IReadOnlyList<TodoItem> items)
    {
        await _gate.WaitAsync();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
            var temporaryPath = filePath + ".tmp";
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, items.ToArray(), TodoJsonContext.Default.TodoItemArray);
                await stream.FlushAsync();
            }
            // Thay thế sau khi ghi xong, tránh để tệp chính chứa JSON viết dở.
            File.Move(temporaryPath, filePath, overwrite: true);
        }
        finally { _gate.Release(); }
    }
}

// Sinh metadata lúc build, không phụ thuộc reflection khi Android Release trimming.
[JsonSerializable(typeof(TodoItem[]))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class TodoJsonContext : JsonSerializerContext;
