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
    private IReadOnlyList<NamedRow> decks = [];

    public ClassViewModel(IVocabularyRepository repository, IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        SaveCommand = CreateCommand(async () => { RequireLoaded(); await repository.SaveClassAsync(new(classId, Name)); await LoadAsync(); });
        AddCommand = CreateCommand(async () =>
        {
            RequireLoaded();
            await repository.SaveDeckAsync(new(Guid.NewGuid(), classId, NewDeckName, ""));
            NewDeckName = "";
            await LoadAsync();
        });
        DeleteCommand = CreateCommand(async () =>
        {
            RequireLoaded();
            if (!await Interaction.ConfirmAsync(Localization["VDeleteClass"], Localization["VDeleteClassWarning"])) return;
            await repository.DeleteClassAsync(classId);
            await Interaction.NavigateAsync("..");
        });
        RefreshCommand = CreateCommand(LoadAsync);
    }

    public string Name { get => name; set => SetProperty(ref name, value ?? ""); }
    public string NewDeckName { get => newDeckName; set => SetProperty(ref newDeckName, value ?? ""); }
    public IReadOnlyList<NamedRow> Decks { get => decks; private set => SetProperty(ref decks, value); }
    public ICommand SaveCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RefreshCommand { get; }
    public void SetId(string? value) { classId = Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty; loaded = false; }
    public Task RefreshAsync() => RunAsync(LoadAsync);
    private void RequireLoaded() { if (!loaded) throw new StudyException("VNotFound"); }

    private async Task LoadAsync()
    {
        loaded = false;
        var data = await repository.ReadAsync();
        Name = data.Classes.FirstOrDefault(classroom => classroom.Id == classId)?.Name ?? throw new StudyException("VNotFound");
        Decks = data.Decks.Where(deck => deck.ClassId == classId).OrderBy(deck => deck.Name).Select(deck => new NamedRow(deck.Name,
            data.Cards.Count(card => card.DeckId == deck.Id), "VCardCount",
            CreateCommand(() => Interaction.NavigateAsync($"deck?deckId={deck.Id}")), Localization)).ToList();
        loaded = true;
    }
}
