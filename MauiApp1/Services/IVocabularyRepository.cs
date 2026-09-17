using MauiApp1.Models;

namespace MauiApp1.Services;

public interface IVocabularyRepository
{
    Task<VocabularyData> ReadAsync();
    Task SaveClassAsync(VocabularyClass classroom);
    Task DeleteClassAsync(Guid classId);
    Task SaveDeckAsync(VocabularyDeck deck);
    Task DeleteDeckAsync(Guid deckId);
    Task SaveCardAsync(VocabularyCard card);
    Task SetStarredAsync(Guid cardId, bool starred);
    Task DeleteCardAsync(Guid cardId);
    Task<int> ImportAsync(Guid deckId, IReadOnlyList<VocabularyCard> cards);
    Task SaveDraftAsync(CardDraft draft);
    Task DeleteDraftAsync(Guid cardId);
    Task SaveSessionAsync(LearningSession session);
    Task StartSessionAsync(LearningSession session);
}
