namespace MauiApp1.Services;

public static class SystemBarAppearance
{
    public static void Apply()
    {
#if ANDROID
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var application = Application.Current;
            var window = Platform.CurrentActivity?.Window;
            if (application is null || window is null) return;
            var theme = application.UserAppTheme == AppTheme.Unspecified
                ? application.RequestedTheme : application.UserAppTheme;
            var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView);
            if (controller is null) return;
            controller.AppearanceLightStatusBars = theme != AppTheme.Dark;
            controller.AppearanceLightNavigationBars = theme != AppTheme.Dark;
        });
#endif
    }
}
