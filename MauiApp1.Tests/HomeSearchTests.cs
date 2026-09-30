using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using Xunit;

namespace MauiApp1.Tests;

public sealed class HomeSearchTests
{
    [Fact]
    public async Task TypingFiltersImmediatelyWithoutNavigation()
    {
        using var fixture = new SearchFixture();
        await fixture.SeedAsync("English", "Japanese");
        var viewModel = await fixture.CreateHomeAsync();

        viewModel.Search = "jap";

        Assert.Equal("Japanese", Assert.Single(viewModel.Classes).Name);
        Assert.Empty(fixture.Interaction.Routes);
    }

    [Fact]
    public async Task SearchIgnoresCaseAccentsAndExtraWhitespace()
    {
        using var fixture = new SearchFixture();
        await fixture.SeedAsync("\u0110\u1ed9ng v\u1eadt", "English");
        var viewModel = await fixture.CreateHomeAsync();

        viewModel.Search = "  DONG   VAT  ";

        Assert.Equal("\u0110\u1ed9ng v\u1eadt", Assert.Single(viewModel.Classes).Name);
    }

    [Fact]
    public async Task SearchIncludesClassesOutsideDashboardLimit()
    {
        using var fixture = new SearchFixture();
        await fixture.SeedAsync(Enumerable.Range(1, 9).Select(index => $"Class {index}").ToArray());
        var viewModel = await fixture.CreateHomeAsync();
        Assert.Equal(6, viewModel.Classes.Count);

        viewModel.Search = "Class 9";
        Assert.Equal("Class 9", Assert.Single(viewModel.Classes).Name);
        viewModel.Search = "Class";
        Assert.Equal(9, viewModel.Classes.Count);
    }

    [Fact]
    public async Task RepeatedEnterFiltersWithoutNavigatingOrClearingQuery()
    {
        using var fixture = new SearchFixture();
        await fixture.SeedAsync("English", "Japanese");
        var viewModel = await fixture.CreateHomeAsync();
        viewModel.Search = "English";

        for (var attempt = 0; attempt < 5; attempt++) viewModel.SearchCommand.Execute(null);

        Assert.Empty(fixture.Interaction.Routes);
        Assert.Equal("English", viewModel.Search);
        Assert.Equal("English", Assert.Single(viewModel.Classes).Name);
    }

    [Fact]
    public async Task NoMatchShowsSearchEmptyStateAndNotifiesBindings()
    {
        using var fixture = new SearchFixture();
        await fixture.SeedAsync("English");
        var viewModel = await fixture.CreateHomeAsync();
        var changed = new List<string?>();
        viewModel.PropertyChanged += (_, arguments) => changed.Add(arguments.PropertyName);

        viewModel.Search = "does not exist";

        Assert.Empty(viewModel.Classes);
        Assert.True(viewModel.IsEmpty);
        Assert.False(viewModel.HasClasses);
        Assert.Equal(fixture.Localization["VNoClassesFound"], viewModel.EmptyHeading);
        Assert.Contains(nameof(viewModel.IsEmpty), changed);
        Assert.Contains(nameof(viewModel.HasClasses), changed);
        Assert.Contains(nameof(viewModel.EmptyHeading), changed);
    }

    [Fact]
    public async Task ClearingSearchRestoresBoundedDashboard()
    {
        using var fixture = new SearchFixture();
        await fixture.SeedAsync(Enumerable.Range(1, 9).Select(index => $"Class {index}").ToArray());
        var viewModel = await fixture.CreateHomeAsync();
        viewModel.Search = "missing";

        viewModel.Search = "   ";

        Assert.False(viewModel.HasSearch);
        Assert.False(viewModel.IsEmpty);
        Assert.Equal(6, viewModel.Classes.Count);
        Assert.Equal(fixture.Localization["VHomeEmptyHeading"], viewModel.EmptyHeading);
    }

    [Fact]
    public async Task RefreshPreservesSearchAndIncludesNewMatches()
    {
        using var fixture = new SearchFixture();
        await fixture.SeedAsync("English", "Japanese");
        var viewModel = await fixture.CreateHomeAsync();
        viewModel.Search = "English";
        await fixture.SeedAsync("English Advanced");

        await viewModel.RefreshAsync();

        Assert.Equal("English", viewModel.Search);
        Assert.Equal(2, viewModel.Classes.Count);
        Assert.All(viewModel.Classes, classroom => Assert.Contains("English", classroom.Name));
    }

    [Fact]
    public async Task LibraryAlsoFiltersWhileTypingWithoutNavigation()
    {
        using var fixture = new SearchFixture();
        await fixture.SeedAsync("English", "Japanese");
        var viewModel = new LibraryViewModel(fixture.Repository, fixture.Interaction, fixture.Localization);
        await viewModel.RefreshAsync();

        viewModel.Search = "jap";

        Assert.Equal("Japanese", Assert.Single(viewModel.Classes).Name);
        Assert.Empty(fixture.Interaction.Routes);
        viewModel.Search = "missing";
        Assert.True(viewModel.IsSearchEmpty);
        viewModel.Search = "";
        Assert.Equal(2, viewModel.Classes.Count);
    }

    private sealed class SearchFixture : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "vocabmate-search-tests-" + Guid.NewGuid().ToString("N"));
        public JsonVocabularyRepository Repository { get; }
        public RecordingInteraction Interaction { get; } = new();
        public LocalizationService Localization { get; } = new();

        public SearchFixture() => Repository = new JsonVocabularyRepository(directory);

        public async Task SeedAsync(params string[] names)
        {
            foreach (var name in names) await Repository.SaveClassAsync(new VocabularyClass(Guid.NewGuid(), name));
        }

        public async Task<HomeViewModel> CreateHomeAsync()
        {
            var viewModel = new HomeViewModel(Repository, Interaction, Localization);
            await viewModel.RefreshAsync();
            return viewModel;
        }

        public void Dispose()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private sealed class RecordingInteraction : IUserInteraction
    {
        public List<string> Routes { get; } = [];
        public Task NavigateAsync(string route) { Routes.Add(route); return Task.CompletedTask; }
        public Task AlertAsync(string title, string message) => throw new InvalidOperationException($"Unexpected alert: {title}: {message}");
        public Task<bool> ConfirmAsync(string title, string message, string acceptKey = "Delete") => throw new NotSupportedException();
        public Task<string?> ActionSheetAsync(string title, string cancel, string? destruction, params string[] buttons) => throw new NotSupportedException();
        public Task<string?> PromptAsync(string title, string message, string? initialValue = "") => throw new NotSupportedException();
    }
}
