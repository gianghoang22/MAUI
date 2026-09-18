#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Platform;
using Windows.System;

namespace MauiApp1.Views;

public partial class InteractionDialog : Microsoft.Maui.Controls.ContentView
{
    private UIElement? keyboardRoot;
    private Microsoft.UI.Xaml.Controls.Control? previousFocus;

    private partial async Task<string?> ShowPlatformAsync()
    {
        var owner = Shell.Current;
        var context = owner.Handler!.MauiContext!;
        var root = ((FrameworkElement)owner.Handler.PlatformView!).XamlRoot;
        var content = (FrameworkElement)this.ToPlatform(context);
        var popup = new Popup { XamlRoot = root, Child = content, IsLightDismissEnabled = false };
        void Resize(XamlRoot sender, XamlRootChangedEventArgs arguments)
        {
            content.Width = sender.Size.Width;
            content.Height = sender.Size.Height;
        }
        content.Width = root.Size.Width;
        content.Height = root.Size.Height;
        root.Changed += Resize;
        popup.Closed += OnPopupClosed;
        try
        {
            popup.IsOpen = true;
            return await Result;
        }
        finally
        {
            root.Changed -= Resize;
            popup.Closed -= OnPopupClosed;
            popup.IsOpen = false;
            popup.Child = null;
        }
    }

    private void OnPopupClosed(object? sender, object arguments) => Complete(null);

    partial void CaptureFocus()
    {
        if (Shell.Current.Handler?.PlatformView is FrameworkElement { XamlRoot: { } root })
            previousFocus = FocusManager.GetFocusedElement(root) as Microsoft.UI.Xaml.Controls.Control;
        previousFocus ??= Shell.Current.CurrentPage.GetVisualTreeDescendants()
            .OfType<Microsoft.Maui.Controls.Button>()
            .FirstOrDefault(button => button.IsVisible && button.AutomationId?.EndsWith("ActionsButton", StringComparison.Ordinal) == true)
            ?.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.Control;
    }

    partial void RestoreFocus()
    {
        var target = previousFocus;
        previousFocus = null;
        if (target is null) return;
        Shell.Current.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(60), () =>
        {
            if (target is { IsLoaded: true, IsEnabled: true }) target.Focus(FocusState.Programmatic);
        });
    }

    partial void ConnectKeyboard()
    {
        if (keyboardRoot is not null || Handler?.PlatformView is not UIElement root) return;
        keyboardRoot = root;
        root.TabFocusNavigation = KeyboardNavigationMode.Cycle;
        root.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnDialogKeyDown), true);
    }

    partial void DisconnectKeyboard()
    {
        keyboardRoot?.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnDialogKeyDown));
        keyboardRoot = null;
    }

    private void OnDialogKeyDown(object sender, KeyRoutedEventArgs arguments)
    {
        if (arguments.Key != VirtualKey.Escape) return;
        arguments.Handled = true;
        Complete(null);
    }
}
#endif
