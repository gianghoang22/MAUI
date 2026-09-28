using MauiApp1.Models;
using MauiApp1.Services;
using Xunit;

namespace MauiApp1.Tests;

public sealed class MultilingualTests
{
    [Theory]
    [InlineData("Japanese", "ja")]
    [InlineData("日本語", "ja")]
    [InlineData("Nhật", "ja")]
    [InlineData("Tiếng Nhật", "ja")]
    [InlineData("EN", "en")]
    [InlineData("한국어", "ko")]
    [InlineData("French", "fr")]
    [InlineData("Klingon", "Klingon")]
    public void LanguageNamesCodesAndCustomLabelsAreAccepted(string header, string expected)
    {
        Assert.Equal(expected, VocabularyLanguages.Canonicalize(header));
    }

    [Theory]
    [InlineData("Japanese", "English", "猫", "cat", "ja", "en")]
    [InlineData("Korean", "Vietnamese", "물", "nước", "ko", "vi")]
    [InlineData("Arabic", "French", "كتاب", "livre", "ar", "fr")]
    [InlineData("Klingon", "English", "Qapla'", "success", "Klingon", "en")]
    public async Task WorkbookRoundTripsArbitraryLanguagePairs(string first, string second, string front, string back,
        string expectedFirst, string expectedSecond)
    {
        var workbook = new XlsxWorkbook();
        using var stream = new MemoryStream();
        workbook.Write(stream, [new(Guid.NewGuid(), Guid.NewGuid(), front, back)], first, second);
        stream.Position = 0;
        var preview = await workbook.ReadAsync(stream);
        Assert.Equal(expectedFirst, preview.FirstLanguage);
        Assert.Equal(expectedSecond, preview.SecondLanguage);
        Assert.Equal(front, preview.Rows[0].Vietnamese);
        Assert.Equal(back, preview.Rows[0].English);
    }

    [Fact]
    public async Task LegacyReversedEnglishVietnameseWorkbookStillMapsCorrectly()
    {
        using var stream = new MemoryStream();
        var workbook = new XlsxWorkbook();
        workbook.Write(stream, [new(Guid.NewGuid(), Guid.NewGuid(), "apple", "quả táo")], "en", "vi");
        stream.Position = 0;
        var preview = await workbook.ReadAsync(stream);
        Assert.Equal("vi", preview.FirstLanguage);
        Assert.Equal("en", preview.SecondLanguage);
        Assert.Equal("quả táo", preview.Rows[0].Vietnamese);
        Assert.Equal("apple", preview.Rows[0].English);
    }

    [Fact]
    public void ExistingDeckReversesMatchingLanguagesButRejectsDifferentPair()
    {
        var deck = new VocabularyDeck(Guid.NewGuid(), Guid.NewGuid(), "Japanese", "") { FirstLanguage = "ja", SecondLanguage = "en" };
        var data = new VocabularyData { Decks = [deck], Cards = [new(Guid.NewGuid(), deck.Id, "猫", "cat")] };
        var preview = new WorkbookPreview("Words", [new(2, "dog", "犬", null)]) { FirstLanguage = "English", SecondLanguage = "日本語" };
        var prepared = VocabularyImport.Prepare(data, deck.Id, preview);
        Assert.Equal("ja", prepared.FirstLanguage);
        Assert.Equal("犬", prepared.Rows[0].Vietnamese);
        Assert.Equal("dog", prepared.Rows[0].English);
        Assert.Equal("dog", preview.Rows[0].Vietnamese);
        Assert.Throws<StudyException>(() => VocabularyImport.Prepare(data, deck.Id, preview with { SecondLanguage = "French" }));
    }

    [Fact]
    public void EmptyDeckAdoptsHeadersButDraftsPreventImplicitRelabeling()
    {
        var deck = new VocabularyDeck(Guid.NewGuid(), Guid.NewGuid(), "New", "");
        var data = new VocabularyData { Decks = [deck] };
        var preview = new WorkbookPreview("Words", [new(2, "猫", "cat", null)]) { FirstLanguage = "Japanese", SecondLanguage = "English" };
        Assert.Equal("ja", VocabularyImport.Prepare(data, deck.Id, preview).FirstLanguage);
        data.Drafts.Add(new(Guid.NewGuid(), deck.Id, true, "con mèo", "cat", DateTimeOffset.UtcNow));
        Assert.Throws<StudyException>(() => VocabularyImport.Prepare(data, deck.Id, preview));
    }

    [Theory]
    [InlineData(LearningDirection.EnglishToVietnamese, "cat", "猫")]
    [InlineData(LearningDirection.VietnameseToEnglish, "猫", "cat")]
    public void JapaneseEnglishStudyGradesBothDirectionsAndKeepsHistoryLanguages(LearningDirection direction, string prompt, string answer)
    {
        var deck = new VocabularyDeck(Guid.NewGuid(), Guid.NewGuid(), "Japanese", "") { FirstLanguage = "ja", SecondLanguage = "en" };
        var engine = new LearningEngine();
        var session = engine.Create(deck, [new(Guid.NewGuid(), deck.Id, "猫", "cat")], LearningMode.Written, direction);
        Assert.Equal(prompt, session.Questions[0].Prompt);
        session = engine.Next(engine.Submit(session, answer));
        Assert.Equal(1, session.Correct);
        Assert.Equal("ja", session.FirstLanguage);
        Assert.Equal("en", session.SecondLanguage);
        var result = new LearningResult(session.Id, deck.Id, deck.Name, LearningMode.Written, direction,
            1, 0, 0, 1, DateTimeOffset.UtcNow, session.Questions) { FirstLanguage = "ja", SecondLanguage = "en" };
        Assert.Equal("ja", engine.Retry(result).FirstLanguage);
        Assert.Contains("Japanese", VocabularyLanguages.Direction("ja", "en", direction, "en"));
    }

    [Fact]
    public async Task ImportCommitsLanguageMetadataAndCardsAtomically()
    {
        using var directory = new TestDirectory();
        var repository = new JsonVocabularyRepository(directory.Path);
        var deck = new VocabularyDeck(Guid.NewGuid(), Guid.NewGuid(), "New", "");
        await repository.SaveClassAsync(new(deck.ClassId, "Languages"));
        await repository.SaveDeckAsync(deck);
        var good = new VocabularyCard(Guid.NewGuid(), deck.Id, "猫", "cat");
        var bad = new VocabularyCard(Guid.NewGuid(), deck.Id, "", "invalid");
        await Assert.ThrowsAsync<StudyException>(() => repository.ImportAsync(deck.Id, [good, bad], "ja", "en"));
        var untouched = await repository.ReadAsync();
        Assert.Empty(untouched.Cards);
        Assert.Equal("vi", untouched.Decks[0].FirstLanguage);

        Assert.Equal(1, await repository.ImportAsync(deck.Id, [good], "Japanese", "English"));
        var restored = await new JsonVocabularyRepository(directory.Path).ReadAsync();
        Assert.Equal("ja", restored.Decks[0].FirstLanguage);
        Assert.Equal("猫", restored.Cards[0].Vietnamese);
        Assert.Equal(0, await repository.ImportAsync(deck.Id, [good], "ja", "en"));
        await Assert.ThrowsAsync<StudyException>(() => repository.ImportAsync(deck.Id, [good], "fr", "en"));
        Assert.Single((await repository.ReadAsync()).Cards);
    }

    private sealed class TestDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "vocabmate-language-tests-" + Guid.NewGuid().ToString("N"));
        public TestDirectory() => Directory.CreateDirectory(Path);
        public void Dispose() => Directory.Delete(Path, true);
    }
}
