using System.Text.Json.Serialization;

namespace MauiApp1.Models;

public sealed record VocabularyClass(Guid Id, string Name);
public sealed record VocabularyDeck(Guid Id, Guid ClassId, string Name, string Description);
public sealed record VocabularyCard(Guid Id, Guid DeckId, string Vietnamese, string English, bool IsStarred = false);
public sealed record CardDraft(Guid CardId, Guid DeckId, bool IsNew, string Vietnamese, string English, DateTimeOffset UpdatedAt);
public enum LearningMode { Flashcards, MultipleChoice, Written, Match }
public enum LearningDirection { EnglishToVietnamese, VietnameseToEnglish }
public enum LearningFilter { Unstarred, Starred, All }
public sealed record StudyQuestion(Guid CardId, string Prompt, string Answer, List<string> AcceptedAnswers, List<string> Choices);
public sealed record AnswerAttempt(Guid CardId, string Submitted, bool Correct);

public sealed record LearningSession(
    Guid Id, Guid DeckId, string DeckName, LearningMode Mode, LearningDirection Direction,
    List<StudyQuestion> Questions, List<Guid> MatchOrder, List<Guid> MatchedIds,
    List<AnswerAttempt> Attempts, int Index, int MatchMistakes, string Input, bool Revealed,
    DateTimeOffset StartedAt, DateTimeOffset? FinishedAt)
{
    public LearningFilter Filter { get; init; } = LearningFilter.All;
    [JsonIgnore] public bool IsComplete => FinishedAt.HasValue;
    [JsonIgnore] public bool IsAnswered => Attempts.Count > Index;
    [JsonIgnore] public int Correct => Mode == LearningMode.Match ? MatchedIds.Count : Attempts.Count(attempt => attempt.Correct);
}

public sealed record LearningResult(Guid Id, Guid DeckId, string DeckName, LearningMode Mode,
    LearningDirection Direction, int Total, int Correct, int Mistakes, int Seconds,
    DateTimeOffset FinishedAt, List<StudyQuestion> WrongQuestions);

public sealed class VocabularyData
{
    [JsonRequired] public int SchemaVersion { get; set; } = 1;
    [JsonRequired] public List<VocabularyClass> Classes { get; set; } = [];
    [JsonRequired] public List<VocabularyDeck> Decks { get; set; } = [];
    [JsonRequired] public List<VocabularyCard> Cards { get; set; } = [];
    [JsonRequired] public List<CardDraft> Drafts { get; set; } = [];
    [JsonRequired] public List<LearningResult> Results { get; set; } = [];
    public LearningSession? Session { get; set; }
}
