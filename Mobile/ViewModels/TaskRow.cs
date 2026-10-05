using System.Windows.Input;
using MauiBasics.Domain;

namespace MauiBasics.ViewModels;

// Chữ hiển thị thuộc presentation, không đặt trong model nghiệp vụ.
public sealed class TaskRow(TodoItem item, ICommand toggleCommand)
{
    public Guid Id => item.Id;
    public string Title => item.Title;
    public bool IsCompleted => item.IsCompleted;
    public string Status => item.IsCompleted ? "Đã hoàn thành" : "Chưa hoàn thành";
    public string ToggleText => item.IsCompleted ? "Làm lại" : "Hoàn thành";
    public string ToggleDescription => $"{ToggleText}: {Title}";
    public string DeleteDescription => $"Xóa việc: {Title}";
    public ICommand ToggleCommand { get; } = toggleCommand;
}
