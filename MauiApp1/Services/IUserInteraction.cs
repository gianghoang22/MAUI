namespace MauiApp1.Services;

public interface IUserInteraction
{
    Task NavigateAsync(string route);
    Task AlertAsync(string title, string message);
    Task<bool> ConfirmAsync(string title, string message, string acceptKey = "Delete");
}
