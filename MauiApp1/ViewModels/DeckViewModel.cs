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
    private string search = "";
    private int modeIndex;
    private int directionIndex;
    private bool translating;
    private int filterIndex;
    private bool canResume;
    private List<VocabularyCard> allCards = [];
    private IReadOnlyList<VocabularyCardRow> cards = [];

    public DeckViewModel(IVocabularyRepository repository, LearningEngine engine,
        IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        this.engine = engine;
        SaveCommand = CreateCommand(async () => { RequireDeck(); await repository.SaveDeckAsync(deck! with { Name = Name, Description = Description }); await LoadAsync(); });
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
    public string Search { get => search; set { if (SetProperty(ref search, value ?? "")) Filter(); } }
    public int ModeIndex { get => modeIndex; set { if (!translating && value is >= 0 and <= 3) SetProperty(ref modeIndex, value); } }
    public int DirectionIndex { get => directionIndex; set { if (!translating && value is >= 0 and <= 1) SetProperty(ref directionIndex, value); } }
    public int FilterIndex { get => filterIndex; set { if (!translating && value is >= 0 and <= 2) SetProperty(ref filterIndex, value); } }
    public bool CanResume { get => canResume; private set => SetProperty(ref canResume, value); }
    public string MasterySummary => Localization.Format("VMasterySummary", allCards.Count(card => card.IsStarred), allCards.Count);
    public IReadOnlyList<string> Filters => Enumerable.Range(0, 3).Select(index => Localization["VFilter" + index]).ToList();
    public IReadOnlyList<string> Modes => Enumerable.Range(0, 4).Select(index => Localization["VMode" + index]).ToList();
    public IReadOnlyList<string> Directions => [Localization["VDirection0"], Localization["VDirection1"]];
    public IReadOnlyList<VocabularyCardRow> Cards { get => cards; private set => SetProperty(ref cards, value); }
    public ICommand SaveCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }
    public void SetId(string? value) { deckId = Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty; deck = null; }
    public Task RefreshAsync() => RunAsync(LoadAsync);

    protected override void OnLanguageChanged(object? sender, EventArgs arguments)
    {
        translating = true;
        try { base.OnLanguageChanged(sender, arguments); OnPropertyChanged(nameof(ModeIndex)); OnPropertyChanged(nameof(DirectionIndex)); OnPropertyChanged(nameof(FilterIndex)); Filter(); }
        finally { translating = false; }
    }

    private void RequireDeck() { if (deck is null) throw new StudyException("VNotFound"); }

    private async Task LoadAsync()
    {
        var data = await repository.ReadAsync();
        deck = data.Decks.FirstOrDefault(item => item.Id == deckId) ?? throw new StudyException("VNotFound");
        Name = deck.Name;
        Description = deck.Description;
        allCards = data.Cards.Where(card => card.DeckId == deckId).ToList();
        CanResume = data.Session is { IsComplete: false } active && active.DeckId == deckId;
        OnPropertyChanged(nameof(MasterySummary));
        Filter();
    }

    private void Filter()
    {
        var query = VocabularyRules.Normalize(Search);
        Cards = allCards.Where(card => VocabularyRules.Normalize(card.Vietnamese).Contains(query) || VocabularyRules.Normalize(card.English).Contains(query))
            .Select(card => new VocabularyCardRow(card.Vietnamese, card.English,
                CreateCommand(() => Interaction.NavigateAsync($"card?deckId={deckId}&cardId={card.Id}")), card.IsStarred,
                CreateCommand(async () => { await repository.SetStarredAsync(card.Id, !card.IsStarred); await LoadAsync(); }),
                Localization[card.IsStarred ? "VUnstar" : "VStar"])).ToList();
    }

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
