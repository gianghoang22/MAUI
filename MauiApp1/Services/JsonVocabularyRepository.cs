using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1.Services;

public sealed class JsonVocabularyRepository(string directory) : IVocabularyRepository
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly JsonSerializerOptions options = new() { WriteIndented = true };
    private readonly string path = Path.Combine(directory, "vocabmate.json");

    public async Task<VocabularyData> ReadAsync()
    {
        await gate.WaitAsync();
        try { return await LoadAsync(); }
        finally { gate.Release(); }
    }

    public Task SaveClassAsync(VocabularyClass classroom) => UpdateAsync(data =>
    {
        VocabularyRules.RequireName(classroom.Name);
        if (data.Classes.Any(existing => existing.Id != classroom.Id && VocabularyRules.Normalize(existing.Name) == VocabularyRules.Normalize(classroom.Name)))
            throw new StudyException("VDuplicateName");
        data.Classes.RemoveAll(existing => existing.Id == classroom.Id);
        data.Classes.Add(classroom with { Name = classroom.Name.Trim() });
    });

    public Task DeleteClassAsync(Guid classId) => UpdateAsync(data =>
    {
        var deckIds = data.Decks.Where(deck => deck.ClassId == classId).Select(deck => deck.Id).ToHashSet();
        RemoveDecks(data, deckIds);
        data.Classes.RemoveAll(classroom => classroom.Id == classId);
    });

    public Task SaveDeckAsync(VocabularyDeck deck) => UpdateAsync(data =>
    {
        VocabularyRules.RequireName(deck.Name);
        if (!data.Classes.Any(classroom => classroom.Id == deck.ClassId)) throw new StudyException("VNotFound");
        if (deck.Description.Length > 500) throw new StudyException("VDescriptionInvalid");
        if (data.Decks.Any(existing => existing.Id != deck.Id && existing.ClassId == deck.ClassId && VocabularyRules.Normalize(existing.Name) == VocabularyRules.Normalize(deck.Name)))
            throw new StudyException("VDuplicateName");
        data.Decks.RemoveAll(existing => existing.Id == deck.Id);
        data.Decks.Add(deck with { Name = deck.Name.Trim(), Description = deck.Description.Trim() });
    });

    public Task DeleteDeckAsync(Guid deckId) => UpdateAsync(data => RemoveDecks(data, [deckId]));

    public Task SaveCardAsync(VocabularyCard card) => UpdateAsync(data =>
    {
        VocabularyRules.RequireCard(card);
        RequireDeck(data, card.DeckId);
        if (data.Cards.Any(existing => existing.Id != card.Id && existing.DeckId == card.DeckId &&
            VocabularyRules.PairKey(existing.Vietnamese, existing.English) == VocabularyRules.PairKey(card.Vietnamese, card.English)))
            throw new StudyException("VDuplicateCard");
        data.Cards.RemoveAll(existing => existing.Id == card.Id);
        data.Cards.Add(card with { Vietnamese = card.Vietnamese.Trim(), English = card.English.Trim() });
        data.Drafts.RemoveAll(draft => draft.CardId == card.Id);
    });

    public Task DeleteCardAsync(Guid cardId) => UpdateAsync(data =>
    {
        data.Cards.RemoveAll(card => card.Id == cardId);
        data.Drafts.RemoveAll(draft => draft.CardId == cardId);
    });

    public async Task<int> ImportAsync(Guid deckId, IReadOnlyList<VocabularyCard> cards)
    {
        var added = 0;
        await UpdateAsync(data =>
        {
            RequireDeck(data, deckId);
            if (cards.Count > 2000) throw new StudyException("VImportLimit");
            var existing = data.Cards.Where(card => card.DeckId == deckId)
                .Select(card => VocabularyRules.PairKey(card.Vietnamese, card.English)).ToHashSet();
            foreach (var card in cards)
            {
                VocabularyRules.RequireCard(card);
                if (card.DeckId != deckId) throw new StudyException("VImportInvalid");
                if (!existing.Add(VocabularyRules.PairKey(card.Vietnamese, card.English))) continue;
                data.Cards.Add(card with { Vietnamese = card.Vietnamese.Trim(), English = card.English.Trim() });
                added++;
            }
        });
        return added;
    }

    public Task SaveDraftAsync(CardDraft draft) => UpdateAsync(data =>
    {
        RequireDeck(data, draft.DeckId);
        if (draft.Vietnamese.Length > 200 || draft.English.Length > 200) throw new StudyException("VTermInvalid");
        if (!draft.IsNew && !data.Cards.Any(card => card.Id == draft.CardId && card.DeckId == draft.DeckId)) throw new StudyException("VNotFound");
        data.Drafts.RemoveAll(existing => existing.CardId == draft.CardId || (draft.IsNew && existing.IsNew && existing.DeckId == draft.DeckId));
        data.Drafts.Add(draft);
    });

    public Task DeleteDraftAsync(Guid cardId) => UpdateAsync(data => data.Drafts.RemoveAll(draft => draft.CardId == cardId));

    public Task StartSessionAsync(LearningSession session) => UpdateAsync(data =>
    {
        RequireDeck(data, session.DeckId);
        data.Session = session;
    });

    public Task SaveSessionAsync(LearningSession session) => UpdateAsync(data =>
    {
        RequireDeck(data, session.DeckId);
        if (data.Session is not { } current || current.Id != session.Id || current.Index > session.Index ||
            current.Attempts.Count > session.Attempts.Count || current.MatchedIds.Count > session.MatchedIds.Count ||
            current.DeckId != session.DeckId || current.MatchMistakes > session.MatchMistakes ||
            current.MatchedIds.Except(session.MatchedIds).Any() ||
            !current.Attempts.SequenceEqual(session.Attempts.Take(current.Attempts.Count)) ||
            (current.Index == session.Index && current.Revealed && !session.Revealed)) throw new StudyException("VSessionChanged");
        data.Session = session;
        if (!session.IsComplete || data.Results.Any(result => result.Id == session.Id)) return;
        var wrongIds = session.Attempts.Where(attempt => !attempt.Correct).Select(attempt => attempt.CardId).ToHashSet();
        data.Results.Add(new LearningResult(session.Id, session.DeckId, session.DeckName, session.Mode, session.Direction,
            session.Questions.Count, session.Correct, session.MatchMistakes,
            (int)Math.Clamp((session.FinishedAt!.Value - session.StartedAt).TotalSeconds, 0, int.MaxValue), session.FinishedAt.Value,
            session.Questions.Where(question => wrongIds.Contains(question.CardId)).ToList()));
        if (data.Results.Count > 100) data.Results.RemoveRange(0, data.Results.Count - 100);
    });

    private static void RequireDeck(VocabularyData data, Guid deckId)
    {
        if (!data.Decks.Any(deck => deck.Id == deckId)) throw new StudyException("VNotFound");
    }

    private static void RemoveDecks(VocabularyData data, HashSet<Guid> deckIds)
    {
        data.Decks.RemoveAll(deck => deckIds.Contains(deck.Id));
        data.Cards.RemoveAll(card => deckIds.Contains(card.DeckId));
        data.Drafts.RemoveAll(draft => deckIds.Contains(draft.DeckId));
        data.Results.RemoveAll(result => deckIds.Contains(result.DeckId));
        if (data.Session is not null && deckIds.Contains(data.Session.DeckId)) data.Session = null;
    }

    private async Task UpdateAsync(Action<VocabularyData> change)
    {
        await gate.WaitAsync();
        string? temporary = null;
        try
        {
            var data = await LoadAsync();
            change(data);
            Validate(data);
            Directory.CreateDirectory(directory);
            temporary = Path.Combine(directory, $"vocabmate-{Guid.NewGuid():N}.tmp");
            await using (var stream = File.Create(temporary))
            {
                await JsonSerializer.SerializeAsync(stream, data, options);
                await stream.FlushAsync();
                stream.Flush(true);
            }
            File.Move(temporary, path, true);
        }
        finally
        {
            gate.Release();
            if (temporary is not null && File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private async Task<VocabularyData> LoadAsync()
    {
        if (!File.Exists(path)) return new VocabularyData();
        try
        {
            await using var stream = File.OpenRead(path);
            var data = await JsonSerializer.DeserializeAsync<VocabularyData>(stream, options);
            Validate(data);
            return data!;
        }
        catch (JsonException exception) { throw new StudyException("VDataInvalid", exception); }
    }

    private static void Validate(VocabularyData? data)
    {
        if (data is null || data.SchemaVersion != 1 || data.Classes is null || data.Decks is null || data.Cards is null || data.Drafts is null || data.Results is null)
            throw new StudyException("VDataInvalid");
        if (data.Classes.Any(item => item is null || item.Id == Guid.Empty || string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 80) ||
            data.Decks.Any(item => item is null || item.Id == Guid.Empty || string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 80 || item.Description is null || item.Description.Length > 500 || !data.Classes.Any(parent => parent.Id == item.ClassId)) ||
            data.Cards.Any(item => item is null || item.Id == Guid.Empty || !VocabularyRules.IsValidTerm(item.Vietnamese) || !VocabularyRules.IsValidTerm(item.English) || !data.Decks.Any(parent => parent.Id == item.DeckId)) ||
            data.Drafts.Any(item => item is null || item.CardId == Guid.Empty || item.Vietnamese is null || item.English is null || item.Vietnamese.Length > 200 || item.English.Length > 200 || !data.Decks.Any(parent => parent.Id == item.DeckId)) ||
            data.Results.Any(item => item is null || item.Id == Guid.Empty || item.DeckName is null || item.WrongQuestions is null || item.WrongQuestions.Any(InvalidQuestion) ||
                !Enum.IsDefined(item.Mode) || !Enum.IsDefined(item.Direction) || item.Seconds < 0 || item.Mistakes < 0 ||
                item.Total < 1 || item.Correct < 0 || item.Correct > item.Total || !data.Decks.Any(parent => parent.Id == item.DeckId)) ||
            data.Classes.Select(item => item.Id).Distinct().Count() != data.Classes.Count ||
            data.Decks.Select(item => item.Id).Distinct().Count() != data.Decks.Count ||
            data.Cards.Select(item => item.Id).Distinct().Count() != data.Cards.Count ||
            data.Classes.Select(item => VocabularyRules.Normalize(item.Name)).Distinct().Count() != data.Classes.Count ||
            data.Decks.Select(item => (item.ClassId, VocabularyRules.Normalize(item.Name))).Distinct().Count() != data.Decks.Count ||
            data.Cards.Select(item => (item.DeckId, VocabularyRules.PairKey(item.Vietnamese, item.English))).Distinct().Count() != data.Cards.Count ||
            data.Drafts.Select(item => item.CardId).Distinct().Count() != data.Drafts.Count ||
            data.Results.Select(item => item.Id).Distinct().Count() != data.Results.Count)
            throw new StudyException("VDataInvalid");
        if (data.Session is not null)
        {
            var session = data.Session;
            if (!data.Decks.Any(deck => deck.Id == session.DeckId) || session.Id == Guid.Empty || session.DeckName is null ||
                session.Questions is null || session.Questions.Count == 0 || session.Attempts is null || session.MatchedIds is null || session.MatchOrder is null ||
                session.Input is null || session.Input.Length > 200 || !Enum.IsDefined(session.Mode) || !Enum.IsDefined(session.Direction) ||
                session.Index < 0 || session.Index > session.Questions.Count || session.Attempts.Count > session.Questions.Count ||
                session.Questions.Any(InvalidQuestion) ||
                session.Attempts.Any(attempt => attempt is null || attempt.Submitted is null || attempt.Submitted.Length > 200 || !session.Questions.Any(question => question.CardId == attempt.CardId)) ||
                session.MatchedIds.Any(cardId => !session.Questions.Any(question => question.CardId == cardId)))
                throw new StudyException("VDataInvalid");
            var questionIds = session.Questions.Select(question => question.CardId).ToHashSet();
            if (questionIds.Count != session.Questions.Count || session.MatchOrder.Count != questionIds.Count ||
                !questionIds.SetEquals(session.MatchOrder) || session.MatchedIds.Distinct().Count() != session.MatchedIds.Count || session.MatchMistakes < 0 ||
                session.StartedAt == default || session.FinishedAt < session.StartedAt ||
                (session.Mode == LearningMode.Match && (session.Index != 0 || session.Attempts.Count != 0 || session.IsComplete != (session.MatchedIds.Count == questionIds.Count))) ||
                (session.Mode != LearningMode.Match && (session.MatchedIds.Count != 0 || session.Attempts.Count < session.Index || session.Attempts.Count > session.Index + 1 ||
                    session.IsComplete != (session.Index == session.Questions.Count) ||
                    session.Attempts.Where((attempt, index) => attempt.CardId != session.Questions[index].CardId).Any())) ||
                (session.Mode == LearningMode.MultipleChoice && session.Questions.Any(question => question.Choices.Count != 4 || question.Choices.DistinctBy(VocabularyRules.Normalize).Count() != 4)))
                throw new StudyException("VDataInvalid");
        }
    }

    private static bool InvalidQuestion(StudyQuestion question) => question is null || question.CardId == Guid.Empty ||
        !VocabularyRules.IsValidTerm(question.Prompt) || !VocabularyRules.IsValidTerm(question.Answer) || question.AcceptedAnswers is null ||
        question.AcceptedAnswers.Count == 0 || question.AcceptedAnswers.Any(answer => !VocabularyRules.IsValidTerm(answer)) ||
        !question.AcceptedAnswers.Any(answer => VocabularyRules.Normalize(answer) == VocabularyRules.Normalize(question.Answer)) ||
        question.Choices is null || question.Choices.Any(choice => !VocabularyRules.IsValidTerm(choice));
}
