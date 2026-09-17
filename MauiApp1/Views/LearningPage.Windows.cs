#if WINDOWS
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using Windows.UI.Core;
using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class LearningPage
{
    private UIElement? keyboardRoot;
    private string? focusState;

    partial void ConnectKeyboard()
    {
        if (keyboardRoot is not null || Handler?.PlatformView is not UIElement root) return;
        keyboardRoot = root;
        root.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnStudyKeyDown), true);
        viewModel.PropertyChanged += OnStudyChanged;
        QueueStudyFocus();
    }

    partial void DisconnectKeyboard()
    {
        keyboardRoot?.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnStudyKeyDown));
        keyboardRoot = null;
        focusState = null;
        viewModel.PropertyChanged -= OnStudyChanged;
    }

    private void OnStudyChanged(object? sender, PropertyChangedEventArgs arguments)
    {
        if (string.IsNullOrEmpty(arguments.PropertyName) || arguments.PropertyName == nameof(LearningViewModel.IsBusy))
            QueueStudyFocus();
    }

    private void QueueStudyFocus() => Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(60), () =>
    {
        if (!isActive || viewModel.IsBusy || viewModel.ShowSetup || keyboardRoot is null) return;
        var state = $"{viewModel.QuestionText}:{viewModel.Position}:{viewModel.ShowWritten}:{viewModel.ShowNext}:{viewModel.ShowFlip}:{viewModel.ShowRating}:{viewModel.ShowChoices}";
        if (state == focusState) return;
        Microsoft.Maui.Controls.VisualElement? target = viewModel.ShowWritten ? WrittenAnswerEntry :
            viewModel.ShowNext ? NextQuestionButton :
            viewModel.ShowFlip ? FlipButton :
            viewModel.ShowRating ? RememberButton :
            viewModel.ShowChoices ? ChoiceList.Children.FirstOrDefault() as Microsoft.Maui.Controls.Button :
            viewModel.ShowResult && viewModel.CanContinue ? ContinueLearningButton :
            viewModel.ShowResult ? StudySetupButton : null;
        if (target?.Focus() == true) focusState = state;
    });

    private void OnStudyKeyDown(object sender, KeyRoutedEventArgs arguments)
    {
        if (arguments.Handled || arguments.KeyStatus.WasKeyDown || viewModel.IsBusy || viewModel.ShowSetup || !isActive || keyboardRoot?.XamlRoot is null) return;
        if (IsPressed(VirtualKey.Control) || IsPressed(VirtualKey.Menu) || IsPressed(VirtualKey.Shift)) return;
        var focused = FocusManager.GetFocusedElement(keyboardRoot.XamlRoot) as DependencyObject;
        for (var current = focused; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is TextBox or PasswordBox or RichEditBox or ComboBox) return;
            if (current is ButtonBase && arguments.Key is VirtualKey.Enter or VirtualKey.Space) return;
        }

        ICommand? command = null;
        var number = arguments.Key >= VirtualKey.Number1 && arguments.Key <= VirtualKey.Number4
            ? (int)arguments.Key - (int)VirtualKey.Number1
            : arguments.Key >= VirtualKey.NumberPad1 && arguments.Key <= VirtualKey.NumberPad4
                ? (int)arguments.Key - (int)VirtualKey.NumberPad1 : -1;
        if (viewModel.ShowChoices && number >= 0 && number < viewModel.Choices.Count)
            command = viewModel.Choices[number].SelectCommand;
        else if (viewModel.ShowRating && number is 0 or 1)
            command = number == 0 ? viewModel.RememberCommand : viewModel.ForgetCommand;
        else if (arguments.Key is VirtualKey.Space or VirtualKey.Enter)
            command = viewModel.ShowNext ? viewModel.NextCommand : viewModel.ShowFlip ? viewModel.FlipCommand : null;
        if (command?.CanExecute(null) != true) return;
        arguments.Handled = true;
        command.Execute(null);
    }

    private static bool IsPressed(VirtualKey key) =>
        (InputKeyboardSource.GetKeyStateForCurrentThread(key) & CoreVirtualKeyStates.Down) != 0;
}
#endif
