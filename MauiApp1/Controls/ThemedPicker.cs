using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using MauiApp1.Views;

namespace MauiApp1.Controls;

public sealed class ThemedPicker : Button
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<string>), typeof(ThemedPicker), propertyChanged: OnItemsSourceChanged);
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(ThemedPicker), -1, BindingMode.TwoWay, propertyChanged: OnSelectionChanged);
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(ThemedPicker), "", propertyChanged: OnSelectionChanged);
    private bool isOpen;
    private INotifyCollectionChanged? observedItems;

    public IEnumerable<string>? ItemsSource { get => (IEnumerable<string>?)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    public ThemedPicker()
    {
        SetDynamicResource(StyleProperty, "ThemedPickerStyle");
        Clicked += OnChoose;
        Loaded += (_, _) => { ObserveItems(); RefreshText(); };
        Unloaded += (_, _) => StopObservingItems();
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var picker = (ThemedPicker)bindable;
        picker.StopObservingItems();
        if (picker.IsLoaded) picker.ObserveItems();
        picker.RefreshText();
    }

    private static void OnSelectionChanged(BindableObject bindable, object oldValue, object newValue) => ((ThemedPicker)bindable).RefreshText();

    private void ObserveItems()
    {
        StopObservingItems();
        observedItems = ItemsSource as INotifyCollectionChanged;
        if (observedItems is not null) observedItems.CollectionChanged += OnItemsChanged;
    }

    private void StopObservingItems()
    {
        if (observedItems is not null) observedItems.CollectionChanged -= OnItemsChanged;
        observedItems = null;
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs arguments) => RefreshText();

    private void RefreshText()
    {
        var items = ItemsSource?.ToArray() ?? [];
        Text = SelectedIndex >= 0 && SelectedIndex < items.Length ? items[SelectedIndex] : Title;
        SemanticProperties.SetDescription(this, string.IsNullOrWhiteSpace(Title) ? Text : $"{Title}: {Text}");
    }

    private async void OnChoose(object? sender, EventArgs arguments)
    {
        if (isOpen || !IsEnabled || ItemsSource is null) return;
        var items = ItemsSource.ToArray();
        if (items.Length == 0) return;
        var context = BindingContext;
        isOpen = true;
        try
        {
            Focus();
            var resources = Application.Current!.Resources;
            var dialog = InteractionDialog.Selection(Title, (string)resources["L10n.Cancel"], items, SelectedIndex,
                (string)resources["L10n.VSelectedOption"]);
            var result = await dialog.ShowAsync();
            if (IsLoaded && IsEnabled && ReferenceEquals(context, BindingContext) && items.SequenceEqual(ItemsSource ?? []) &&
                int.TryParse(result, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index >= 0 && index < items.Length)
                SelectedIndex = index;
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            var resources = Application.Current!.Resources;
            await InteractionDialog.Message((string)resources["L10n.ErrorTitle"], (string)resources["L10n.ErrorUnexpected"], (string)resources["L10n.Cancel"]).ShowAsync();
        }
        finally
        {
            isOpen = false;
        }
    }
}
