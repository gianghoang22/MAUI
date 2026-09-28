using System.Text.Json;
using System.Text.Json.Nodes;
using MauiApp1.Models;
using MauiApp1.Services;
using Xunit;

namespace MauiApp1.Tests;

public sealed class LearningRoundTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MatchingAcceptsEitherSideFirst(bool leftFirst)
    {
        var deck = NewDeck();
        var engine = new LearningEngine();
        var session = engine.Create(deck, Cards(deck, 6), LearningMode.Match, LearningDirection.EnglishToVietnamese);
        var selection = new MatchSelection();
        var cardId = session.Questions[0].CardId;

        Assert.Null(selection.Select(session, cardId, leftFirst));
        Assert.Equal(cardId, leftFirst ? selection.Left : selection.Right);
        var pair = selection.Select(session, cardId, !leftFirst);
        Assert.NotNull(pair);
        session = engine.Match(session, pair.Value.Left, pair.Value.Right);

        Assert.Single(session.MatchedIds);
        Assert.Equal(0, session.MatchMistakes);
        Assert.Null(selection.Left);
        Assert.Null(selection.Right);
        Assert.Null(selection.Select(session, cardId, leftFirst));
    }

    [Fact]
    public void MatchingCanDeselectReselectAndRecoverFromWrongPair()
    {
        var deck = NewDeck();
        var engine = new LearningEngine();
        var session = engine.Create(deck, Cards(deck, 3), LearningMode.Match, LearningDirection.EnglishToVietnamese);
        var first = session.Questions[0].CardId;
        var second = session.Questions[1].CardId;
        var selection = new MatchSelection();
        selection.Select(session, first, false);
        selection.Select(session, first, false);
        Assert.Null(selection.Right);
        selection.Select(session, first, false);
        selection.Select(session, second, false);
        Assert.Equal(second, selection.Right);
        var wrong = selection.Select(session, first, true)!.Value;
        session = engine.Match(session, wrong.Left, wrong.Right);
        Assert.Equal(1, session.MatchMistakes);
        Assert.Empty(session.MatchedIds);
        selection.Select(session, first, false);
        var correct = selection.Select(session, first, true)!.Value;
        session = engine.Match(session, correct.Left, correct.Right);
        Assert.Single(session.MatchedIds);
        Assert.Equal(1, session.MatchMistakes);
    }

    [Theory]
    [InlineData(LearningMode.Match, 7, LearningDirection.EnglishToVietnamese)]
    [InlineData(LearningMode.Match, 12, LearningDirection.VietnameseToEnglish)]
    [InlineData(LearningMode.Match, 13, LearningDirection.EnglishToVietnamese)]
    [InlineData(LearningMode.Match, 25, LearningDirection.VietnameseToEnglish)]
    [InlineData(LearningMode.Flashcards, 25, LearningDirection.EnglishToVietnamese)]
    [InlineData(LearningMode.MultipleChoice, 25, LearningDirection.VietnameseToEnglish)]
    [InlineData(LearningMode.Written, 25, LearningDirection.EnglishToVietnamese)]
    public void NextBatchesExhaustTheDeckWithoutRepeating(LearningMode mode, int count, LearningDirection direction)
    {
        var deck = NewDeck();
        var cards = Cards(deck, count);
        var engine = new LearningEngine();
        var session = engine.Create(deck, cards, mode, direction);
        var visited = new HashSet<Guid>();
        var sizes = new List<int>();
        while (true)
        {
            sizes.Add(session.Questions.Count);
            foreach (var question in session.Questions) Assert.True(visited.Add(question.CardId), "Repeated card in a round");
            session = Complete(engine, session);
            if (!engine.HasNextBatch(session, cards)) break;
            session = engine.Continue(deck, cards, session);
        }
        Assert.Equal(count, visited.Count);
        Assert.All(sizes, size => Assert.InRange(size, 1, mode == LearningMode.Match ? 6 : 20));
        if (mode == LearningMode.Match && count == 13) Assert.Equal(new[] { 6, 6, 1 }, sizes);
        Assert.Throws<StudyException>(() => engine.Continue(deck, cards, session));
        Assert.NotEmpty(engine.Create(deck, cards, mode, direction).Questions);
    }

    [Fact]
    public void ContinueRequiresCompletionAndInitialMatchRequiresTwoPairs()
    {
        var deck = NewDeck();
        var cards = Cards(deck, 7);
        var engine = new LearningEngine();
        var session = engine.Create(deck, cards, LearningMode.Match, LearningDirection.EnglishToVietnamese);
        Assert.Throws<StudyException>(() => engine.Continue(deck, cards, session));
        Assert.Throws<StudyException>(() => engine.Create(deck, cards.Take(1).ToList(), LearningMode.Match, session.Direction));
    }

    [Theory]
    [InlineData(LearningFilter.All, 8)]
    [InlineData(LearningFilter.Starred, 3)]
    [InlineData(LearningFilter.Unstarred, 5)]
    public void BatchFiltersKeepTheirMeaningAfterThreeCardsAreStarred(LearningFilter filter, int expected)
    {
        var deck = NewDeck();
        var cards = Cards(deck, 8).Select((card, index) => card with { IsStarred = index < 3 }).ToList();
        var engine = new LearningEngine();
        var session = engine.Create(deck, cards, LearningMode.Flashcards, LearningDirection.EnglishToVietnamese, filter);
        Assert.Equal(expected, session.Questions.Count);
    }

    [Fact]
    public void CompletedPromptGroupsDoNotReturnUnderAnotherCardId()
    {
        var deck = NewDeck();
        var cards = Cards(deck, 8);
        cards.Add(new(Guid.NewGuid(), deck.Id, "another meaning", cards[0].English));
        var engine = new LearningEngine();
        var session = Complete(engine, engine.Create(deck, cards, LearningMode.Match, LearningDirection.EnglishToVietnamese));
        var prompts = session.Questions.Select(question => question.Prompt).ToHashSet();
        var next = engine.Continue(deck, cards, session);
        Assert.Equal(2, next.Questions.Count);
        Assert.DoesNotContain(next.Questions, question => prompts.Contains(question.Prompt));
    }

    [Fact]
    public async Task RoundProgressAndLanguagesSurviveReload()
    {
        using var directory = new TestDirectory();
        var repository = new JsonVocabularyRepository(directory.Path);
        var deck = NewDeck() with { FirstLanguage = "ja", SecondLanguage = "en" };
        await repository.SaveClassAsync(new(deck.ClassId, "Languages"));
        await repository.SaveDeckAsync(deck);
        var cards = Cards(deck, 13);
        await repository.ImportAsync(deck.Id, cards);
        var engine = new LearningEngine();
        var first = engine.Create(deck, cards, LearningMode.Match, LearningDirection.VietnameseToEnglish);
        await repository.StartSessionAsync(first);
        first = Complete(engine, first);
        await repository.SaveSessionAsync(first);
        var second = engine.Continue(deck, cards, first);
        await repository.StartSessionAsync(second);
        second = engine.Match(second, second.Questions[0].CardId, second.Questions[0].CardId);
        await repository.SaveSessionAsync(second);

        var restored = (await new JsonVocabularyRepository(directory.Path).ReadAsync()).Session!;
        Assert.Equal(6, restored.CompletedPrompts.Count);
        Assert.Single(restored.MatchedIds);
        Assert.Equal("ja", restored.FirstLanguage);
        restored = Complete(engine, restored);
        await repository.SaveSessionAsync(restored);
        var third = engine.Continue(deck, cards, restored);
        Assert.Single(third.Questions);
        Assert.DoesNotContain(third.Questions[0].CardId, first.Questions.Concat(second.Questions).Select(question => question.CardId));
        Assert.Equal("ja", (await repository.ReadAsync()).Results[0].FirstLanguage);
    }

    [Fact]
    public async Task LegacyJsonKeepsVietnameseEnglishDefaults()
    {
        using var directory = new TestDirectory();
        var deck = NewDeck();
        var cards = Cards(deck, 2);
        var engine = new LearningEngine();
        var session = engine.Create(deck, cards, LearningMode.Match, LearningDirection.EnglishToVietnamese);
        var data = new VocabularyData { Classes = [new(deck.ClassId, "Legacy")], Decks = [deck], Cards = cards, Session = session };
        var json = JsonNode.Parse(JsonSerializer.Serialize(data))!;
        foreach (var property in new[] { "FirstLanguage", "SecondLanguage" })
        {
            json["Decks"]![0]!.AsObject().Remove(property);
            json["Session"]!.AsObject().Remove(property);
        }
        json["Session"]!.AsObject().Remove("CompletedPrompts");
        await File.WriteAllTextAsync(System.IO.Path.Combine(directory.Path, "vocabmate.json"), json.ToJsonString());
        var restored = await new JsonVocabularyRepository(directory.Path).ReadAsync();
        Assert.Equal("vi", restored.Decks[0].FirstLanguage);
        Assert.Equal("en", restored.Session!.SecondLanguage);
        Assert.Empty(restored.Session.CompletedPrompts);
    }

    private static VocabularyDeck NewDeck() => new(Guid.NewGuid(), Guid.NewGuid(), "Words", "");
    private static List<VocabularyCard> Cards(VocabularyDeck deck, int count) => Enumerable.Range(1, count)
        .Select(index => new VocabularyCard(Guid.NewGuid(), deck.Id, $"từ {index}", $"word {index}")).ToList();

    private static LearningSession Complete(LearningEngine engine, LearningSession session)
    {
        while (!session.IsComplete)
        {
            if (session.Mode == LearningMode.Match)
            {
                var cardId = session.Questions.First(question => !session.MatchedIds.Contains(question.CardId)).CardId;
                session = engine.Match(session, cardId, cardId);
            }
            else
            {
                session = session.Mode == LearningMode.Flashcards ? engine.Rate(session, true) :
                    engine.Submit(session, session.Questions[session.Index].Answer);
                session = engine.Next(session);
            }
        }
        return session;
    }

    private sealed class TestDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "vocabmate-round-tests-" + Guid.NewGuid().ToString("N"));
        public TestDirectory() => Directory.CreateDirectory(Path);
        public void Dispose() => Directory.Delete(Path, true);
    }
}
