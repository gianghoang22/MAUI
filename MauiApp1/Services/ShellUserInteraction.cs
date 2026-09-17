using MauiApp1.Localization;

namespace MauiApp1.Services;

public sealed class ShellUserInteraction(LocalizationService localization) : IUserInteraction
{
    public Task NavigateAsync(string route) => Shell.Current.GoToAsync(route);

    public Task AlertAsync(string title, string message) =>
        Shell.Current.DisplayAlertAsync(title, message, localization["Close"]);

    public Task<bool> ConfirmAsync(string title, string message, string acceptKey = "Delete") =>
        Shell.Current.DisplayAlertAsync(title, message, localization[acceptKey], localization["Cancel"]);
}
