using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiApp1.Localization;

public abstract class LocalizedObject : INotifyPropertyChanged
{
    protected LocalizedObject(LocalizationService localization)
    {
        Localization = localization;
        localization.LanguageChanged += OnLanguageChanged;
    }

    protected LocalizationService Localization { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnLanguageChanged(object? sender, EventArgs arguments) => OnPropertyChanged(null);

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
