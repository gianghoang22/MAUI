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
    private IReadOnlyList<ImportRow> rows = [];

    public ImportViewModel(IVocabularyRepository repository, IWorkbookFiles files, IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        ChooseCommand = CreateCommand(async () =>
        {
            Rows = [];
            SheetName = "";
            var preview = await files.PickAsync();
            if (preview is null) return;
            var data = await repository.ReadAsync();
            var pairs = data.Cards.Where(card => card.DeckId == deckId).Select(card => VocabularyRules.PairKey(card.Vietnamese, card.English)).ToHashSet();
            SheetName = preview.SheetName;
            Rows = preview.Rows.Select(row => new ImportRow(row, row.ErrorKey is null && !pairs.Add(VocabularyRules.PairKey(row.Vietnamese, row.English)), Localization)).ToList();
        });
        ImportCommand = CreateCommand(async () =>
        {
            if (!CanImport) throw new StudyException("VFixImport");
            if (!await Interaction.ConfirmAsync(Localization["VImportConfirm"], Summary, "VImportAction")) return;
            var cards = Rows.Where(row => row.IsValid && !row.IsDuplicate).Select(row => new VocabularyCard(Guid.NewGuid(), deckId, row.Vietnamese, row.English)).ToList();
            var count = await repository.ImportAsync(deckId, cards);
            await Interaction.AlertAsync(Localization["VImportDone"], Localization.Format("VImportedCount", count));
            await Interaction.NavigateAsync("..");
        });
        TemplateCommand = CreateCommand(() => files.ShareAsync([
            new(Guid.NewGuid(), Guid.Empty, "quả táo", "apple"), new(Guid.NewGuid(), Guid.Empty, "con mèo", "cat"),
            new(Guid.NewGuid(), Guid.Empty, "quyển sách", "book"), new(Guid.NewGuid(), Guid.Empty, "ngôi nhà", "house")]));
    }

    public string SheetName { get => sheetName; private set => SetProperty(ref sheetName, value); }
    public IReadOnlyList<ImportRow> Rows
    {
        get => rows;
        private set { SetProperty(ref rows, value); OnPropertyChanged(nameof(CanImport)); OnPropertyChanged(nameof(Summary)); }
    }
    public string Summary => Localization.Format("VPreviewSummary", Rows.Count(row => row.IsValid && !row.IsDuplicate), Rows.Count(row => row.IsDuplicate), Rows.Count(row => !row.IsValid));
    public bool CanImport => Rows.Any(row => row.IsValid && !row.IsDuplicate) && Rows.All(row => row.IsValid);
    public ICommand ChooseCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand TemplateCommand { get; }
    public ICommand MenuCommand => CreateMenuCommand(() => Localization["VImportHeading"],
        new("VChooseFile", ChooseCommand), new("VExcelTemplate", TemplateCommand),
        new("VImportAction", ImportCommand, () => CanImport));
    public void SetId(string? value) => deckId = Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty;
    public Task RefreshAsync() => RunAsync(async () => { if (!(await repository.ReadAsync()).Decks.Any(deck => deck.Id == deckId)) throw new StudyException("VNotFound"); });
}
