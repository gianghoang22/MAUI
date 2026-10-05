using MauiBasics.Views;

namespace MauiBasics;

public partial class AppShell : Shell
{
    public AppShell(TasksPage tasksPage, LearnPage learnPage)
    {
        InitializeComponent();
        TasksTab.Content = tasksPage;
        LearnTab.Content = learnPage;
    }
}
