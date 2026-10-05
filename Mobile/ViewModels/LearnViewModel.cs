namespace MauiBasics.ViewModels;

public sealed class LearnViewModel : ObservableObject
{
    private int _count;

    public LearnViewModel()
    {
        IncrementCommand = new Command(() => Count++);
        ResetCommand = new Command(() => Count = 0, () => Count > 0);
    }

    public int Count
    {
        get => _count;
        private set
        {
            if (!SetProperty(ref _count, value)) return;
            OnPropertyChanged(nameof(CounterDescription));
            ResetCommand.ChangeCanExecute();
        }
    }

    public string CounterDescription => $"Bạn đã bấm {Count} lần";
    public string DeviceDescription => $"{DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model}\n{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}";
    public Command IncrementCommand { get; }
    public Command ResetCommand { get; }
}
