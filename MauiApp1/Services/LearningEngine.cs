using MauiApp1.Models;

namespace MauiApp1.Services;

public sealed class LearningEngine
{
    public LearningSession Create(VocabularyDeck deck, IReadOnlyList<VocabularyCard> cards, LearningMode mode, LearningDirection direction,
        LearningFilter filter = LearningFilter.All)
        => CreateBatch(deck, cards, mode, direction, filter, []);

    private LearningSession CreateBatch(VocabularyDeck deck, IReadOnlyList<VocabularyCard> cards, LearningMode mode,
        LearningDirection direction, LearningFilter filter, List<string> completedPrompts)
    {
        if (!Enum.IsDefined(mode) || !Enum.IsDefined(direction) || !Enum.IsDefined(filter)) throw new StudyException("VInvalidSession");
        var completed = completedPrompts.ToHashSet(StringComparer.Ordinal);
        var eligible = cards.Where(card => card.DeckId == deck.Id &&
            (filter == LearningFilter.All || card.IsStarred == (filter == LearningFilter.Starred)) &&
            !completed.Contains(VocabularyRules.Normalize(Prompt(card, direction))));
        var groups = eligible.GroupBy(card => VocabularyRules.Normalize(Prompt(card, direction))).ToList();
        if (groups.Count == 0) throw new StudyException(cards.Count == 0 ? "VNoCards" : "VNoFilteredCards");
        var allAnswers = cards.Where(card => card.DeckId == deck.Id).Select(card => Answer(card, direction)).DistinctBy(VocabularyRules.Normalize).ToList();
        var questions = new List<StudyQuestion>();
        var usedMatchAnswers = new HashSet<string>();
        var orderedGroups = Shuffle(groups);
        if (mode == LearningMode.Match)
            orderedGroups = orderedGroups.OrderBy(group => group.Select(card => Answer(card, direction)).DistinctBy(VocabularyRules.Normalize).Count()).ToList();
        foreach (var group in orderedGroups)
        {
            var card = group.First();
            var accepted = cards.Where(item => item.DeckId == deck.Id && VocabularyRules.Normalize(Prompt(item, direction)) == group.Key)
                .Select(item => Answer(item, direction)).DistinctBy(VocabularyRules.Normalize).ToList();
            var answer = Answer(card, direction);
            if (mode == LearningMode.Match)
            {
                var available = accepted.FirstOrDefault(candidate => !usedMatchAnswers.Contains(VocabularyRules.Normalize(candidate)));
                if (available is null) continue;
                answer = available;
                usedMatchAnswers.Add(VocabularyRules.Normalize(answer));
            }
            var choices = new List<string>();
            if (mode == LearningMode.MultipleChoice)
            {
                var acceptedKeys = accepted.Select(VocabularyRules.Normalize).ToHashSet();
                var wrong = Shuffle(allAnswers.Where(answer => !acceptedKeys.Contains(VocabularyRules.Normalize(answer)))).Take(3).ToList();
                if (wrong.Count < 3) throw new StudyException("VNeedFourAnswers");
                choices = Shuffle(wrong.Append(Answer(card, direction)));
            }
            questions.Add(new StudyQuestion(card.Id, Prompt(card, direction), answer, accepted, choices));
            if (questions.Count == (mode == LearningMode.Match ? 6 : 20)) break;
        }
        if (mode == LearningMode.Match && questions.Count < 2 && completedPrompts.Count == 0) throw new StudyException("VNeedPairs");
        return NewSession(deck.Id, deck.Name, mode, direction, questions) with
        {
            Filter = filter, CompletedPrompts = completedPrompts,
            FirstLanguage = deck.FirstLanguage, SecondLanguage = deck.SecondLanguage
        };
    }

    public bool HasNextBatch(LearningSession session, IReadOnlyList<VocabularyCard> cards)
    {
        var completed = CompletedPrompts(session).ToHashSet(StringComparer.Ordinal);
        return cards.Any(card => card.DeckId == session.DeckId &&
            (session.Filter == LearningFilter.All || card.IsStarred == (session.Filter == LearningFilter.Starred)) &&
            !completed.Contains(VocabularyRules.Normalize(Prompt(card, session.Direction))));
    }

