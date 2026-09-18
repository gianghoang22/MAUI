using System.Diagnostics;
using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public abstract class ViewModelBase(IUserInteraction interaction, LocalizationService localization) : LocalizedObject(localization)
{
    private bool isBusy;
    protected IUserInteraction Interaction { get; } = interaction;

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetProperty(ref isBusy, value))
                OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    public bool IsNotBusy => !IsBusy;
    public ICommand BackCommand => CreateCommand(() => Interaction.NavigateAsync(".."));
    protected ICommand CreateCommand(Func<Task> action) => new Command(async () => await RunAsync(action));

    protected sealed record MenuAction(string TextKey, ICommand Command, Func<bool>? IsAvailable = null, bool IsDestructive = false);

    protected ICommand CreateMenuCommand(Func<string> title, params MenuAction[] actions) => new Command(async () =>
    {
        MenuAction? selected = null;
        await RunAsync(async () =>
        {
            var available = actions.Where(action => (action.IsAvailable?.Invoke() ?? true) && action.Command.CanExecute(null))
                .Select(action => (Action: action, Text: Localization[action.TextKey])).ToList();
            if (available.Count == 0) return;
            var choice = await Interaction.ActionSheetAsync(title(), Localization["Cancel"],
                available.FirstOrDefault(item => item.Action.IsDestructive).Text,
                available.Where(item => !item.Action.IsDestructive).Select(item => item.Text).ToArray());
            selected = available.FirstOrDefault(item => item.Text == choice).Action;
        });
        if (selected is not null && (selected.IsAvailable?.Invoke() ?? true) && selected.Command.CanExecute(null))
            selected.Command.Execute(null);
    });

    public async Task RunAsync(Func<Task> action)
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            var message = exception is StudyException studyException
                ? Localization[studyException.ResourceKey]
                : Localization["ErrorUnexpected"];
            await Interaction.AlertAsync(Localization["ErrorTitle"], message);
        }
        finally
        {
            IsBusy = false;
        }
    }

}
