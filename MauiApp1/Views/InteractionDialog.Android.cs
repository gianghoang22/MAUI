#if ANDROID
using Android.App;
using Android.Graphics.Drawables;
using Android.Views;
using Microsoft.Maui.Platform;

namespace MauiApp1.Views;

public partial class InteractionDialog : Microsoft.Maui.Controls.ContentView
{
    private partial async Task<string?> ShowPlatformAsync()
    {
        var context = Shell.Current.Handler!.MauiContext!;
        using var dialog = new Dialog(Platform.CurrentActivity!);
        dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
        var content = this.ToPlatform(context);
        dialog.SetContentView(content);
        dialog.SetCancelable(true);
        dialog.DismissEvent += OnNativeDismissed;
        var window = dialog.Window!;
        window.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));
        window.ClearFlags(WindowManagerFlags.DimBehind);
        window.SetSoftInputMode(SoftInput.AdjustResize);
        try
        {
            dialog.Show();
            window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            return await Result;
        }
        finally
        {
            dialog.DismissEvent -= OnNativeDismissed;
            dialog.Dismiss();
        }
    }

    private void OnNativeDismissed(object? sender, EventArgs arguments) => Complete(null);
}
#endif
