namespace MauiApp1.Controls;

public sealed class CollectionWorkspace : Grid
{
    public static readonly BindableProperty FormProperty = BindableProperty.Create(nameof(Form), typeof(View), typeof(CollectionWorkspace), propertyChanged: OnContentChanged);
    public static readonly BindableProperty CollectionProperty = BindableProperty.Create(nameof(Collection), typeof(CollectionView), typeof(CollectionWorkspace), propertyChanged: OnContentChanged);
    public static readonly BindableProperty ListHeadingProperty = BindableProperty.Create(nameof(ListHeading), typeof(View), typeof(CollectionWorkspace), propertyChanged: OnContentChanged);
    private readonly ScrollView formScroll = new();
    private readonly VerticalStackLayout mobileHeader = new() { Spacing = 24, Margin = new Thickness(0, 0, 0, 24) };
    private bool? isWide;
    private bool layoutPending;

    public View? Form { get => (View?)GetValue(FormProperty); set => SetValue(FormProperty, value); }
    public CollectionView? Collection { get => (CollectionView?)GetValue(CollectionProperty); set => SetValue(CollectionProperty, value); }
    public View? ListHeading { get => (View?)GetValue(ListHeadingProperty); set => SetValue(ListHeadingProperty, value); }

    public CollectionWorkspace()
    {
        MaximumWidthRequest = LayoutMetrics.MaximumContentWidth;
        ColumnSpacing = 32;
        RowSpacing = 16;
        SizeChanged += (_, _) => ScheduleLayout();
    }

    private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue is CollectionView previous) previous.Header = null;
        ((CollectionWorkspace)bindable).Rebuild();
    }

    private void Rebuild()
    {
        if (Collection is not null) Collection.Header = null;
        formScroll.Content = null;
        mobileHeader.Children.Clear();
        Children.Clear();
        if (Collection is null) return;
        var wide = LayoutMetrics.UseSplitWorkspace(Width, Height);
        isWide = wide;
        ColumnDefinitions.Clear();
        RowDefinitions.Clear();
        RowDefinitions.Add(new RowDefinition(GridLength.Star));
        ColumnDefinitions.Add(new ColumnDefinition(wide ? new GridLength(0.36, GridUnitType.Star) : GridLength.Star));
        if (wide)
        {
            ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.64, GridUnitType.Star)));
            formScroll.Content = Form;
            Children.Add(formScroll);
            Collection.Header = ListHeading;
        }
        else
        {
            if (Form is not null) mobileHeader.Children.Add(Form);
            if (ListHeading is not null) mobileHeader.Children.Add(ListHeading);
            Collection.Header = mobileHeader;
        }
        Grid.SetColumn(Collection, wide ? 1 : 0);
        Grid.SetRow(Collection, 0);
        Children.Add(Collection);
        ScheduleLayout();
    }

    private void ScheduleLayout()
    {
        if (layoutPending) return;
        layoutPending = true;
        Dispatcher.Dispatch(() => { layoutPending = false; UpdateLayout(); });
    }

    private void UpdateLayout()
    {
        if (Collection is null || Width <= 0) return;
        var wide = LayoutMetrics.UseSplitWorkspace(Width, Height);
        var inset = LayoutMetrics.PageInset(Width);
        if (Padding.Left != inset) Padding = new Thickness(inset);
        if (isWide != wide) Rebuild();
    }
}
