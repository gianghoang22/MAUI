using MauiApp1.Models;

namespace MauiApp1.Services;

public static class HomeOverview
{
    public static IReadOnlyList<VocabularyDeck> RecentDecks(VocabularyData data)
    {
        var activity = data.Results.Select(result => (result.DeckId, At: result.FinishedAt));
        if (data.Session is { } session)
            activity = activity.Append((session.DeckId, session.FinishedAt ?? session.StartedAt));

        var decks = data.Decks.ToDictionary(deck => deck.Id);
        return activity.GroupBy(item => item.DeckId)
            .OrderByDescending(group => group.Max(item => item.At))
            .ThenBy(group => group.Key)
            .Where(group => decks.ContainsKey(group.Key))
            .Take(6)
            .Select(group => decks[group.Key])
            .ToArray();
    }

    public static LearningSession? ResumableSession(VocabularyData data) =>
        data.Session is { IsComplete: false, Questions.Count: > 0 } session
        && data.Decks.Any(deck => deck.Id == session.DeckId) ? session : null;

    public static int CompletedQuestions(LearningSession session) =>
        Math.Clamp(session.Mode == LearningMode.Match ? session.MatchedIds.Count : session.Index, 0, session.Questions.Count);
}
