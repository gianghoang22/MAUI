using MauiApp1.Views;

namespace MauiApp1;

public partial class AppShell : Shell
{
    public AppShell(MainPage library, HistoryPage history, SettingsPage settings)
    {
        InitializeComponent();
        LibraryContent.Content = library;
        HistoryContent.Content = history;
        SettingsContent.Content = settings;
        Routing.RegisterRoute("class", typeof(ClassPage));
        Routing.RegisterRoute("deck", typeof(DeckPage));
        Routing.RegisterRoute("card", typeof(CardEditorPage));
        Routing.RegisterRoute("import", typeof(ImportPage));
        Routing.RegisterRoute("learn", typeof(LearningPage));
    }
}
