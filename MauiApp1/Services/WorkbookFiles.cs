using MauiApp1.Models;

namespace MauiApp1.Services;

public interface IWorkbookFiles
{
    Task<WorkbookPreview?> PickAsync();
    Task ShareAsync(IEnumerable<VocabularyCard> cards);
}

public sealed class WorkbookFiles(XlsxWorkbook workbook) : IWorkbookFiles
{
    public async Task<WorkbookPreview?> PickAsync()
    {
        var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.WinUI] = [".xlsx"],
            [DevicePlatform.Android] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]
        });
        var selected = await FilePicker.Default.PickAsync(new PickOptions { FileTypes = fileTypes });
        if (selected is null) return null;
        if (!selected.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)) throw new StudyException("VImportInvalid");
        await using var stream = await selected.OpenReadAsync();
        return await workbook.ReadAsync(stream);
    }

    public async Task ShareAsync(IEnumerable<VocabularyCard> cards)
    {
        var path = Path.Combine(FileSystem.CacheDirectory, $"VocabMate-{Guid.NewGuid():N}.xlsx");
        await using (var stream = File.Create(path)) workbook.Write(stream, cards);
        await Share.Default.RequestAsync(new ShareFileRequest { Title = "VocabMate", File = new ShareFile(path) });
    }
}
