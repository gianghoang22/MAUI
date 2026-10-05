namespace MauiBasics.Domain;

// Model thuần C#: không biết XAML, Android, màu sắc hay cách lưu JSON.
public sealed record TodoItem(Guid Id, string Title, bool IsCompleted);
