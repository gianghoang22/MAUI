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
