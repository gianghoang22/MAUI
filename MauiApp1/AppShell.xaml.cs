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
        // Luồng mở màn: thư viện -> lớp -> bộ từ -> sửa thẻ / import / học.
        // ID đi trong query của route; màn đích nhận ID rồi tự tải dữ liệu.
        Routing.RegisterRoute("class", typeof(ClassPage));
        Routing.RegisterRoute("deck", typeof(DeckPage));
        Routing.RegisterRoute("card", typeof(CardEditorPage));
        Routing.RegisterRoute("import", typeof(ImportPage));
        Routing.RegisterRoute("learn", typeof(LearningPage));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Services.SystemBarAppearance.Apply();
    }
}
