using MauiBasics.Data;
using MauiBasics.Domain;
using MauiBasics.ViewModels;
using MauiBasics.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MauiBasics;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<App>();

        // Composition root: chỉ nơi này biết interface dùng implementation nào.
        builder.Services.AddSingleton<ITodoRepository>(_ =>
            new JsonTodoRepository(Path.Combine(FileSystem.AppDataDirectory, "todos.json")));
        builder.Services.AddSingleton<TodoService>();
        builder.Services.AddSingleton<TasksViewModel>();
        builder.Services.AddSingleton<LearnViewModel>();
        // Android có thể tạo lại Activity khi đổi cỡ chữ: tạo View/handler mới,
        // giữ ViewModel để trạng thái trong tiến trình không bị mất.
        builder.Services.AddTransient<TasksPage>();
        builder.Services.AddTransient<LearnPage>();
        builder.Services.AddTransient<AppShell>();

        return builder.Build();
    }
}
