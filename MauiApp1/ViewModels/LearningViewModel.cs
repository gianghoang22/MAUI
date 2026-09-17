using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public sealed class LearningViewModel : AutosaveViewModel, IRefreshable
{
    private readonly IVocabularyRepository repository;
    private readonly LearningEngine engine;
    private LearningSession? session;
    private string input = "";
    private Guid? selectedLeft;
    private HashSet<Guid> starredIds = [];
    private List<VocabularyCard> libraryCards = [];
    private int modeIndex;
    private int directionIndex;
    private int filterIndex;
    private bool showSetup;
    private bool translating;
    private string matchFeedbackKey = "VMatchHint";
    protected override string SavedHintKey => "VSessionCheckpoint";

    public LearningViewModel(IVocabularyRepository repository, LearningEngine engine, IPronunciation pronunciation,
        IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        this.engine = engine;
        FlipCommand = CreateCommand(() => ChangeAsync(current => current.Mode == LearningMode.Flashcards && !current.IsComplete
            ? current with { Revealed = !current.Revealed } : throw new StudyException("VInvalidSession")));
        RememberCommand = CreateCommand(() => ChangeAsync(current => engine.Next(engine.Rate(current, true))));
        ForgetCommand = CreateCommand(() => ChangeAsync(current => engine.Next(engine.Rate(current, false))));
        SubmitCommand = CreateCommand(() => ChangeAsync(current => engine.Submit(current, Input)));
        NextCommand = CreateCommand(() => ChangeAsync(engine.Next));
        RefreshCommand = CreateCommand(LoadAsync);
        SetupCommand = CreateCommand(() => { ShowSetup = !ShowSetup; return Task.CompletedTask; });
        ApplySetupCommand = CreateCommand(() => StartBatchAsync(true));
        ContinueCommand = CreateCommand(() => StartBatchAsync(false));
        StarCommand = CreateCommand(async () =>
        {
            if (CurrentQuestion is null) return;
            var cardId = CurrentQuestion.CardId;
            await repository.SetStarredAsync(cardId, !starredIds.Contains(cardId));
            await ReloadStarsAsync();
            OnPropertyChanged(nameof(StarText));
            OnPropertyChanged(nameof(StarDescription));
        });
        ExitCommand = CreateCommand(async () => { await FlushAsync(); await Interaction.NavigateAsync(".."); });
        SpeakCommand = CreateCommand(async () =>
        {
            if (session is null || CurrentQuestion is null) return;
            await pronunciation.SpeakAsync(session.Direction == LearningDirection.EnglishToVietnamese ? CurrentQuestion.Prompt : CurrentQuestion.Answer);
        });
        RetryCommand = CreateCommand(async () =>
        {
            var result = (await repository.ReadAsync()).Results.FirstOrDefault(item => item.Id == session?.Id) ?? throw new StudyException("VNotFound");
            var next = engine.Retry(result);
            await repository.StartSessionAsync(next);
            ResetAutosave();
            UpdateSetupSelection(next);
            ShowSetup = false;
            selectedLeft = null;
            Apply(next);
        });
    }

    private StudyQuestion? CurrentQuestion => session is not null && session.Index < session.Questions.Count ? session.Questions[session.Index] : null;
    public string DeckName => session?.DeckName ?? "";
    public string ModeName => session is null ? "" : Localization["VMode" + (int)session.Mode];
    public string DirectionName => session is null ? "" : Localization["VDirection" + (int)session.Direction];
    public string QuestionText => CurrentQuestion?.Prompt ?? "";
    public string AnswerText => CurrentQuestion is null ? "" : string.Join(" / ", CurrentQuestion.AcceptedAnswers);
    public string Input { get => input; set { if (SetProperty(ref input, value ?? "")) MarkDirty(); } }
    public bool ShowQuestion => session is { IsComplete: false } && session.Mode != LearningMode.Match;
    public bool ShowAnswer => ShowQuestion && session!.Revealed;
    public bool IsFlashcard => ShowQuestion && session!.Mode == LearningMode.Flashcards;
    public bool ShowFlip => IsFlashcard && !session!.IsAnswered;
    public double Progress => session is null ? 0 : (double)(session.Mode == LearningMode.Match ? session.MatchedIds.Count : session.Index) / session.Questions.Count;
    public string FrontCaption => Localization[session?.Direction == LearningDirection.VietnameseToEnglish ? "VVietnamese" : "VEnglish"];
    public string BackCaption => Localization[session?.Direction == LearningDirection.VietnameseToEnglish ? "VEnglish" : "VVietnamese"];
    public string StarText => CurrentQuestion is not null && starredIds.Contains(CurrentQuestion.CardId) ? "★" : "☆";
    public string StarDescription => Localization[StarText == "★" ? "VUnstar" : "VStar"];
    public bool ShowSetup { get => showSetup; set => SetProperty(ref showSetup, value); }
    public int ModeIndex { get => modeIndex; set { if (!translating && value is >= 0 and <= 3) SetProperty(ref modeIndex, value); } }
    public int DirectionIndex { get => directionIndex; set { if (!translating && value is >= 0 and <= 1) SetProperty(ref directionIndex, value); } }
    public int FilterIndex { get => filterIndex; set { if (!translating && value is >= 0 and <= 2) SetProperty(ref filterIndex, value); } }
    public IReadOnlyList<string> Modes => Enumerable.Range(0, 4).Select(index => Localization["VMode" + index]).ToList();
    public IReadOnlyList<string> Directions => [Localization["VDirection0"], Localization["VDirection1"]];
    public IReadOnlyList<string> Filters => Enumerable.Range(0, 3).Select(index => Localization["VFilter" + index]).ToList();
    public bool ShowRating => ShowFlip;
    public bool ShowChoices => ShowQuestion && session!.Mode == LearningMode.MultipleChoice && !session.IsAnswered;
    public bool ShowWritten => ShowQuestion && session!.Mode == LearningMode.Written && !session.IsAnswered;
    public bool ShowNext => ShowQuestion && session!.IsAnswered;
    public bool ShowMatch => session is { IsComplete: false, Mode: LearningMode.Match };
    public bool ShowResult => session is { IsComplete: true };
    public bool CanContinue => session is not null && libraryCards.Any(card => card.DeckId == session.DeckId &&
        (session.Filter == LearningFilter.All || card.IsStarred == (session.Filter == LearningFilter.Starred)));
    public bool NoRemainingCards => ShowResult && !CanContinue;
    public bool CanRetry => ShowResult && session!.Attempts.Any(attempt => !attempt.Correct);
    public string Feedback => ShowNext ? Localization[session!.Attempts[session.Index].Correct ? "VCorrect" : "VIncorrect"] : "";
    public string MatchFeedback => Localization[matchFeedbackKey];
    public string Position => session is null ? "" : Localization.Format("VPosition", session.Mode == LearningMode.Match ? session.MatchedIds.Count : Math.Min(session.Index + 1, session.Questions.Count), session.Questions.Count);
    public string Score => session is null ? "" : Localization.Format("VScore", session.Correct, session.Questions.Count);
    public string ResultDetails => session is null ? "" : Localization.Format("VResultTime",
        (int)((session.FinishedAt ?? DateTimeOffset.UtcNow) - session.StartedAt).TotalSeconds, session.MatchMistakes);
    public string WrongAnswers => session is null ? "" : string.Join(Environment.NewLine,
        session.Questions.Where(question => session.Attempts.Any(attempt => attempt.CardId == question.CardId && !attempt.Correct))
            .Select(question => $"{question.Prompt} → {string.Join(" / ", question.AcceptedAnswers)}"));
    public IReadOnlyList<ChoiceRow> Choices => CurrentQuestion?.Choices.Select(choice =>
        new ChoiceRow(choice, CreateCommand(() => ChangeAsync(current => engine.Submit(current, choice))))).ToList() ?? [];
    public IReadOnlyList<MatchRow> MatchLeft => session?.Questions.Select(question => new MatchRow(question.CardId, question.Prompt,
        session.MatchedIds.Contains(question.CardId), selectedLeft == question.CardId, CreateCommand(() =>
        {
            selectedLeft = question.CardId;
            OnPropertyChanged(nameof(MatchLeft));
            return Task.CompletedTask;
        }))).ToList() ?? [];
    public IReadOnlyList<MatchRow> MatchRight => session?.MatchOrder.Select(cardId =>
    {
        var question = session.Questions.First(item => item.CardId == cardId);
        return new MatchRow(cardId, question.Answer, session.MatchedIds.Contains(cardId), false, CreateCommand(async () =>
        {
            if (selectedLeft is null) throw new StudyException("VSelectLeft");
            var left = selectedLeft.Value;
            await ChangeAsync(current => engine.Match(current, left, cardId));
            selectedLeft = null;
            matchFeedbackKey = left == cardId ? "VCorrect" : "VTryAgain";
            OnPropertyChanged(null);
        }));
    }).ToList() ?? [];
    public ICommand SetupCommand { get; }
    public ICommand ApplySetupCommand { get; }
    public ICommand ContinueCommand { get; }
    public ICommand StarCommand { get; }
    public ICommand FlipCommand { get; }
    public ICommand RememberCommand { get; }
    public ICommand ForgetCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand SpeakCommand { get; }
    public ICommand RetryCommand { get; }
    public Task RefreshAsync() => RunAsync(LoadAsync);

    private async Task LoadAsync()
    {
        await FlushAsync();
        var current = (await repository.ReadAsync()).Session ?? throw new StudyException("VNoSession");
        ResetAutosave();
        ModeIndex = (int)current.Mode;
        DirectionIndex = (int)current.Direction;
        FilterIndex = (int)current.Filter;
        await ReloadStarsAsync();
        Apply(current);
    }

    protected override void OnLanguageChanged(object? sender, EventArgs arguments)
    {
        translating = true;
        try { base.OnLanguageChanged(sender, arguments); OnPropertyChanged(nameof(ModeIndex)); OnPropertyChanged(nameof(DirectionIndex)); OnPropertyChanged(nameof(FilterIndex)); }
        finally { translating = false; }
    }

    private async Task ReloadStarsAsync()
    {
        libraryCards = (await repository.ReadAsync()).Cards;
        starredIds = libraryCards.Where(card => card.IsStarred).Select(card => card.Id).ToHashSet();
    }

    private async Task StartBatchAsync(bool changeSettings)
    {
        await FlushAsync();
        if (session is null) throw new StudyException("VNoSession");
        var data = await repository.ReadAsync();
        var deck = data.Decks.FirstOrDefault(item => item.Id == session.DeckId) ?? throw new StudyException("VNotFound");
        var next = engine.Create(deck, data.Cards.Where(card => card.DeckId == deck.Id).ToList(),
            changeSettings ? (LearningMode)ModeIndex : session.Mode,
            changeSettings ? (LearningDirection)DirectionIndex : session.Direction,
            changeSettings ? (LearningFilter)FilterIndex : session.Filter);
        if (!session.IsComplete && !await Interaction.ConfirmAsync(Localization["VReplaceSession"], Localization["VChangeSetupWarning"], "VStartNew")) return;
        await repository.StartSessionAsync(next);
        ResetAutosave();
        UpdateSetupSelection(next);
        ShowSetup = false;
        selectedLeft = null;
        matchFeedbackKey = "VMatchHint";
        await ReloadStarsAsync();
        Apply(next);
    }

    private void UpdateSetupSelection(LearningSession current)
    {
        ModeIndex = (int)current.Mode;
        DirectionIndex = (int)current.Direction;
        FilterIndex = (int)current.Filter;
    }

    private void Apply(LearningSession current)
    {
        AutosaveEnabled = false;
        session = current;
        Input = current.Input;
        AutosaveEnabled = !current.IsComplete;
        OnPropertyChanged(null);
    }

    protected override async Task PersistDraftAsync()
    {
        if (session is null || session.IsComplete) return;
        var updated = session with { Input = Input };
        await repository.SaveSessionAsync(updated);
        session = updated;
    }

    private async Task ChangeAsync(Func<LearningSession, LearningSession> change)
    {
        await FlushAsync();
        if (session is null) throw new StudyException("VNoSession");
        var next = change(session);
        await repository.SaveSessionAsync(next);
        await ReloadStarsAsync();
        Apply(next);
    }
}
