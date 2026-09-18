namespace MauiApp1.Views;

public partial class InteractionDialog : Microsoft.Maui.Controls.ContentView
{
    private readonly TaskCompletionSource<string?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool isPrompt;
    private bool hasAccept;
    private bool updatingLayout;

    public Task<string?> Result => completion.Task;

    private InteractionDialog(string title, string cancel)
    {
        InitializeComponent();
        DialogHeading.Text = title;
        CancelButton.Text = cancel;
        CaptureFocus();
        Loaded += OnDialogLoaded;
    }

    public async Task<string?> ShowAsync()
    {
        Parent = Shell.Current.CurrentPage;
        try
        {
            return await ShowPlatformAsync();
        }
        finally
        {
            DisconnectKeyboard();
            Handler?.DisconnectHandler();
            Parent = null;
            RestoreFocus();
        }
    }

    public static InteractionDialog Actions(string title, string cancel, string? destruction, IEnumerable<string> actions)
    {
        var dialog = new InteractionDialog(title, cancel);
        foreach (var action in actions)
        {
            var button = new Button
            {
                Text = action,
                Style = (Style)Application.Current!.Resources["DialogActionButton"],
                AutomationId = $"DialogAction{dialog.ActionList.Children.Count}"
            };
            button.Clicked += (_, _) => dialog.Complete(action);
            dialog.ActionList.Children.Add(button);
        }
        dialog.ActionList.IsVisible = dialog.ActionList.Children.Count > 0;
        dialog.DestructiveActions.IsVisible = !string.IsNullOrEmpty(destruction);
        dialog.DestructiveButton.Text = destruction;
        return dialog;
    }

    public static InteractionDialog Message(string title, string message, string cancel, string? accept = null, bool destructive = false)
    {
        var dialog = new InteractionDialog(title, cancel);
        dialog.DialogMessage.Text = message;
        dialog.DialogMessage.IsVisible = !string.IsNullOrEmpty(message);
        dialog.hasAccept = accept is not null;
        dialog.AcceptButton.Text = accept;
        dialog.AcceptButton.IsVisible = dialog.hasAccept;
        if (destructive)
            dialog.AcceptButton.Style = (Style)Application.Current!.Resources["DialogDangerButton"];
        return dialog;
    }

    public static InteractionDialog Prompt(string title, string message, string cancel, string accept, string? initialValue)
    {
        var dialog = Message(title, message, cancel, accept);
        dialog.isPrompt = true;
        dialog.PromptField.IsVisible = true;
        dialog.PromptEntry.Text = initialValue ?? "";
        SemanticProperties.SetDescription(dialog.PromptEntry, message);
        return dialog;
    }

    private void OnDialogLoaded(object? sender, EventArgs arguments)
    {
        ConnectKeyboard();
        UpdateDialogSize();
        Dispatcher.Dispatch(() =>
        {
            if (completion.Task.IsCompleted) return;
            if (isPrompt) PromptEntry.Focus();
            else if (ActionList.Children.FirstOrDefault() is Button firstAction) firstAction.Focus();
            else CancelButton.Focus();
        });
    }

    private void OnDialogSizeChanged(object? sender, EventArgs arguments) => UpdateDialogSize();

    private void UpdateDialogSize()
    {
        if (updatingLayout || DialogHost is null || DialogSurface is null || DialogFooter is null || DialogHost.Width <= 0 || DialogHost.Height <= 0) return;
        updatingLayout = true;
        try
        {
            var compact = DialogHost.Width < 600 || DialogHost.Height < 550;
            var scale = compact ? 0.93 : 1.0;
            var inset = compact ? 16 : 24;
            var padding = compact ? 12 : 16;
            var gap = compact ? 10 : 12;
            var width = Math.Min(isPrompt || hasAccept ? 400 : 360, Math.Max(0, DialogHost.Width - inset * 2));
            DialogSurface.WidthRequest = width;
            DialogSurface.Margin = inset;
            DialogSurface.Padding = padding;
            DialogLayout.RowSpacing = gap;
            DialogBody.Spacing = gap;
            DestructiveActions.Spacing = gap;
            DialogHeading.FontSize = Math.Round(18 * scale, 1);
            DialogMessage.FontSize = Math.Round(14 * scale, 1);
            PromptEntry.FontSize = Math.Round(14 * scale, 1);
            foreach (var button in ActionList.Children.OfType<Button>().Concat([CancelButton, AcceptButton, DestructiveButton]))
                button.FontSize = Math.Round(14 * scale, 1);
            var stacked = hasAccept && width < 300;
            DialogFooter.RowSpacing = stacked ? 8 : 0;
            if (DialogFooter.RowDefinitions.Count != (stacked ? 2 : 1))
            {
                DialogFooter.RowDefinitions.Clear();
                DialogFooter.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                if (stacked) DialogFooter.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }
            Grid.SetRow(CancelButton, stacked ? 1 : 0);
            Grid.SetColumnSpan(CancelButton, stacked || !hasAccept ? 2 : 1);
            Grid.SetColumn(AcceptButton, stacked ? 0 : 1);
            Grid.SetColumnSpan(AcceptButton, stacked ? 2 : 1);
            var chromeHeight = inset * 2 + DialogSurface.Padding.VerticalThickness
                + DialogSurface.StrokeThickness * 2 + DialogLayout.RowSpacing * 3 + 1;
            var minimumFooter = CancelButton.MinimumHeightRequest * (stacked ? 2 : 1) + DialogFooter.RowSpacing;
            DialogScroll.MaximumHeightRequest = Math.Min(320, Math.Max(0, DialogHost.Height - chromeHeight
                - Math.Max(24, DialogHeading.Height) - Math.Max(minimumFooter, DialogFooter.Height)));
        }
        finally
        {
            updatingLayout = false;
        }
    }

    private void OnCancelClicked(object? sender, EventArgs arguments) => Complete(null);
    private void OnDestructiveClicked(object? sender, EventArgs arguments) => Complete(DestructiveButton.Text);
    private void OnAcceptClicked(object? sender, EventArgs arguments) => Complete(isPrompt ? PromptEntry.Text ?? "" : AcceptButton.Text);
    private void OnPromptCompleted(object? sender, EventArgs arguments) => OnAcceptClicked(sender, arguments);

    private void Complete(string? result)
    {
        if (!completion.TrySetResult(result)) return;
        DialogSurface.IsEnabled = false;
        PromptEntry.Unfocus();
    }

    private partial Task<string?> ShowPlatformAsync();
    partial void CaptureFocus();
    partial void RestoreFocus();
    partial void ConnectKeyboard();
    partial void DisconnectKeyboard();
}