    public LearningSession Continue(VocabularyDeck deck, IReadOnlyList<VocabularyCard> cards, LearningSession session)
    {
        if (!session.IsComplete || deck.Id != session.DeckId) throw new StudyException("VInvalidSession");
        if (!HasNextBatch(session, cards)) throw new StudyException("VRoundComplete");
        return CreateBatch(deck with { FirstLanguage = session.FirstLanguage, SecondLanguage = session.SecondLanguage },
            cards, session.Mode, session.Direction, session.Filter, CompletedPrompts(session));
    }

    private static List<string> CompletedPrompts(LearningSession session) => session.CompletedPrompts
        .Concat(session.Questions.Select(question => VocabularyRules.Normalize(question.Prompt))).Distinct().ToList();

    public LearningSession Retry(LearningResult result)
    {
        if (result.WrongQuestions.Count == 0) throw new StudyException("VNoWrongAnswers");
        return NewSession(result.DeckId, result.DeckName, result.Mode, result.Direction, Shuffle(result.WrongQuestions)) with
        { FirstLanguage = result.FirstLanguage, SecondLanguage = result.SecondLanguage };
    }

    public LearningSession Submit(LearningSession session, string answer)
    {
        RequireAnswerable(session);
        if (session.Mode is not (LearningMode.MultipleChoice or LearningMode.Written)) throw new StudyException("VInvalidSession");
        if (string.IsNullOrWhiteSpace(answer)) throw new StudyException("VAnswerRequired");
        var question = session.Questions[session.Index];
        if (session.Mode == LearningMode.MultipleChoice && !question.Choices.Contains(answer)) throw new StudyException("VInvalidSession");
        var correct = question.AcceptedAnswers.Any(expected => VocabularyRules.Normalize(expected) == VocabularyRules.Normalize(answer));
        return session with { Attempts = [.. session.Attempts, new AnswerAttempt(question.CardId, answer, correct)], Input = answer, Revealed = true };
    }

    public LearningSession Rate(LearningSession session, bool remembered)
    {
        RequireAnswerable(session);
        if (session.Mode != LearningMode.Flashcards) throw new StudyException("VInvalidSession");
        return session with { Attempts = [.. session.Attempts, new AnswerAttempt(session.Questions[session.Index].CardId, "", remembered)] };
    }

    public LearningSession Next(LearningSession session)
    {
        if (session.IsComplete || !session.IsAnswered) throw new StudyException("VInvalidSession");
        var next = session.Index + 1;
        return session with { Index = next, Input = "", Revealed = false, FinishedAt = next == session.Questions.Count ? DateTimeOffset.UtcNow : null };
    }

    public LearningSession Match(LearningSession session, Guid left, Guid right)
    {
        if (session.Mode != LearningMode.Match || session.IsComplete || session.MatchedIds.Contains(left) || session.MatchedIds.Contains(right) ||
            !session.Questions.Any(question => question.CardId == left) || !session.Questions.Any(question => question.CardId == right))
            throw new StudyException("VInvalidSession");
        if (left != right) return session with { MatchMistakes = session.MatchMistakes + 1 };
        var matched = new List<Guid>(session.MatchedIds) { left };
        return session with { MatchedIds = matched, FinishedAt = matched.Count == session.Questions.Count ? DateTimeOffset.UtcNow : null };
    }

    private static void RequireAnswerable(LearningSession session)
    {
        if (session.IsComplete || session.Index >= session.Questions.Count || session.IsAnswered) throw new StudyException("VInvalidSession");
    }

    private static LearningSession NewSession(Guid deckId, string name, LearningMode mode, LearningDirection direction, List<StudyQuestion> questions) =>
        new(Guid.NewGuid(), deckId, name, mode, direction, questions, Shuffle(questions.Select(question => question.CardId)), [], [], 0, 0, "", false, DateTimeOffset.UtcNow, null);

    private static string Prompt(VocabularyCard card, LearningDirection direction) => direction == LearningDirection.EnglishToVietnamese ? card.English : card.Vietnamese;
    private static string Answer(VocabularyCard card, LearningDirection direction) => direction == LearningDirection.EnglishToVietnamese ? card.Vietnamese : card.English;

    private static List<T> Shuffle<T>(IEnumerable<T> source)
    {
        var items = source.ToArray();
        Random.Shared.Shuffle(items);
        return [.. items];
    }
}
