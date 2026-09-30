using MauiApp1.Views;

namespace MauiApp1;

public partial class AppShell : Shell
{
    public AppShell(HomePage home, MainPage library, HistoryPage history, SettingsPage settings)
    {
        InitializeComponent();
        HomeContent.Content = home;
        LibraryContent.Content = library;
        HistoryContent.Content = history;
        SettingsContent.Content = settings;
        Navigated += (_, _) => UpdateNavigationLayout(Window?.Width ?? Width);
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
        UpdateNavigationLayout(Window?.Width ?? Width);
    }

    public void UpdateNavigationLayout(double width)
    {
        var sidebar = Controls.LayoutMetrics.UseSidebar(width);
        FlyoutBehavior = sidebar ? FlyoutBehavior.Locked : FlyoutBehavior.Disabled;
        Shell.SetTabBarIsVisible(this, !sidebar);
        if (CurrentPage is { } page) Shell.SetTabBarIsVisible(page, !sidebar);
        foreach (var item in Items)
        {
            foreach (var section in item.Items)
                foreach (var content in section.Items)
                    Shell.SetTabBarIsVisible(content, !sidebar);
            item.Handler?.UpdateValue(Shell.TabBarIsVisibleProperty.PropertyName);
        }
    }
}
