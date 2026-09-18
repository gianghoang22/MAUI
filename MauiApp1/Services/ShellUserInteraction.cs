using MauiApp1.Localization;
using MauiApp1.Views;

namespace MauiApp1.Services;

public sealed class ShellUserInteraction(LocalizationService localization) : IUserInteraction
{
    private readonly SemaphoreSlim dialogGate = new(1, 1);

    public Task NavigateAsync(string route) => Shell.Current.GoToAsync(route);

    public Task AlertAsync(string title, string message) =>
        ShowDialogAsync(() => InteractionDialog.Message(title, message, localization["Close"]));

    public async Task<bool> ConfirmAsync(string title, string message, string acceptKey = "Delete") =>
        await ShowDialogAsync(() => InteractionDialog.Message(title, message, localization["Cancel"],
            localization[acceptKey], acceptKey is "Delete" or "VDiscard")) is not null;

    public Task<string?> ActionSheetAsync(string title, string cancel, string? destruction, params string[] buttons) =>
        ShowDialogAsync(() => InteractionDialog.Actions(title, cancel, destruction, buttons));

    public Task<string?> PromptAsync(string title, string message, string? initialValue = "") =>
        ShowDialogAsync(() => InteractionDialog.Prompt(title, message, localization["Cancel"], localization["VSave"], initialValue));

    private async Task<string?> ShowDialogAsync(Func<InteractionDialog> createDialog)
    {
        await dialogGate.WaitAsync();
        try
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var dialog = createDialog();
                return await dialog.ShowAsync();
            });
        }
        finally
        {
            dialogGate.Release();
        }
    }
}
