using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public sealed class ClassViewModel : ViewModelBase, IRefreshable
{
    private readonly IVocabularyRepository repository;
    private Guid classId;
    private bool loaded;
    private string name = "";
    private string newDeckName = "";
    private string search = "";
    private IReadOnlyList<NamedRow> allDecks = [];

    public ClassViewModel(IVocabularyRepository repository, IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        SaveCommand = CreateCommand(async () => { RequireLoaded(); await repository.SaveClassAsync(new(classId, Name)); await LoadAsync(); });
        AddCommand = new Command(async () =>
        {
            RequireLoaded();
            if (string.IsNullOrWhiteSpace(NewDeckName))
                return;
            await RunAsync(async () =>
            {
                await repository.SaveDeckAsync(new(Guid.NewGuid(), classId, NewDeckName.Trim(), ""));
                NewDeckName = "";
                Search = "";
                await LoadAsync();
            });
        }, () => CanAddDeck);
        DeleteCommand = CreateCommand(async () =>
        {
            RequireLoaded();
            if (!await Interaction.ConfirmAsync(Localization["VDeleteClass"], Localization["VDeleteClassWarning"])) return;
            await repository.DeleteClassAsync(classId);
            await Interaction.NavigateAsync("..");
        });
        RefreshCommand = CreateCommand(LoadAsync);
        ClearSearchCommand = new Command(() => Search = "");
        MenuCommand = CreateMenuCommand(() => Name,
            new("VSave", SaveCommand), new("VDeleteClass", DeleteCommand, IsDestructive: true));
    }

    public string Name { get => name; set => SetProperty(ref name, value ?? ""); }
    public string NewDeckName
    {
        get => newDeckName;
        set
        {
            if (SetProperty(ref newDeckName, value ?? ""))
            {
                OnPropertyChanged(nameof(CanAddDeck));
                ((Command)AddCommand).ChangeCanExecute();
            }
        }
    }
    public bool CanAddDeck => !string.IsNullOrWhiteSpace(NewDeckName);
    public string Search
    {
        get => search;
        set
        {
            if (SetProperty(ref search, value ?? "")) FilterDecks();
        }
    }
    public string EmptyMessage => Localization[string.IsNullOrWhiteSpace(Search) ? "VDecksEmpty" : "VNoDecksFound"];
    public bool HasSearch => !string.IsNullOrWhiteSpace(Search);
    public bool IsSearchEmpty => Decks.Count == 0;
    public string SearchSummary => Localization.Format("VSearchResultCount", Decks.Count, allDecks.Count);
    public ICommand ClearSearchCommand { get; }
    public ObservableCollection<NamedRow> Decks { get; } = [];
    public ICommand SaveCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand MenuCommand { get; }
    public void SetId(string? value) { classId = Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty; loaded = false; }
    public Task RefreshAsync() => RunAsync(LoadAsync);
    private void RequireLoaded() { if (!loaded) throw new StudyException("VNotFound"); }

    // Chỉ lấy bộ từ thuộc lớp đang mở; bấm một bộ sẽ truyền deckId sang màn bộ từ.
    private async Task LoadAsync()
    {
        loaded = false;
        var data = await repository.ReadAsync();
        Name = data.Classes.FirstOrDefault(classroom => classroom.Id == classId)?.Name ?? throw new StudyException("VNotFound");
        allDecks = data.Decks.Where(deck => deck.ClassId == classId).OrderBy(deck => deck.Name).Select(deck => new NamedRow(deck.Name,
            data.Cards.Count(card => card.DeckId == deck.Id), "VCardCount",
            CreateCommand(() => Interaction.NavigateAsync($"deck?deckId={deck.Id}")), Localization,
            CreateMenuCommand(() => deck.Name,
                new("VEdit", CreateCommand(async () =>
                {
                    var updatedName = await Interaction.PromptAsync(Localization["VEdit"], Localization["VDeckName"], deck.Name);
                    if (string.IsNullOrWhiteSpace(updatedName)) return;
                    await repository.SaveDeckAsync(deck with { Name = updatedName.Trim() });
                    await LoadAsync();
                })),
                new("VDeleteDeck", CreateCommand(async () =>
                {
                    if (!await Interaction.ConfirmAsync(Localization["VDeleteDeck"], Localization["VDeleteDeckWarning"])) return;
                    await repository.DeleteDeckAsync(deck.Id);
                    await LoadAsync();
                }), IsDestructive: true)))).ToList();
        FilterDecks();
        loaded = true;
    }

    private void FilterDecks()
    {
        var query = VocabularySearch.Normalize(Search);
        CollectionUpdates.Apply(Decks, allDecks.Where(deck => VocabularySearch.Normalize(deck.Name).Contains(query, StringComparison.Ordinal)));
        OnPropertyChanged(nameof(EmptyMessage));
        OnPropertyChanged(nameof(HasSearch));
        OnPropertyChanged(nameof(SearchSummary));
        OnPropertyChanged(nameof(IsSearchEmpty));
    }
}
