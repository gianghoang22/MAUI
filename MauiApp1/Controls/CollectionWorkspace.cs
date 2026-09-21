namespace MauiApp1.Controls;

public sealed class CollectionWorkspace : Grid
{
    public static readonly BindableProperty FormProperty = BindableProperty.Create(nameof(Form), typeof(View), typeof(CollectionWorkspace), propertyChanged: OnContentChanged);
    public static readonly BindableProperty CollectionProperty = BindableProperty.Create(nameof(Collection), typeof(CollectionView), typeof(CollectionWorkspace), propertyChanged: OnContentChanged);
    public static readonly BindableProperty ListHeadingProperty = BindableProperty.Create(nameof(ListHeading), typeof(View), typeof(CollectionWorkspace), propertyChanged: OnContentChanged);
    private readonly ScrollView formScroll = new();
    private readonly VerticalStackLayout mobileHeader = new() { Spacing = 16, Margin = new Thickness(0, 0, 0, 16) };
    private bool? isWide;
    private bool layoutPending;

    public View? Form { get => (View?)GetValue(FormProperty); set => SetValue(FormProperty, value); }
    public CollectionView? Collection { get => (CollectionView?)GetValue(CollectionProperty); set => SetValue(CollectionProperty, value); }
    public View? ListHeading { get => (View?)GetValue(ListHeadingProperty); set => SetValue(ListHeadingProperty, value); }

    public CollectionWorkspace()
    {
        ColumnSpacing = 24;
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
        formScroll.Content = null;
        mobileHeader.Children.Clear();
        Children.Clear();
        isWide = null;
        if (Collection is null) return;
        Collection.Header = null;
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
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
        var desktop = DeviceInfo.Platform == DevicePlatform.WinUI;
        var wide = desktop && Width >= 900 && Height >= 520;
        var inset = Width >= 1400 ? 40 : wide ? 24 : 16;
        if (Padding.Left != inset) Padding = new Thickness(inset);
        formScroll.MaximumHeightRequest = wide ? double.PositiveInfinity : Math.Clamp((Height - Padding.VerticalThickness) * 0.3, 120, 240);
        if (isWide != wide)
        {
            isWide = wide;
            ColumnDefinitions.Clear();
            RowDefinitions.Clear();
            ColumnDefinitions.Add(new ColumnDefinition(wide ? new GridLength(0.32, GridUnitType.Star) : GridLength.Star));
            RowDefinitions.Add(new RowDefinition(desktop && !wide ? GridLength.Auto : GridLength.Star));
            if (wide) ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.68, GridUnitType.Star)));
            else if (desktop) RowDefinitions.Add(new RowDefinition(GridLength.Star));
            Grid.SetColumn(Collection, wide ? 1 : 0);
            Grid.SetRow(Collection, desktop && !wide ? 1 : 0);
        }
    }
}
