using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public sealed class LibraryViewModel : ViewModelBase, IRefreshable
{
    private readonly IVocabularyRepository repository;
    private string newName = "";
    private bool canResume;
    private int deckCount;
    private int cardCount;
    private IReadOnlyList<NamedRow> allClasses = [];
    private string search = "";
    private bool showCreate;

    public LibraryViewModel(IVocabularyRepository repository, IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        AddCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(NewName))
                return;
            await RunAsync(async () =>
            {
                await repository.SaveClassAsync(new VocabularyClass(Guid.NewGuid(), NewName.Trim()));
                NewName = "";
                Search = "";
                ShowCreate = false;
                await LoadAsync();
            });
        }, () => CanAdd);
        RefreshCommand = CreateCommand(LoadAsync);
        ResumeCommand = CreateCommand(() => Interaction.NavigateAsync("learn"));
        ToggleCreateCommand = new Command(() => ShowCreate = !ShowCreate);
        ClearSearchCommand = new Command(() => Search = "");
    }

    public string NewName
    {
        get => newName;
        set
        {
            if (SetProperty(ref newName, value ?? ""))
            {
                OnPropertyChanged(nameof(CanAdd));
                ((Command)AddCommand).ChangeCanExecute();
            }
        }
    }
    public bool CanAdd => !string.IsNullOrWhiteSpace(NewName);
    public ObservableCollection<NamedRow> Classes { get; } = [];
    public bool CanResume { get => canResume; private set => SetProperty(ref canResume, value); }
    public bool ShowCreate
    {
        get => showCreate;
        set
        {
            if (SetProperty(ref showCreate, value)) OnPropertyChanged(nameof(CreateActionText));
        }
    }
    public string CreateActionText => Localization[ShowCreate ? "Cancel" : "VAddClass"];
    public int ClassCount => allClasses.Count;
    public int DeckCount => deckCount;
    public int CardCount => cardCount;
    public string EmptyMessage => Localization[string.IsNullOrWhiteSpace(Search) ? "VClassesEmpty" : "VNoClassesFound"];
    public bool HasSearch => !string.IsNullOrWhiteSpace(Search);
    public bool IsSearchEmpty => Classes.Count == 0;
    public string SearchSummary => Localization.Format("VSearchResultCount", Classes.Count, allClasses.Count);
    public ICommand ClearSearchCommand { get; }
    public string Search
    {
        get => search;
        set
        {
            if (SetProperty(ref search, value ?? "")) ApplySearch();
        }
    }
    public string Summary => Localization.Format("VLibrarySummary", deckCount, cardCount);
    public ICommand AddCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand ToggleCreateCommand { get; }
    public Task RefreshAsync() => RunAsync(LoadAsync);

    private async Task LoadAsync()
    {
        var data = await repository.ReadAsync();
        deckCount = data.Decks.Count;
        cardCount = data.Cards.Count;
        OnPropertyChanged(nameof(Summary));
        CanResume = data.Session is { IsComplete: false };
        allClasses = data.Classes.OrderBy(classroom => classroom.Name).Select(classroom =>
        {
            var menuCommand = CreateCommand(async () =>
            {
                var action = await Interaction.ActionSheetAsync(classroom.Name, Localization["Cancel"], Localization["Delete"], Localization["VEdit"]);
                if (action == Localization["VEdit"])
                {
                    var updatedName = await Interaction.PromptAsync(Localization["VEdit"], Localization["VClassName"], classroom.Name);
                    if (!string.IsNullOrWhiteSpace(updatedName) && updatedName.Trim() != classroom.Name)
                    {
                        await repository.SaveClassAsync(classroom with { Name = updatedName.Trim() });
                        await LoadAsync();
                    }
                }
                else if (action == Localization["Delete"])
                {
                    if (await Interaction.ConfirmAsync(Localization["VDeleteClass"], Localization["VDeleteClassWarning"]))
                    {
                        await repository.DeleteClassAsync(classroom.Id);
                        await LoadAsync();
                    }
                }
            });

            return new NamedRow(classroom.Name,
                data.Decks.Count(deck => deck.ClassId == classroom.Id), "VDeckCount",
                CreateCommand(() => Interaction.NavigateAsync($"class?classId={classroom.Id}")), Localization, menuCommand);
        }).ToList();
        OnPropertyChanged(nameof(ClassCount));
        OnPropertyChanged(nameof(DeckCount));
        OnPropertyChanged(nameof(CardCount));
        ApplySearch();
    }

    private void ApplySearch()
    {
        var query = VocabularySearch.Normalize(Search);
        CollectionUpdates.Apply(Classes, allClasses.Where(classroom => VocabularySearch.Normalize(classroom.Name).Contains(query, StringComparison.Ordinal)));
        OnPropertyChanged(nameof(EmptyMessage));
        OnPropertyChanged(nameof(HasSearch));
        OnPropertyChanged(nameof(SearchSummary));
        OnPropertyChanged(nameof(IsSearchEmpty));
    }
}
