namespace MauiApp1.Controls;

public sealed class CollectionWorkspace : Grid
{
    public static readonly BindableProperty FormProperty = BindableProperty.Create(nameof(Form), typeof(View), typeof(CollectionWorkspace), propertyChanged: OnContentChanged);
    public static readonly BindableProperty CollectionProperty = BindableProperty.Create(nameof(Collection), typeof(CollectionView), typeof(CollectionWorkspace), propertyChanged: OnContentChanged);
    public static readonly BindableProperty ListHeadingProperty = BindableProperty.Create(nameof(ListHeading), typeof(View), typeof(CollectionWorkspace), propertyChanged: OnContentChanged);
    private readonly ScrollView formScroll = new();
    private readonly Grid listPanel = new() { RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star)], RowSpacing = 12 };
    private readonly VerticalStackLayout mobileHeader = new() { Spacing = 12 };

    public View? Form { get => (View?)GetValue(FormProperty); set => SetValue(FormProperty, value); }
    public CollectionView? Collection { get => (CollectionView?)GetValue(CollectionProperty); set => SetValue(CollectionProperty, value); }
    public View? ListHeading { get => (View?)GetValue(ListHeadingProperty); set => SetValue(ListHeadingProperty, value); }

    public CollectionWorkspace()
    {
        Padding = 16;
        ColumnSpacing = 20;
        RowSpacing = 16;
        SizeChanged += (_, _) => UpdateLayout();
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
        listPanel.Children.Clear();
        Children.Clear();
        if (Collection is null) return;
        Collection.Header = null;
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            formScroll.Content = Form;
            Children.Add(formScroll);
            if (ListHeading is not null) listPanel.Add(ListHeading, 0, 0);
            listPanel.Add(Collection, 0, 1);
            Children.Add(listPanel);
        }
        else
        {
            if (Form is not null) mobileHeader.Children.Add(Form);
            if (ListHeading is not null) mobileHeader.Children.Add(ListHeading);
            Collection.Header = mobileHeader;
            Children.Add(Collection);
        }
        UpdateLayout();
    }

    private void UpdateLayout()
    {
        if (Collection is null || DeviceInfo.Platform != DevicePlatform.WinUI) return;
        var wide = Width >= 900;
        var sidebar = Math.Clamp((Width - Padding.HorizontalThickness - ColumnSpacing) * 0.3, 280, 400);
        formScroll.MaximumHeightRequest = wide ? double.PositiveInfinity : Math.Max(140, (Height - Padding.VerticalThickness) * 0.48);
        Grid.SetColumn((BindableObject)listPanel, wide ? 1 : 0);
        Grid.SetRow((BindableObject)listPanel, wide ? 0 : 1);
        if (ColumnDefinitions.Count != (wide ? 2 : 1))
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();
            ColumnDefinitions.Add(new ColumnDefinition { Width = wide ? new GridLength(sidebar) : GridLength.Star });
            RowDefinitions.Add(new RowDefinition { Height = wide ? GridLength.Star : GridLength.Auto });
            if (wide) ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            else RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        }
        else if (wide) ColumnDefinitions[0].Width = new GridLength(sidebar);
    }
}
