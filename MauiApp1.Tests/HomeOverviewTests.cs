using System.Text.RegularExpressions;
using System.Xml.Linq;
using MauiApp1.Controls;
using MauiApp1.Models;
using MauiApp1.Services;
using Xunit;

namespace MauiApp1.Tests;

public sealed class HomeOverviewTests
{
    [Fact]
    public void RecentDecksUseRealActivityDeduplicateAndSkipDeletedDecks()
    {
        var first = new VocabularyDeck(Guid.NewGuid(), Guid.NewGuid(), "First", "");
        var second = new VocabularyDeck(Guid.NewGuid(), first.ClassId, "Second", "");
        var untouched = new VocabularyDeck(Guid.NewGuid(), first.ClassId, "Untouched", "");
        var start = DateTimeOffset.Parse("2026-09-01T00:00:00Z");
        var data = new VocabularyData
        {
            Decks = [first, second, untouched],
            Results = [Result(first, start), Result(second, start.AddDays(1)), Result(first, start.AddDays(2)), Result(untouched with { Id = Guid.NewGuid() }, start.AddDays(4))],
            Session = Session(second, start.AddDays(3))
        };

        Assert.Equal(new[] { second.Id, first.Id }, HomeOverview.RecentDecks(data).Select(deck => deck.Id));
    }

    [Fact]
    public void RecentDecksAreBoundedAndEmptyHistoryStaysEmpty()
    {
        var start = DateTimeOffset.Parse("2026-09-01T00:00:00Z");
        var data = new VocabularyData();
        Assert.Empty(HomeOverview.RecentDecks(data));
        for (var index = 0; index < 10; index++)
        {
            var deck = new VocabularyDeck(Guid.NewGuid(), Guid.NewGuid(), index.ToString(), "");
            data.Decks.Add(deck);
            data.Results.Add(Result(deck, start.AddDays(index)));
        }
        Assert.Equal(new[] { "9", "8", "7", "6", "5", "4" }, HomeOverview.RecentDecks(data).Select(deck => deck.Name));
    }

    [Fact]
    public void ResumeRequiresAnExistingDeckAndAnUnfinishedNonemptySession()
    {
        var deck = new VocabularyDeck(Guid.NewGuid(), Guid.NewGuid(), "Deck", "");
        var session = Session(deck, DateTimeOffset.UtcNow);
        var data = new VocabularyData { Decks = [deck], Session = session };
        Assert.Same(session, HomeOverview.ResumableSession(data));
        data.Session = session with { FinishedAt = DateTimeOffset.UtcNow };
        Assert.Null(HomeOverview.ResumableSession(data));
        data.Session = session with { Questions = [] };
        Assert.Null(HomeOverview.ResumableSession(data));
        data.Session = session;
        data.Decks.Clear();
        Assert.Null(HomeOverview.ResumableSession(data));
    }

    [Fact]
    public void ResumeProgressSupportsMatchingAndBoundsInvalidPositions()
    {
        var deck = new VocabularyDeck(Guid.NewGuid(), Guid.NewGuid(), "Deck", "");
        var session = Session(deck, DateTimeOffset.UtcNow);
        Assert.Equal(0, HomeOverview.CompletedQuestions(session));
        Assert.Equal(1, HomeOverview.CompletedQuestions(session with { Index = 20 }));
        Assert.Equal(0, HomeOverview.CompletedQuestions(session with { Index = -1 }));
        Assert.Equal(1, HomeOverview.CompletedQuestions(session with { Mode = LearningMode.Match, MatchedIds = [Guid.NewGuid()] }));
    }

    [Theory]
    [InlineData(360, false, 1)]
    [InlineData(639, false, 1)]
    [InlineData(640, false, 2)]
    [InlineData(1099, false, 2)]
    [InlineData(1100, true, 2)]
    [InlineData(1920, true, 2)]
    public void DashboardUsesBoundedRowsAndAdaptiveNavigation(double width, bool sidebar, int columns)
    {
        Assert.Equal(sidebar, LayoutMetrics.UseSidebar(width));
        Assert.Equal(columns, LayoutMetrics.CompactColumns(width));
    }

    [Theory]
    [InlineData("HomePage.xaml")]
    [InlineData("MainPage.xaml")]
    [InlineData("AppShell.xaml")]
    public void DashboardResourcesExistInBothLanguages(string file)
    {
        var source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Design", file));
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2009/xaml";
        var theme = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Design", "StudyTheme.xaml"));
        var keys = theme.Descendants().Attributes(xaml + "Key").Select(attribute => attribute.Value).ToHashSet();
        foreach (Match match in Regex.Matches(source, @"\{StaticResource ([\w.]+)\}"))
            Assert.Contains(match.Groups[1].Value, keys);
        foreach (var language in new[] { "Strings.resx", "Strings.vi.resx" })
        {
            var resources = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Design", language)).Descendants("data").Select(element => element.Attribute("name")!.Value).ToArray();
            Assert.Equal(resources.Length, resources.Distinct().Count());
            foreach (Match match in Regex.Matches(source, @"\{DynamicResource L10n\.(\w+)\}"))
                Assert.Contains(match.Groups[1].Value, resources);
        }
    }

    private static LearningResult Result(VocabularyDeck deck, DateTimeOffset finished) =>
        new(Guid.NewGuid(), deck.Id, deck.Name, LearningMode.Flashcards, LearningDirection.EnglishToVietnamese, 1, 1, 0, 10, finished, []);

    private static LearningSession Session(VocabularyDeck deck, DateTimeOffset started) =>
        new(Guid.NewGuid(), deck.Id, deck.Name, LearningMode.Flashcards, LearningDirection.EnglishToVietnamese,
            [new StudyQuestion(Guid.NewGuid(), "cat", "mèo", ["mèo"], [])], [], [], [], 0, 0, "", false, started, null);
}
