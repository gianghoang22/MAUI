using MauiApp1.Localization;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly IPreferences preferences;
    private int selectedTheme;
    private bool isRefreshingLanguage;

    public SettingsViewModel(IPreferences preferences, IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.preferences = preferences;
        selectedTheme = Math.Clamp(preferences.Get("vocabmate.theme", preferences.Get("studymate.theme", 0)), 0, 2);
    }

    public IReadOnlyList<string> Themes => [Localization["ThemeSystem"], Localization["ThemeLight"], Localization["ThemeDark"]];
    public IReadOnlyList<string> Languages { get; } = ["Tiếng Việt", "English"];
    public int SelectedLanguage
    {
        get => Localization.LanguageCode == "en" ? 1 : 0;
        set
        {
            if (isRefreshingLanguage || value is < 0 or > 1 || value == SelectedLanguage)
                return;
            var languageCode = value == 1 ? "en" : "vi";
            preferences.Set("vocabmate.language", languageCode);
            Localization.SetLanguage(languageCode);
        }
    }
    public string DataLocation => Path.Combine(FileSystem.AppDataDirectory, "vocabmate.json");
    public int SelectedTheme
    {
        get => selectedTheme;
        set
        {
            if (isRefreshingLanguage || value is < 0 or > 2 || !SetProperty(ref selectedTheme, value))
                return;
            preferences.Set("vocabmate.theme", value);
            ApplyTheme(value);
        }
    }

    protected override void OnLanguageChanged(object? sender, EventArgs arguments)
    {
        isRefreshingLanguage = true;
        try
        {
            base.OnLanguageChanged(sender, arguments);
            OnPropertyChanged(nameof(SelectedTheme));
        }
        finally
        {
            isRefreshingLanguage = false;
        }
    }

    public static void ApplyTheme(int selected) => Application.Current!.UserAppTheme = selected switch
    {
        1 => AppTheme.Light,
        2 => AppTheme.Dark,
        _ => AppTheme.Unspecified
    };
}
