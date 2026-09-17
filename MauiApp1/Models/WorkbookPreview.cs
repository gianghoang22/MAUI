namespace MauiApp1.Models;

public sealed record WorkbookRow(int RowNumber, string Vietnamese, string English, string? ErrorKey);
public sealed record WorkbookPreview(string SheetName, IReadOnlyList<WorkbookRow> Rows);
