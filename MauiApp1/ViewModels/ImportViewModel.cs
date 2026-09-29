using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public sealed class ImportViewModel : ViewModelBase, IRefreshable
{
    private readonly IVocabularyRepository repository;
    private Guid deckId;
    private string sheetName = "";
    private string firstLanguage = "";
    private string secondLanguage = "";
    private string languageErrorKey = "";
    private VocabularyData? data;
    private WorkbookPreview? workbook;
    private WorkbookPreview? prepared;
    private IReadOnlyList<ImportRow> rows = [];

    public ImportViewModel(IVocabularyRepository repository, IWorkbookFiles files, IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        // Chọn file chỉ dựng bản xem trước, chưa thêm thẻ vào bộ từ.
        ChooseCommand = CreateCommand(async () =>
        {
            workbook = null;
            prepared = null;
            Rows = [];
            SheetName = "";
            languageErrorKey = "";
            OnPropertyChanged(nameof(HasPreview));
            OnPropertyChanged(nameof(LanguageError));
            OnPropertyChanged(nameof(PreviewLanguages));
            var preview = await files.PickAsync();
            if (preview is null) return;
            data = await repository.ReadAsync();
            workbook = preview;
            SheetName = preview.SheetName;
            FirstLanguage = VocabularyLanguages.DisplayName(preview.FirstLanguage, Localization.LanguageCode);
            SecondLanguage = VocabularyLanguages.DisplayName(preview.SecondLanguage, Localization.LanguageCode);
            RebuildPreview();
            OnPropertyChanged(nameof(HasPreview));
        });
        // Chỉ ghi sau khi hết lỗi và người dùng xác nhận; dòng trùng được bỏ qua.
        ImportCommand = CreateCommand(async () =>
        {
            if (!CanImport || prepared is null) throw new StudyException("VFixImport");
            if (!await Interaction.ConfirmAsync(Localization["VImportConfirm"], $"{PreviewLanguages}\n{Summary}", "VImportAction")) return;
            var cards = Rows.Where(row => row.IsValid && !row.IsDuplicate)
                .Select(row => new VocabularyCard(Guid.NewGuid(), deckId, row.Vietnamese, row.English)).ToList();
            var count = await repository.ImportAsync(deckId, cards, prepared.FirstLanguage, prepared.SecondLanguage);
            await Interaction.AlertAsync(Localization["VImportDone"], Localization.Format("VImportedCount", count));
            await Interaction.NavigateAsync("..");
        });
        TemplateCommand = CreateCommand(async () =>
        {
            var deck = (await repository.ReadAsync()).Decks.FirstOrDefault(item => item.Id == deckId) ?? throw new StudyException("VNotFound");
            List<VocabularyCard> cards = deck.FirstLanguage == "vi" && deck.SecondLanguage == "en" ? [
                new(Guid.NewGuid(), Guid.Empty, "quả táo", "apple"), new(Guid.NewGuid(), Guid.Empty, "con mèo", "cat"),
                new(Guid.NewGuid(), Guid.Empty, "quyển sách", "book"), new(Guid.NewGuid(), Guid.Empty, "ngôi nhà", "house")] : [];
            await files.ShareAsync(cards, deck.FirstLanguage, deck.SecondLanguage);
        });
    }

    public string SheetName { get => sheetName; private set => SetProperty(ref sheetName, value); }
    public bool HasPreview => workbook is not null;
    public string FirstLanguage
    {
        get => firstLanguage;
        set { if (SetProperty(ref firstLanguage, value ?? "")) RebuildPreview(); }
    }
    public string SecondLanguage
    {
        get => secondLanguage;
        set { if (SetProperty(ref secondLanguage, value ?? "")) RebuildPreview(); }
    }
    public string LanguageError => languageErrorKey.Length == 0 ? "" : Localization[languageErrorKey];
    public string PreviewLanguages => prepared is null ? "" :
        $"{VocabularyLanguages.DisplayName(prepared.FirstLanguage, Localization.LanguageCode)} / {VocabularyLanguages.DisplayName(prepared.SecondLanguage, Localization.LanguageCode)}";
    public IReadOnlyList<ImportRow> Rows
    {
        get => rows;
        private set { SetProperty(ref rows, value); OnPropertyChanged(nameof(CanImport)); OnPropertyChanged(nameof(Summary)); }
    }
    public string Summary => Localization.Format("VPreviewSummary", Rows.Count(row => row.IsValid && !row.IsDuplicate), Rows.Count(row => row.IsDuplicate), Rows.Count(row => !row.IsValid));
    public bool CanImport => prepared is not null && languageErrorKey.Length == 0 && Rows.Any(row => row.IsValid && !row.IsDuplicate) && Rows.All(row => row.IsValid);
    public ICommand ChooseCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand TemplateCommand { get; }
    public ICommand MenuCommand => CreateMenuCommand(() => Localization["VImportHeading"],
        new("VChooseFile", ChooseCommand), new("VExcelTemplate", TemplateCommand),
        new("VImportAction", ImportCommand, () => CanImport));
    public void SetId(string? value) => deckId = Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty;
    public Task RefreshAsync() => RunAsync(async () =>
    {
        data = await repository.ReadAsync();
        if (!data.Decks.Any(deck => deck.Id == deckId)) throw new StudyException("VNotFound");
        RebuildPreview();
    });

    // Đổi nhãn ngôn ngữ cũng phải kiểm tra lại thứ tự hai mặt và các cặp trùng.
    private void RebuildPreview()
    {
        if (workbook is null || data is null) return;
        prepared = null;
        languageErrorKey = "";
        try
        {
            prepared = VocabularyImport.Prepare(data, deckId, workbook with { FirstLanguage = FirstLanguage, SecondLanguage = SecondLanguage });
        }
        catch (StudyException exception)
        {
            languageErrorKey = exception.ResourceKey;
        }
        var pairs = data.Cards.Where(card => card.DeckId == deckId).Select(card => VocabularyRules.PairKey(card.Vietnamese, card.English)).ToHashSet();
        Rows = (prepared ?? workbook).Rows.Select(row => new ImportRow(row,
            prepared is not null && row.ErrorKey is null && !pairs.Add(VocabularyRules.PairKey(row.Vietnamese, row.English)), Localization)).ToList();
        OnPropertyChanged(nameof(LanguageError));
        OnPropertyChanged(nameof(PreviewLanguages));
    }
}
