namespace MauiApp1.Models;

public sealed record WorkbookRow(int RowNumber, string Vietnamese, string English, string? ErrorKey);
public sealed record WorkbookPreview(string SheetName, IReadOnlyList<WorkbookRow> Rows)
{
    public string FirstLanguage { get; init; } = "vi";
    public string SecondLanguage { get; init; } = "en";
}
