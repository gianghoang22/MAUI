using System.Text;
using MauiApp1.Models;
using MauiApp1.Services;
using Xunit;

namespace MauiApp1.Tests;

public sealed class CoreBehaviorTests
{
    [Fact]
    public async Task RepositoryRejectsDuplicatesAndKeepsInvalidImportAtomic()
    {
        using var dataDirectory = new TemporaryDirectory();
        var repository = new JsonVocabularyRepository(dataDirectory.Path);
        var classroom = new VocabularyClass(Guid.NewGuid(), "English");
        var deck = new VocabularyDeck(Guid.NewGuid(), classroom.Id, "Everyday", "");

        await repository.SaveClassAsync(classroom);
        await Assert.ThrowsAsync<StudyException>(() => repository.SaveClassAsync(new(Guid.NewGuid(), " english ")));
        await repository.SaveDeckAsync(deck);

        var validCards = new List<VocabularyCard>
        {
            new(Guid.NewGuid(), deck.Id, "quả táo", "apple"),
            new(Guid.NewGuid(), deck.Id, "con mèo", "cat")
        };
        Assert.Equal(2, await repository.ImportAsync(deck.Id, validCards));

        var invalidBatch = new[]
        {
            new VocabularyCard(Guid.NewGuid(), deck.Id, "nước", "water"),
            new VocabularyCard(Guid.NewGuid(), deck.Id, "", "invalid")
        };
        await Assert.ThrowsAsync<StudyException>(() => repository.ImportAsync(deck.Id, invalidBatch));
        Assert.Equal(2, (await repository.ReadAsync()).Cards.Count);
    }

    [Fact]
    public void SearchNormalizationIgnoresAccentsCaseAndWhitespaceButRulesKeepGradingStrict()
    {
        Assert.Equal(VocabularySearch.Normalize("  ĐI  "), VocabularySearch.Normalize("di"));
        Assert.Equal(VocabularySearch.Normalize("quả   táo"), VocabularySearch.Normalize("QUA TAO"));
        Assert.NotEqual(VocabularyRules.Normalize("má"), VocabularyRules.Normalize("ma"));
    }

    [Fact]
    public void LearningFilterExcludesMasteredCards()
    {
        var deck = new VocabularyDeck(Guid.NewGuid(), Guid.NewGuid(), "Everyday", "");
        var cards = Enumerable.Range(0, 5)
            .Select(index => new VocabularyCard(Guid.NewGuid(), deck.Id, $"từ {index}", $"word {index}", index < 2))
            .ToList();
        var engine = new LearningEngine();

        var session = engine.Create(deck, cards, LearningMode.Flashcards, LearningDirection.EnglishToVietnamese, LearningFilter.Unstarred);

        Assert.DoesNotContain(session.Questions, question => cards.First(card => card.Id == question.CardId).IsStarred);
        Assert.Equal(3, session.Questions.Count);
    }

    [Fact]
    public async Task CompletedSessionIsPersistedOnlyOnceAndRejectsStaleProgress()
    {
        using var dataDirectory = new TemporaryDirectory();
        var repository = new JsonVocabularyRepository(dataDirectory.Path);
        var classroom = new VocabularyClass(Guid.NewGuid(), "English");
        var deck = new VocabularyDeck(Guid.NewGuid(), classroom.Id, "Everyday", "");
        var card = new VocabularyCard(Guid.NewGuid(), deck.Id, "quả táo", "apple");
        var engine = new LearningEngine();

        await repository.SaveClassAsync(classroom);
        await repository.SaveDeckAsync(deck);
        await repository.SaveCardAsync(card);
        var session = engine.Create(deck, [card], LearningMode.Flashcards, LearningDirection.EnglishToVietnamese);
        await repository.StartSessionAsync(session);

        var completed = engine.Next(engine.Rate(session with { Revealed = true }, true));
        await repository.SaveSessionAsync(completed);
        await repository.SaveSessionAsync(completed);

        var data = await repository.ReadAsync();
        Assert.Single(data.Results);
        await Assert.ThrowsAsync<StudyException>(() => repository.SaveSessionAsync(session));
    }

    [Fact]
    public async Task WorkbookRoundTripPreservesVocabulary()
    {
        var deckId = Guid.NewGuid();
        var cards = new[]
        {
            new VocabularyCard(Guid.NewGuid(), deckId, "quả táo", "apple"),
            new VocabularyCard(Guid.NewGuid(), deckId, "con mèo", "cat")
        };
        var workbook = new XlsxWorkbook();
        using var stream = new MemoryStream();
        workbook.Write(stream, cards);
        stream.Position = 0;

        var preview = await workbook.ReadAsync(stream);

        Assert.Equal(2, preview.Rows.Count);
        Assert.Equal("quả táo", preview.Rows[0].Vietnamese);
        Assert.Equal("apple", preview.Rows[0].English);
        Assert.All(preview.Rows, row => Assert.Null(row.ErrorKey));
    }

    [Theory]
    [InlineData("VocabMate-template-daily-life.xlsx", 10, 0, 0)]
    [InlineData("VocabMate-template-travel.xlsx", 10, 0, 0)]
    [InlineData("VocabMate-template-preview-cases.xlsx", 5, 1, 1)]
    public async Task SampleWorkbooksAreReadableByImporter(string fileName, int expectedRows, int expectedInvalidRows, int expectedDuplicatePairs)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Samples", fileName);
        await using var stream = File.OpenRead(path);

        var preview = await new XlsxWorkbook().ReadAsync(stream);

        Assert.Equal("vi", preview.FirstLanguage);
        Assert.Equal("en", preview.SecondLanguage);
        Assert.Equal(expectedRows, preview.Rows.Count);
        Assert.Equal(expectedInvalidRows, preview.Rows.Count(row => row.ErrorKey is not null));
        Assert.Equal(expectedDuplicatePairs, preview.Rows
            .GroupBy(row => VocabularyRules.PairKey(row.Vietnamese, row.English))
            .Count(group => group.Count() > 1));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory() => Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "vocabmate-tests-" + Guid.NewGuid().ToString("N"));
        public string Path { get; }
        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, true);
        }
    }
}
