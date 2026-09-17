using MauiApp1.Services;

namespace MauiApp1.Controls;

public sealed class RequiredTermBehavior : Behavior<Entry>
{
    public static readonly BindableProperty IsValidProperty = BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(RequiredTermBehavior), false);
    public bool IsValid { get => (bool)GetValue(IsValidProperty); private set => SetValue(IsValidProperty, value); }

    protected override void OnAttachedTo(Entry bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.TextChanged += OnTextChanged;
        IsValid = VocabularyRules.IsValidTerm(bindable.Text);
    }

    protected override void OnDetachingFrom(Entry bindable)
    {
        bindable.TextChanged -= OnTextChanged;
        base.OnDetachingFrom(bindable);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs arguments) => IsValid = VocabularyRules.IsValidTerm(arguments.NewTextValue);
}
