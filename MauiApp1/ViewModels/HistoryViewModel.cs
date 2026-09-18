using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public sealed class HistoryViewModel : ViewModelBase, IRefreshable
{
    private readonly IVocabularyRepository repository;
    private readonly LearningEngine engine;
    private IReadOnlyList<ResultRow> results = [];

    public HistoryViewModel(IVocabularyRepository repository, LearningEngine engine, IUserInteraction interaction, LocalizationService localization) : base(interaction, localization)
    {
        this.repository = repository;
        this.engine = engine;
        RefreshCommand = CreateCommand(LoadAsync);
    }

    public IReadOnlyList<ResultRow> Results
    {
        get => results;
        private set
        {
            if (!SetProperty(ref results, value)) return;
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(IsEmpty));
        }
    }
    public bool HasResults => Results.Count > 0;
    public bool IsEmpty => !HasResults;
    public ICommand RefreshCommand { get; }
    public ICommand MenuCommand => CreateMenuCommand(() => Localization["VHistoryHeading"], new MenuAction("Refresh", RefreshCommand));
    public Task RefreshAsync() => RunAsync(LoadAsync);

    private async Task LoadAsync()
    {
        Results = (await repository.ReadAsync()).Results.OrderByDescending(result => result.FinishedAt).Select(result =>
            new ResultRow(result, CreateMenuCommand(() => result.DeckName, new MenuAction("VRetryWrong", CreateCommand(async () =>
            {
                var data = await repository.ReadAsync();
                if (data.Session is { IsComplete: false } && !await Interaction.ConfirmAsync(Localization["VReplaceSession"], Localization["VReplaceSessionWarning"], "VStartNew")) return;
                await repository.StartSessionAsync(engine.Retry(result));
                await Interaction.NavigateAsync("learn");
            }), () => result.WrongQuestions.Count > 0)), Localization)).ToList();
    }
}
