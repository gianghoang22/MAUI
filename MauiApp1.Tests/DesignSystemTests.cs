using System.Globalization;
using System.Xml.Linq;
using MauiApp1.Controls;
using Xunit;

namespace MauiApp1.Tests;

public sealed class DesignSystemTests
{
    [Theory]
    [InlineData(360, 800, false)]
    [InlineData(600, 960, false)]
    [InlineData(999, 800, false)]
    [InlineData(1000, 559, false)]
    [InlineData(1000, 560, true)]
    [InlineData(1280, 800, true)]
    [InlineData(1360, 500, false)]
    public void WorkspaceOnlySplitsWhenBothPanesHaveRoom(double width, double height, bool expected)
    {
        Assert.Equal(expected, LayoutMetrics.UseSplitWorkspace(width, height));
    }

    [Theory]
    [InlineData(360, 20)]
    [InlineData(599, 20)]
    [InlineData(600, 28)]
    [InlineData(1199, 28)]
    [InlineData(1200, 40)]
    public void GuttersFollowViewportSize(double width, double expected)
    {
        Assert.Equal(expected, LayoutMetrics.PageInset(width));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(320, 1)]
    [InlineData(655, 1)]
    [InlineData(656, 2)]
    [InlineData(991, 2)]
    [InlineData(992, 3)]
    [InlineData(2000, 3)]
    public void TilesKeepReadableMinimumWidths(double width, int expected)
    {
        Assert.Equal(expected, LayoutMetrics.TileColumns(width));
    }

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void TextColorsMeetContrastTargetAcrossSurfaces(string theme)
    {
        var colors = ReadColors();
        foreach (var foreground in new[] { "TextPrimary", "TextMuted", "AccentText" })
        foreach (var background in new[] { "PageBackground", "Surface", "Field", "AccentSubtle", "AccentSoft" })
        {
            var ratio = Contrast(colors[foreground + theme], colors[background + theme]);
            Assert.True(ratio >= 4.5, $"{foreground}/{background} ({theme}): {ratio:F2}:1");
        }
        Assert.True(Contrast(colors["DangerText" + theme], colors["DangerSurface" + theme]) >= 4.5);
        var primaryText = theme == "Light" ? "#FFFFFF" : colors["PageBackgroundDark"];
        Assert.True(Contrast(primaryText, colors["AccentSelected" + theme]) >= 4.5);
    }

    [Fact]
    public void HeroTextRemainsReadable()
    {
        var colors = ReadColors();
        Assert.True(Contrast(colors["HeroMuted"], colors["HeroBackground"]) >= 4.5);
        Assert.True(Contrast("#FFFFFF", colors["HeroBackground"]) >= 4.5);
    }

    [Fact]
    public void SharedInputsMeetAndroidTouchTargetHeight()
    {
        var document = ReadTheme();
        XNamespace maui = "http://schemas.microsoft.com/dotnet/2021/maui";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2009/xaml";
        foreach (var target in new[] { "Entry", "Picker", "Editor", "SearchBar", "BaseButton", "DialogActionButton" })
        {
            var style = document.Root!.Elements(maui + "Style").Single(element =>
                target.EndsWith("Button", StringComparison.Ordinal) ? (string?)element.Attribute(xaml + "Key") == target :
                (string?)element.Attribute("TargetType") == target && element.Attribute(xaml + "Key") is null);
            var height = style.Elements(maui + "Setter").Single(element =>
                (string?)element.Attribute("Property") == "MinimumHeightRequest");
            Assert.True(double.Parse(height.Attribute("Value")!.Value, CultureInfo.InvariantCulture) >= 48);
        }
    }

    private static XDocument ReadTheme() => XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Design", "StudyTheme.xaml"));

    private static Dictionary<string, string> ReadColors()
    {
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2009/xaml";
        return ReadTheme().Root!.Elements().Where(element => element.Name.LocalName == "Color")
            .ToDictionary(element => element.Attribute(xaml + "Key")!.Value, element => element.Value);
    }

    private static double Contrast(string foreground, string background)
    {
        var first = Luminance(foreground);
        var second = Luminance(background);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    private static double Luminance(string color)
    {
        var channels = Enumerable.Range(0, 3).Select(index =>
        {
            var channel = int.Parse(color.AsSpan(1 + index * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
            return channel <= 0.04045 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
        }).ToArray();
        return channels[0] * 0.2126 + channels[1] * 0.7152 + channels[2] * 0.0722;
    }
}
