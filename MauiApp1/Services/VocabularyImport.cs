using MauiApp1.Models;

namespace MauiApp1.Services;

public static class VocabularyImport
{
    public static WorkbookPreview Prepare(VocabularyData data, Guid deckId, WorkbookPreview workbook)
    {
        var deck = data.Decks.FirstOrDefault(item => item.Id == deckId) ?? throw new StudyException("VNotFound");
        var first = VocabularyLanguages.Canonicalize(workbook.FirstLanguage);
        var second = VocabularyLanguages.Canonicalize(workbook.SecondLanguage);
        // Bộ trống nhận ngôn ngữ từ file; bộ có thẻ/nháp phải giữ đúng cặp ngôn ngữ cũ.
        if (!data.Cards.Any(card => card.DeckId == deckId) && !data.Drafts.Any(draft => draft.DeckId == deckId))
            return workbook with { FirstLanguage = first, SecondLanguage = second };
        if (VocabularyLanguages.Same(first, deck.FirstLanguage) && VocabularyLanguages.Same(second, deck.SecondLanguage))
            return workbook with { FirstLanguage = deck.FirstLanguage, SecondLanguage = deck.SecondLanguage };
        // Cùng cặp ngôn ngữ nhưng ngược cột thì tự đổi hai mặt trước khi nhập.
        if (VocabularyLanguages.Same(first, deck.SecondLanguage) && VocabularyLanguages.Same(second, deck.FirstLanguage))
            return workbook with
            {
                FirstLanguage = deck.FirstLanguage, SecondLanguage = deck.SecondLanguage,
                Rows = workbook.Rows.Select(row => row with { Vietnamese = row.English, English = row.Vietnamese }).ToList()
            };
        throw new StudyException("VLanguageMismatch");
    }
}
