using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public sealed class HomeViewModel : ViewModelBase, IRefreshable
{
    private readonly IVocabularyRepository repository;
    private LearningSession? session;
    private string resumeName = "";
    private string search = "";
    private IReadOnlyList<NamedRow> allClasses = [];

    public HomeViewModel(IVocabularyRepository repository, IUserInteraction interaction, LocalizationService localization)
        : base(interaction, localization)
    {
        this.repository = repository;
        LibraryCommand = CreateCommand(() => Interaction.NavigateAsync("//library"));
        CreateClassCommand = CreateCommand(() => Interaction.NavigateAsync("//library?create=class"));
        SearchCommand = new Command(ApplySearch);
        ResumeCommand = CreateCommand(() => Interaction.NavigateAsync("learn"));
    }

    public ObservableCollection<NamedRow> RecentDecks { get; } = [];
    public ObservableCollection<NamedRow> Classes { get; } = [];
    public bool HasSearch => !string.IsNullOrWhiteSpace(Search);
    public bool HasRecent => !HasSearch && RecentDecks.Count > 0;
    public bool HasClasses => Classes.Count > 0;
    public bool IsEmpty => !HasClasses;
    public bool CanResume => !HasSearch && session is not null;
    public string EmptyHeading => Localization[HasSearch ? "VNoClassesFound" : "VHomeEmptyHeading"];
    public string EmptyBody => Localization[HasSearch ? "VSearchClasses" : "VHomeEmptyBody"];
    public string ResumeName => resumeName;
    public string ResumeSummary => session is null ? "" : Localization.Format("VHomeProgress", HomeOverview.CompletedQuestions(session), session.Questions.Count);
    public double ResumeProgress => session is null ? 0 : (double)HomeOverview.CompletedQuestions(session) / session.Questions.Count;
    public string Search
    {
        get => search;
        set
        {
            if (SetProperty(ref search, value ?? "")) ApplySearch();
        }
    }
    public ICommand LibraryCommand { get; }
    public ICommand CreateClassCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ResumeCommand { get; }

    public Task RefreshAsync() => RunAsync(async () =>
    {
        var data = await repository.ReadAsync();
        session = HomeOverview.ResumableSession(data);
        resumeName = data.Decks.FirstOrDefault(deck => deck.Id == session?.DeckId)?.Name ?? "";
        var cardCounts = data.Cards.GroupBy(card => card.DeckId).ToDictionary(group => group.Key, group => group.Count());
        var deckCounts = data.Decks.GroupBy(deck => deck.ClassId).ToDictionary(group => group.Key, group => group.Count());
        CollectionUpdates.Apply(RecentDecks, HomeOverview.RecentDecks(data).Select(deck =>
            new NamedRow(deck.Name, cardCounts.GetValueOrDefault(deck.Id), "VCardCount",
                CreateCommand(() => Interaction.NavigateAsync($"deck?deckId={deck.Id}")), Localization)));
        allClasses = data.Classes.OrderBy(classroom => classroom.Name).Select(classroom =>
            new NamedRow(classroom.Name, deckCounts.GetValueOrDefault(classroom.Id), "VDeckCount",
                CreateCommand(() => Interaction.NavigateAsync($"class?classId={classroom.Id}")), Localization)).ToList();
        ApplySearch();
        OnPropertyChanged(null);
    });

    private void ApplySearch()
    {
        var query = VocabularySearch.Normalize(Search);
        CollectionUpdates.Apply(Classes, HasSearch
            ? allClasses.Where(classroom => VocabularySearch.Normalize(classroom.Name).Contains(query, StringComparison.Ordinal))
            : allClasses.Take(6));
        OnPropertyChanged(nameof(HasSearch));
        OnPropertyChanged(nameof(HasClasses));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasRecent));
        OnPropertyChanged(nameof(CanResume));
        OnPropertyChanged(nameof(EmptyHeading));
        OnPropertyChanged(nameof(EmptyBody));
    }
}
