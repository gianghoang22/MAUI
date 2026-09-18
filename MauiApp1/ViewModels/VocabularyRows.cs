using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;

namespace MauiApp1.ViewModels;

public sealed class NamedRow(string name, int count, string countKey, ICommand openCommand, LocalizationService localization, ICommand? menuCommand = null) : LocalizedObject(localization)
{
    public string Name => name;
    public string Summary => Localization.Format(countKey, count);
    public ICommand OpenCommand => openCommand;
    public ICommand? MenuCommand => menuCommand;
    public bool HasMenu => menuCommand is not null;
}

public sealed class VocabularyCardRow(VocabularyCard card, ICommand openCommand, ICommand starCommand,
    ICommand menuCommand, LocalizationService localization) : LocalizedObject(localization)
{
    private bool isStarred = card.IsStarred;
    public Guid Id => card.Id;
    public string Vietnamese => card.Vietnamese;
    public string English => card.English;
    public ICommand OpenCommand => openCommand;
    public ICommand StarCommand => starCommand;
    public ICommand MenuCommand => menuCommand;
    public bool IsStarred => isStarred;
    public string StarText => IsStarred ? "★" : "☆";
    public string StarDescription => Localization[IsStarred ? "VUnstar" : "VStar"];

    public void SetStarred(bool value)
    {
        if (!SetProperty(ref isStarred, value, nameof(IsStarred))) return;
        OnPropertyChanged(nameof(StarText));
        OnPropertyChanged(nameof(StarDescription));
    }
}

public sealed class ImportRow(WorkbookRow row, bool duplicate, LocalizationService localization) : LocalizedObject(localization)
{
    public WorkbookRow Source => row;
    public int RowNumber => row.RowNumber;
    public string Vietnamese => row.Vietnamese;
    public string English => row.English;
    public bool IsDuplicate => duplicate;
    public bool IsValid => row.ErrorKey is null;
    public string Status => Localization[row.ErrorKey ?? (duplicate ? "VSkippedDuplicate" : "VReady")];
}

public sealed class ResultRow(LearningResult result, ICommand menuCommand, LocalizationService localization) : LocalizedObject(localization)
{
    public string Name => result.DeckName;
    public string Summary => Localization.Format("VResultSummary", result.Correct, result.Total,
        Localization["VMode" + (int)result.Mode], result.FinishedAt.ToLocalTime().ToString("g", Localization.Culture));
    public string Details => Localization.Format("VResultTime", result.Seconds, result.Mistakes);
    public string WrongAnswers => string.Join(Environment.NewLine, result.WrongQuestions.Select(question => $"{question.Prompt} → {string.Join(" / ", question.AcceptedAnswers)}"));
    public bool CanRetry => result.WrongQuestions.Count > 0;
    public ICommand MenuCommand => menuCommand;
}

public sealed record ChoiceRow(string Text, ICommand SelectCommand);
public sealed record MatchRow(Guid Id, string Text, bool IsMatched, bool IsSelected, ICommand SelectCommand)
{
    public bool IsAvailable => !IsMatched;
}
