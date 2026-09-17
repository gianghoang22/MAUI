namespace MauiApp1.Controls;

public sealed class AdaptiveColumns : Grid
{
    public AdaptiveColumns()
    {
        ColumnSpacing = 12;
        RowSpacing = 12;
        SizeChanged += (_, _) => ArrangeColumns();
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        ArrangeColumns();
    }

    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        ArrangeColumns();
    }

    private void ArrangeColumns()
    {
        var capacity = Math.Clamp((int)((Math.Max(0, Width) + ColumnSpacing) / (280 + ColumnSpacing)), 1, 3);
        var columns = Math.Min(Math.Max(1, Children.Count), capacity);
        if (columns == 3 && Children.Count % 3 != 0) columns = 2;
        var rows = (Children.Count + columns - 1) / columns;
        if (ColumnDefinitions.Count != columns || RowDefinitions.Count != rows)
        {
            ColumnDefinitions.Clear();
            RowDefinitions.Clear();
            for (var column = 0; column < columns; column++) ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (var row = 0; row < rows; row++) RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }
        for (var index = 0; index < Children.Count; index++)
        {
            Grid.SetColumn((BindableObject)Children[index], index % columns);
            Grid.SetRow((BindableObject)Children[index], index / columns);
        }
    }
}
