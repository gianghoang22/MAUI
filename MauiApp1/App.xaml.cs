using Microsoft.Extensions.DependencyInjection;
using MauiApp1.Localization;
using MauiApp1.Services;

namespace MauiApp1;

public partial class App : Application
{
    private readonly IServiceProvider services;
    private readonly LocalizationService localization;
    private readonly EditorLifecycle lifecycle;

    public App(IServiceProvider services, IPreferences preferences, LocalizationService localization, EditorLifecycle lifecycle)
    {
        this.services = services;
        this.localization = localization;
        this.lifecycle = lifecycle;
        var language = preferences.Get("vocabmate.language", preferences.Get("studymate.language", "vi"));
        localization.SetLanguage(language == "en" ? "en" : "vi");
        InitializeComponent();
        ApplyLanguageResources();
        localization.LanguageChanged += OnLanguageChanged;
        ViewModels.SettingsViewModel.ApplyTheme(preferences.Get("vocabmate.theme", preferences.Get("studymate.theme", 0)));
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(services.GetRequiredService<AppShell>()) { Title = "VocabMate" };
        window.Stopped += OnStopped;
        window.Resumed += OnResumed;
        window.Destroying += OnStopped;
        return window;
    }

    private async void OnStopped(object? sender, EventArgs arguments) => await lifecycle.FlushAsync();

    private async void OnResumed(object? sender, EventArgs arguments)
    {
        await lifecycle.FlushAsync();
        if (Shell.Current.CurrentPage.BindingContext is IRefreshable refreshable && refreshable is not IEditorCheckpoint)
            await refreshable.RefreshAsync();
    }

    private void OnLanguageChanged(object? sender, EventArgs arguments) => ApplyLanguageResources();

    private void ApplyLanguageResources()
    {
        foreach (var resource in localization.GetResources()) Resources[$"L10n.{resource.Key}"] = resource.Value;
    }
}
