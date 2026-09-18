using System.Windows.Input;

namespace MauiApp1.Controls;

public partial class VocabularyFlashcard : ContentView
{
    public static readonly BindableProperty PromptProperty = BindableProperty.Create(nameof(Prompt), typeof(string), typeof(VocabularyFlashcard), "", propertyChanged: OnPromptChanged);
    public static readonly BindableProperty AnswerProperty = BindableProperty.Create(nameof(Answer), typeof(string), typeof(VocabularyFlashcard), "", propertyChanged: OnContentChanged);
    public static readonly BindableProperty IsRevealedProperty = BindableProperty.Create(nameof(IsRevealed), typeof(bool), typeof(VocabularyFlashcard), false, propertyChanged: OnRevealChanged);
    public static readonly BindableProperty IsTwoSidedProperty = BindableProperty.Create(nameof(IsTwoSided), typeof(bool), typeof(VocabularyFlashcard), false, propertyChanged: OnContentChanged);
    public static readonly BindableProperty FrontCaptionProperty = BindableProperty.Create(nameof(FrontCaption), typeof(string), typeof(VocabularyFlashcard), "", propertyChanged: OnContentChanged);
    public static readonly BindableProperty BackCaptionProperty = BindableProperty.Create(nameof(BackCaption), typeof(string), typeof(VocabularyFlashcard), "", propertyChanged: OnContentChanged);
    public static readonly BindableProperty FlipCommandProperty = BindableProperty.Create(nameof(FlipCommand), typeof(ICommand), typeof(VocabularyFlashcard));
    public static readonly BindableProperty FaceTextProperty = BindableProperty.Create(nameof(FaceText), typeof(string), typeof(VocabularyFlashcard), "");
    public static readonly BindableProperty FaceCaptionProperty = BindableProperty.Create(nameof(FaceCaption), typeof(string), typeof(VocabularyFlashcard), "");
    public static readonly BindableProperty ShowSecondaryAnswerProperty = BindableProperty.Create(nameof(ShowSecondaryAnswer), typeof(bool), typeof(VocabularyFlashcard), false);
    private Border? surface;
    private int animationVersion;
    private bool questionChanging;

    public VocabularyFlashcard()
    {
        InitializeComponent();
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (IsTwoSided && FlipCommand?.CanExecute(null) == true) FlipCommand.Execute(null); };
        GestureRecognizers.Add(tap);
        Unloaded += (_, _) => ResetSurface();
    }

    public string Prompt { get => (string)GetValue(PromptProperty); set => SetValue(PromptProperty, value); }
    public string Answer { get => (string)GetValue(AnswerProperty); set => SetValue(AnswerProperty, value); }
    public bool IsRevealed { get => (bool)GetValue(IsRevealedProperty); set => SetValue(IsRevealedProperty, value); }
    public bool IsTwoSided { get => (bool)GetValue(IsTwoSidedProperty); set => SetValue(IsTwoSidedProperty, value); }
    public string FrontCaption { get => (string)GetValue(FrontCaptionProperty); set => SetValue(FrontCaptionProperty, value); }
    public string BackCaption { get => (string)GetValue(BackCaptionProperty); set => SetValue(BackCaptionProperty, value); }
    public ICommand? FlipCommand { get => (ICommand?)GetValue(FlipCommandProperty); set => SetValue(FlipCommandProperty, value); }
    public string FaceText { get => (string)GetValue(FaceTextProperty); private set => SetValue(FaceTextProperty, value); }
    public string FaceCaption { get => (string)GetValue(FaceCaptionProperty); private set => SetValue(FaceCaptionProperty, value); }
    public bool ShowSecondaryAnswer { get => (bool)GetValue(ShowSecondaryAnswerProperty); private set => SetValue(ShowSecondaryAnswerProperty, value); }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        surface = GetTemplateChild("CardSurface") as Border;
        UpdateFace();
    }

    private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var card = (VocabularyFlashcard)bindable;
        card.ResetSurface();
        card.UpdateFace();
    }

    private static void OnRevealChanged(BindableObject bindable, object oldValue, object newValue) =>
        _ = ((VocabularyFlashcard)bindable).FlipAsync();

    private static void OnPromptChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var card = (VocabularyFlashcard)bindable;
        card.questionChanging = true;
        card.ResetSurface();
        card.UpdateFace();
        card.Dispatcher.Dispatch(() => { card.questionChanging = false; card.UpdateFace(); });
    }

    private void UpdateFace()
    {
        FaceText = IsTwoSided && IsRevealed && !questionChanging ? Answer : Prompt;
        FaceCaption = IsTwoSided && IsRevealed && !questionChanging ? BackCaption : FrontCaption;
        ShowSecondaryAnswer = !IsTwoSided && IsRevealed && !questionChanging;
    }

    private void ResetSurface()
    {
        animationVersion++;
        if (surface is null) return;
        surface.CancelAnimations();
        surface.AbortAnimation("FlashcardFlip");
        surface.RotationY = 0;
#if WINDOWS
        if (surface.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
            element.Projection = null;
#endif
    }

    private async Task FlipAsync()
    {
        ResetSurface();
        var version = animationVersion;
        var target = surface;
        if (!IsTwoSided || questionChanging || target?.Handler is null || !MotionEnabled) { UpdateFace(); return; }
        try
        {
            var direction = IsRevealed ? 1 : -1;
            await RotateFaceAsync(target, 0, direction * 90, 140, Easing.CubicIn);
            if (version != animationVersion) return;
            UpdateFace();
            await RotateFaceAsync(target, -direction * 90, 0, 180, Easing.CubicOut);
        }
        catch (Exception exception) { System.Diagnostics.Debug.WriteLine(exception); }
        finally
        {
            if (version == animationVersion) { ResetSurface(); UpdateFace(); }
        }
    }

    private static Task RotateFaceAsync(Border target, double from, double to, uint duration, Easing easing)
    {
#if WINDOWS
        if (target.Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement element) return Task.CompletedTask;
        var projection = element.Projection as Microsoft.UI.Xaml.Media.PlaneProjection
            ?? new Microsoft.UI.Xaml.Media.PlaneProjection();
        element.Projection = projection;
        projection.RotationY = from;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        new Animation(angle => projection.RotationY = angle, from, to, easing)
            .Commit(target, "FlashcardFlip", length: duration, finished: (_, _) => completion.TrySetResult());
        return completion.Task;
#else
        target.RotationY = from;
        return target.RotateYToAsync(to, duration, easing);
#endif
    }

#if WINDOWS
    private static bool MotionEnabled => new Windows.UI.ViewManagement.UISettings().AnimationsEnabled;
#else
    private static bool MotionEnabled => true;
#endif
}
