using System.Globalization;
using System.Text;
using MauiApp1.Models;

namespace MauiApp1.Services;

public static class VocabularySearch
{
    public static string Normalize(string text)
    {
        var normalized = VocabularyRules.Normalize(text).Normalize(NormalizationForm.FormD);
        return string.Concat(normalized.Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark))
            .Replace('\u0110', 'D').Normalize(NormalizationForm.FormC);
    }

    public static bool MatchesFilter(VocabularyCard card, int filterIndex) =>
        filterIndex == (int)LearningFilter.All || card.IsStarred == (filterIndex == (int)LearningFilter.Starred);
}
