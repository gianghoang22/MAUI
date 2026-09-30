namespace MauiApp1.Controls;

public static class LayoutMetrics
{
    public const double MaximumContentWidth = 1360;

    public static bool UseSidebar(double width) => width >= 1100;

    public static int CompactColumns(double width) => width >= 640 ? 2 : 1;

    public static bool UseSplitWorkspace(double width, double height) => width >= 1000 && height >= 560;

    public static double PageInset(double width) => width < 600 ? 20 : width < 1200 ? 28 : 40;

    public static int TileColumns(double width) => Math.Clamp((int)((Math.Max(0, width) + 16) / 336), 1, 3);
}
