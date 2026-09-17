namespace MauiApp1.Controls;

public static class VocabularyEntryHandler
{
    public static void Configure()
    {
#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("VocabMate.Entry", (handler, view) =>
        {
            if (view is VocabularyEntry)
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        });
#endif
    }
}
