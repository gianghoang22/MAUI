#if WINDOWS
namespace MauiApp1.Controls;

public sealed class StudyButtonHandler : Microsoft.Maui.Handlers.ButtonHandler
{
    public override void UpdateValue(string property)
    {
        if (((Microsoft.Maui.IElementHandler)this).PlatformView is not null)
            base.UpdateValue(property);
    }
}
#endif
