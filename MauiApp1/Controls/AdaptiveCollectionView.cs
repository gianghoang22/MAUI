using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MauiApp1.Services;

namespace MauiApp1.Controls;

public sealed class AdaptiveCollectionView : CollectionView
{
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(IEnumerable), typeof(AdaptiveCollectionView), propertyChanged: OnSourceChanged);
    public static readonly BindableProperty TileTemplateProperty = BindableProperty.Create(nameof(TileTemplate), typeof(DataTemplate), typeof(AdaptiveCollectionView), propertyChanged: OnTemplateChanged);
    private readonly ObservableCollection<TileRow> rows = [];
    private INotifyCollectionChanged? observedSource;
    private bool updatePending;

    public IEnumerable? Source { get => (IEnumerable?)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public DataTemplate? TileTemplate { get => (DataTemplate?)GetValue(TileTemplateProperty); set => SetValue(TileTemplateProperty, value); }

    public AdaptiveCollectionView()
    {
        ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 16 };
        SelectionMode = SelectionMode.None;
        ItemsSource = rows;
        ItemTemplate = new DataTemplate(() => new TileRowView());
        SizeChanged += (_, _) => ScheduleUpdate();
        Loaded += (_, _) => { ObserveSource(); ScheduleUpdate(); };
        Unloaded += (_, _) => StopObserving();
    }

    private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var collection = (AdaptiveCollectionView)bindable;
        collection.ObserveSource();
        collection.ScheduleUpdate();
    }

    private static void OnTemplateChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((AdaptiveCollectionView)bindable).ScheduleUpdate();

    private void ObserveSource()
    {
        StopObserving();
        observedSource = Source as INotifyCollectionChanged;
        if (observedSource is not null) observedSource.CollectionChanged += OnItemsChanged;
    }

    private void StopObserving()
    {
        if (observedSource is not null) observedSource.CollectionChanged -= OnItemsChanged;
        observedSource = null;
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs arguments) => ScheduleUpdate();

    private void ScheduleUpdate()
    {
        if (updatePending) return;
        updatePending = true;
        Dispatcher.Dispatch(() => { updatePending = false; UpdateRows(); });
    }

    private void UpdateRows()
    {
        if (TileTemplate is null || Width <= 0) return;
        var columns = Math.Clamp((int)((Width + 16) / 296), 1, 3);
        var items = Source?.Cast<object>().ToList() ?? [];
        var desired = new List<TileRow>();
        for (var offset = 0; offset < items.Count; offset += columns)
        {
            var rowItems = items.Skip(offset).Take(columns).ToArray();
            var previous = desired.Count < rows.Count ? rows[desired.Count] : null;
            desired.Add(previous is not null && previous.Columns == columns && previous.Template == TileTemplate && previous.Items.SequenceEqual(rowItems)
                ? previous : new TileRow(rowItems, columns, TileTemplate));
        }
        CollectionUpdates.Apply(rows, desired);
    }

    private sealed record TileRow(IReadOnlyList<object> Items, int Columns, DataTemplate Template);

    private sealed class TileRowView : Grid
    {
        public TileRowView() => ColumnSpacing = 16;

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            Children.Clear();
            ColumnDefinitions.Clear();
            if (BindingContext is not TileRow row) return;
            for (var column = 0; column < row.Columns; column++) ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (var index = 0; index < row.Items.Count; index++)
            {
                var tile = (View)row.Template.CreateContent();
                tile.BindingContext = row.Items[index];
                this.Add(tile, index, 0);
            }
        }
    }
}
