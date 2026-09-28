namespace MauiApp1.Controls;

public static class EditorScrollBarHandler
{
    public static void Configure()
    {
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("VocabMate.HiddenScrollBars", (handler, view) =>
        {
#if WINDOWS
            Microsoft.UI.Xaml.Controls.ScrollViewer.SetVerticalScrollBarVisibility(
                handler.PlatformView, Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden);
            Microsoft.UI.Xaml.Controls.ScrollViewer.SetHorizontalScrollBarVisibility(
                handler.PlatformView, Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden);
#elif ANDROID
            handler.PlatformView.VerticalScrollBarEnabled = false;
            handler.PlatformView.HorizontalScrollBarEnabled = false;
#endif
        });
    }
}
