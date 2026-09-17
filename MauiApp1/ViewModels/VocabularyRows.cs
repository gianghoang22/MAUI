using System.Windows.Input;
using MauiApp1.Localization;
using MauiApp1.Models;

namespace MauiApp1.ViewModels;

public sealed class NamedRow(string name, int count, string countKey, ICommand openCommand, LocalizationService localization) : LocalizedObject(localization)
{
    public string Name => name;
    public string Summary => Localization.Format(countKey, count);
    public ICommand OpenCommand => openCommand;
}

public sealed record VocabularyCardRow(string Vietnamese, string English, ICommand OpenCommand);

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

public sealed class ResultRow(LearningResult result, ICommand retryCommand, LocalizationService localization) : LocalizedObject(localization)
{
    public string Name => result.DeckName;
    public string Summary => Localization.Format("VResultSummary", result.Correct, result.Total,
        Localization["VMode" + (int)result.Mode], result.FinishedAt.ToLocalTime().ToString("g", Localization.Culture));
    public string Details => Localization.Format("VResultTime", result.Seconds, result.Mistakes);
    public string WrongAnswers => string.Join(Environment.NewLine, result.WrongQuestions.Select(question => $"{question.Prompt} → {string.Join(" / ", question.AcceptedAnswers)}"));
    public bool CanRetry => result.WrongQuestions.Count > 0;
    public ICommand RetryCommand => retryCommand;
}

public sealed record ChoiceRow(string Text, ICommand SelectCommand);
public sealed record MatchRow(Guid Id, string Text, bool IsMatched, bool IsSelected, ICommand SelectCommand)
{
    public bool IsAvailable => !IsMatched;
}
