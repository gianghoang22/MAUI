using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public sealed class CardEditorViewModel : AutosaveViewModel, IRefreshable
{
    private readonly IVocabularyRepository repository;
    private Guid deckId;
    private Guid cardId;
    private bool isNew;
    private bool loaded;
    private bool restored;
    private string deckName = "";
    private string vietnamese = "";
    private string english = "";

    public CardEditorViewModel(IVocabularyRepository repository, IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        SaveCommand = CreateCommand(async () =>
        {
            RequireLoaded();
            await FlushAsync();
            await repository.SaveCardAsync(new VocabularyCard(cardId, deckId, Vietnamese, English));
            AutosaveEnabled = false;
            ResetAutosave();
            await Interaction.NavigateAsync("..");
        });
        DiscardCommand = CreateCommand(async () =>
        {
            RequireLoaded();
            if (!await Interaction.ConfirmAsync(Localization["VDiscardDraft"], Localization["VDiscardDraftWarning"], "VDiscard")) return;
            await FlushAsync();
            await repository.DeleteDraftAsync(cardId);
            AutosaveEnabled = false;
            ResetAutosave();
            await Interaction.NavigateAsync("..");
        });
        DeleteCommand = CreateCommand(async () =>
        {
            RequireLoaded();
            if (isNew || !await Interaction.ConfirmAsync(Localization["VDeleteCard"], Localization["VDeleteCardWarning"])) return;
            await FlushAsync();
            await repository.DeleteCardAsync(cardId);
            AutosaveEnabled = false;
            ResetAutosave();
            await Interaction.NavigateAsync("..");
        });
    }

    public string Vietnamese { get => vietnamese; set { if (SetProperty(ref vietnamese, value ?? "")) MarkDirty(); } }
    public string English { get => english; set { if (SetProperty(ref english, value ?? "")) MarkDirty(); } }
    public string DeckName { get => deckName; private set => SetProperty(ref deckName, value); }
    public bool IsExisting => !isNew;
    public bool Restored { get => restored; private set => SetProperty(ref restored, value); }
    public ICommand SaveCommand { get; }
    public ICommand DiscardCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand MenuCommand => CreateMenuCommand(() => Localization["VActions"],
        new("VSaveCard", SaveCommand), new("VDiscardDraft", DiscardCommand),
        new("VDeleteCard", DeleteCommand, () => IsExisting, IsDestructive: true));

    public void SetIds(string? deck, string? card)
    {
        if (loaded) return;
        deckId = Guid.TryParse(deck, out var parsedDeck) ? parsedDeck : Guid.Empty;
        isNew = card is null;
        cardId = card is null ? Guid.NewGuid() : Guid.TryParse(card, out var parsedCard) ? parsedCard : Guid.Empty;
    }

    public Task RefreshAsync() => loaded ? Task.CompletedTask : RunAsync(LoadAsync);

    private async Task LoadAsync()
    {
        var data = await repository.ReadAsync();
        DeckName = data.Decks.FirstOrDefault(deck => deck.Id == deckId)?.Name ?? throw new StudyException("VNotFound");
        var draft = data.Drafts.FirstOrDefault(item => item.DeckId == deckId && (isNew ? item.IsNew : item.CardId == cardId));
        var card = data.Cards.FirstOrDefault(item => item.DeckId == deckId && item.Id == cardId);
        if (!isNew && card is null) throw new StudyException("VNotFound");
        AutosaveEnabled = false;
        if (draft is not null)
        {
            cardId = draft.CardId;
            Vietnamese = draft.Vietnamese;
            English = draft.English;
            Restored = true;
        }
        else
        {
            Vietnamese = card?.Vietnamese ?? "";
            English = card?.English ?? "";
        }
        loaded = true;
        AutosaveEnabled = true;
        OnPropertyChanged(nameof(IsExisting));
    }

    protected override Task PersistDraftAsync() => repository.SaveDraftAsync(new(cardId, deckId, isNew, Vietnamese, English, DateTimeOffset.UtcNow));
    private void RequireLoaded() { if (!loaded) throw new StudyException("VNotFound"); }
}
