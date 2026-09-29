using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public sealed class DeckViewModel : ViewModelBase, IRefreshable
{
    private readonly IVocabularyRepository repository;
    private readonly LearningEngine engine;
    private Guid deckId;
    private VocabularyDeck? deck;
    private string name = "";
    private string description = "";
    private string firstLanguage = "vi";
    private string secondLanguage = "en";
    private string search = "";
    private int modeIndex;
    private int directionIndex;
    private bool translating;
    private int filterIndex;
    // Bộ lọc danh sách chỉ đổi các thẻ đang hiện, không đổi nhóm từ được chọn để học.
    private int listFilterIndex = (int)LearningFilter.All;
    private bool canResume;
    private bool showDetails;
    private List<VocabularyCard> allCards = [];
    private IReadOnlyDictionary<Guid, VocabularyCardRow> cardRows = new Dictionary<Guid, VocabularyCardRow>();

    public DeckViewModel(IVocabularyRepository repository, LearningEngine engine,
        IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        this.engine = engine;
        SaveCommand = CreateCommand(async () => { RequireDeck(); await repository.SaveDeckAsync(deck! with { Name = Name, Description = Description, FirstLanguage = FirstLanguage, SecondLanguage = SecondLanguage }); await LoadAsync(); });
        AddCommand = CreateCommand(async () => { RequireDeck(); await Interaction.NavigateAsync($"card?deckId={deckId}"); });
        ImportCommand = CreateCommand(async () => { RequireDeck(); await Interaction.NavigateAsync($"import?deckId={deckId}"); });
        ResumeCommand = CreateCommand(async () =>
        {
            var current = (await repository.ReadAsync()).Session;
            if (current is not { IsComplete: false } || current.DeckId != deckId) throw new StudyException("VNoSession");
            await Interaction.NavigateAsync("learn");
        });
        StartCommand = CreateCommand(StartAsync);
        RefreshCommand = CreateCommand(LoadAsync);
        ToggleDetailsCommand = new Command(() => ShowDetails = !ShowDetails);
        ResetSearchCommand = new Command(() =>
        {
            Search = "";
            ListFilterIndex = (int)LearningFilter.All;
        });
        ResetStudyFilterCommand = new Command(() => FilterIndex = (int)LearningFilter.All);
        DeleteCommand = CreateCommand(async () =>
        {
            RequireDeck();
            if (!await Interaction.ConfirmAsync(Localization["VDeleteDeck"], Localization["VDeleteDeckWarning"])) return;
            await repository.DeleteDeckAsync(deckId);
            await Interaction.NavigateAsync("..");
        });
    }

    public string Name { get => name; set => SetProperty(ref name, value ?? ""); }
    public string Description { get => description; set => SetProperty(ref description, value ?? ""); }
    public string LanguageSummary => deck is null ? "" : $"{VocabularyLanguages.DisplayName(deck.FirstLanguage, Localization.LanguageCode)} / {VocabularyLanguages.DisplayName(deck.SecondLanguage, Localization.LanguageCode)}";
    public string FirstLanguage
    {
        get => firstLanguage;
        set { if (SetProperty(ref firstLanguage, value ?? "")) OnPropertyChanged(nameof(Directions)); }
    }
    public string SecondLanguage
    {
        get => secondLanguage;
        set { if (SetProperty(ref secondLanguage, value ?? "")) OnPropertyChanged(nameof(Directions)); }
    }
    public string Search { get => search; set { if (SetProperty(ref search, value ?? "")) Filter(); } }
    public int ModeIndex { get => modeIndex; set { if (!translating && value is >= 0 and <= 3) SetProperty(ref modeIndex, value); } }
    public int DirectionIndex { get => directionIndex; set { if (!translating && value is >= 0 and <= 1) SetProperty(ref directionIndex, value); } }
    public int FilterIndex
    {
        get => filterIndex;
        set { if (!translating && value is >= 0 and <= 2 && SetProperty(ref filterIndex, value)) NotifyStudyFilter(); }
    }
    public int ListFilterIndex
    {
        get => listFilterIndex;
        set { if (!translating && value is >= 0 and <= 2 && SetProperty(ref listFilterIndex, value)) Filter(); }
    }
    public bool HasSearchFilters => !string.IsNullOrWhiteSpace(Search) || ListFilterIndex != (int)LearningFilter.All;
    public bool IsSearchEmpty => Cards.Count == 0;
    public string SearchSummary => Localization.Format("VSearchResultCount", Cards.Count, allCards.Count);
    public string EmptyMessage => Localization[allCards.Count == 0 ? "VCardsEmpty" : "VNoCardsFound"];
    public int EligibleCardCount => allCards.Count(card => VocabularySearch.MatchesFilter(card, FilterIndex));
    public bool HasEligibleCards => EligibleCardCount > 0;
    public string StudyFilterSummary => EligibleCardCount == 0 ? Localization["VNoStudyMatches"] : Localization.Format("VStudyEligibleCount", EligibleCardCount);
    public ICommand ResetSearchCommand { get; }
    public ICommand ResetStudyFilterCommand { get; }
    public bool CanResume { get => canResume; private set => SetProperty(ref canResume, value); }
    public bool ShowDetails
    {
        get => showDetails;
        private set
        {
            if (SetProperty(ref showDetails, value)) OnPropertyChanged(nameof(DetailsActionText));
        }
    }
    public string DetailsActionText => Localization[ShowDetails ? "VHideSetDetails" : "VSetDetails"];
    public string MasterySummary => Localization.Format("VMasterySummary", allCards.Count(card => card.IsStarred), allCards.Count);
    public IReadOnlyList<string> Filters => Enumerable.Range(0, 3).Select(index => Localization["VFilter" + index]).ToList();
    public IReadOnlyList<string> Modes => Enumerable.Range(0, 4).Select(index => Localization["VMode" + index]).ToList();
    public IReadOnlyList<string> Directions => [
        VocabularyLanguages.Direction(FirstLanguage, SecondLanguage, LearningDirection.EnglishToVietnamese, Localization.LanguageCode),
        VocabularyLanguages.Direction(FirstLanguage, SecondLanguage, LearningDirection.VietnameseToEnglish, Localization.LanguageCode)];
    public ObservableCollection<VocabularyCardRow> Cards { get; } = [];
    public ICommand SaveCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleDetailsCommand { get; }
    public void SetId(string? value) { deckId = Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty; deck = null; }
    public Task RefreshAsync() => RunAsync(LoadAsync);

    protected override void OnLanguageChanged(object? sender, EventArgs arguments)
    {
        translating = true;
        try { base.OnLanguageChanged(sender, arguments); OnPropertyChanged(nameof(ModeIndex)); OnPropertyChanged(nameof(DirectionIndex)); OnPropertyChanged(nameof(FilterIndex)); OnPropertyChanged(nameof(ListFilterIndex)); Filter(); }
        finally { translating = false; }
    }

    private void RequireDeck() { if (deck is null) throw new StudyException("VNotFound"); }

    private async Task LoadAsync()
    {
        var data = await repository.ReadAsync();
        deck = data.Decks.FirstOrDefault(item => item.Id == deckId) ?? throw new StudyException("VNotFound");
        Name = deck.Name;
        Description = deck.Description;
        FirstLanguage = VocabularyLanguages.DisplayName(deck.FirstLanguage, Localization.LanguageCode);
        SecondLanguage = VocabularyLanguages.DisplayName(deck.SecondLanguage, Localization.LanguageCode);
        OnPropertyChanged(nameof(LanguageSummary));
        allCards = data.Cards.Where(card => card.DeckId == deckId).ToList();
        RebuildCardRows();
        CanResume = data.Session is { IsComplete: false } active && active.DeckId == deckId;
        OnPropertyChanged(nameof(MasterySummary));
        NotifyStudyFilter();
        Filter();
    }

    private void RebuildCardRows()
    {
        cardRows = allCards.ToDictionary(card => card.Id, card => new VocabularyCardRow(card,
                CreateCommand(() => Interaction.NavigateAsync($"card?deckId={deckId}&cardId={card.Id}")),
                CreateCommand(() => ToggleStarAsync(card.Id)),
                CreateMenuCommand(() => card.English,
                    new("VEdit", CreateCommand(() => Interaction.NavigateAsync($"card?deckId={deckId}&cardId={card.Id}"))),
                    new("VDeleteCard", CreateCommand(async () =>
                    {
                        if (!await Interaction.ConfirmAsync(Localization["VDeleteCard"], Localization["VDeleteCardWarning"])) return;
                        await repository.DeleteCardAsync(card.Id);
                        await LoadAsync();
                    }), IsDestructive: true)), Localization));
    }

    private void Filter()
    {
        var query = VocabularySearch.Normalize(Search);
        CollectionUpdates.Apply(Cards, allCards.Where(card => VocabularySearch.MatchesFilter(card, ListFilterIndex) &&
            (VocabularySearch.Normalize(card.Vietnamese).Contains(query, StringComparison.Ordinal) || VocabularySearch.Normalize(card.English).Contains(query, StringComparison.Ordinal)))
            .Select(card => cardRows[card.Id]));
        OnPropertyChanged(nameof(HasSearchFilters));
        OnPropertyChanged(nameof(SearchSummary));
        OnPropertyChanged(nameof(EmptyMessage));
        OnPropertyChanged(nameof(IsSearchEmpty));
    }

    private async Task ToggleStarAsync(Guid cardId)
    {
        var index = allCards.FindIndex(card => card.Id == cardId);
        if (index < 0) return;
        var updated = allCards[index] with { IsStarred = !allCards[index].IsStarred };
        await repository.SetStarredAsync(cardId, updated.IsStarred);
        allCards[index] = updated;
        cardRows[cardId].SetStarred(updated.IsStarred);
        OnPropertyChanged(nameof(MasterySummary));
        NotifyStudyFilter();
        if (ListFilterIndex != (int)LearningFilter.All) Filter();
    }

    private void NotifyStudyFilter()
    {
        OnPropertyChanged(nameof(EligibleCardCount));
        OnPropertyChanged(nameof(HasEligibleCards));
        OnPropertyChanged(nameof(StudyFilterSummary));
    }

    // Tạo câu hỏi từ dữ liệu đã lưu -> xác nhận thay phiên cũ -> lưu phiên mới -> mở màn học.
    private async Task StartAsync()
    {
        RequireDeck();
        var data = await repository.ReadAsync();
        var currentDeck = data.Decks.FirstOrDefault(item => item.Id == deckId) ?? throw new StudyException("VNotFound");
        var session = engine.Create(currentDeck, data.Cards.Where(card => card.DeckId == deckId).ToList(), (LearningMode)ModeIndex, (LearningDirection)DirectionIndex, (LearningFilter)FilterIndex);
        if (data.Session is { IsComplete: false } && !await Interaction.ConfirmAsync(Localization["VReplaceSession"], Localization["VReplaceSessionWarning"], "VStartNew")) return;
        await repository.StartSessionAsync(session);
        await Interaction.NavigateAsync("learn");
    }
}
