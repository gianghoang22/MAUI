namespace MauiApp1.Controls;

public partial class VocabularyFlashcard : ContentView
{
    public static readonly BindableProperty PromptProperty = BindableProperty.Create(nameof(Prompt), typeof(string), typeof(VocabularyFlashcard), "");
    public static readonly BindableProperty AnswerProperty = BindableProperty.Create(nameof(Answer), typeof(string), typeof(VocabularyFlashcard), "");
    public static readonly BindableProperty IsRevealedProperty = BindableProperty.Create(nameof(IsRevealed), typeof(bool), typeof(VocabularyFlashcard), false);

    public VocabularyFlashcard() => InitializeComponent();
    public string Prompt { get => (string)GetValue(PromptProperty); set => SetValue(PromptProperty, value); }
    public string Answer { get => (string)GetValue(AnswerProperty); set => SetValue(AnswerProperty, value); }
    public bool IsRevealed { get => (bool)GetValue(IsRevealedProperty); set => SetValue(IsRevealedProperty, value); }
}
