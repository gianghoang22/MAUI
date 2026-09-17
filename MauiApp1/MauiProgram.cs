using Microsoft.Extensions.Logging;
using MauiApp1.Controls;
using MauiApp1.Localization;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using MauiApp1.Views;

namespace MauiApp1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });
#if DEBUG
        builder.Logging.AddDebug();
#endif
        VocabularyEntryHandler.Configure();
        builder.Services.AddSingleton<LocalizationService>();
        builder.Services.AddSingleton<IVocabularyRepository>(_ => new JsonVocabularyRepository(FileSystem.AppDataDirectory));
        builder.Services.AddSingleton<IUserInteraction, ShellUserInteraction>();
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);
        builder.Services.AddSingleton<EditorLifecycle>();
        builder.Services.AddSingleton<LearningEngine>();
        builder.Services.AddSingleton<XlsxWorkbook>();
        builder.Services.AddSingleton<IWorkbookFiles, WorkbookFiles>();
        builder.Services.AddSingleton<IPronunciation, Pronunciation>();
        builder.Services.AddSingleton<LibraryViewModel>();
        builder.Services.AddSingleton<HistoryViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddTransient<ClassViewModel>();
        builder.Services.AddTransient<DeckViewModel>();
        builder.Services.AddTransient<CardEditorViewModel>();
        builder.Services.AddTransient<ImportViewModel>();
        builder.Services.AddTransient<LearningViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<HistoryPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddTransient<ClassPage>();
        builder.Services.AddTransient<DeckPage>();
        builder.Services.AddTransient<CardEditorPage>();
        builder.Services.AddTransient<ImportPage>();
        builder.Services.AddTransient<LearningPage>();
        builder.Services.AddSingleton<AppShell>();
        return builder.Build();
    }
}
