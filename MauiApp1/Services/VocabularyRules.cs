using System.Text;
using System.Text.RegularExpressions;
using MauiApp1.Models;

namespace MauiApp1.Services;

public static class VocabularyRules
{
    public const int MaxTermLength = 200;
    public static string Normalize(string text) => Regex.Replace(text.Trim().Normalize(NormalizationForm.FormC), @"\s+", " ").ToUpperInvariant();
    public static string PairKey(string vietnamese, string english) => Normalize(vietnamese) + "\u001f" + Normalize(english);
    public static bool IsValidTerm(string? text) => !string.IsNullOrWhiteSpace(text) && text.Trim().Length <= MaxTermLength;

    public static void RequireName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 80)
            throw new StudyException("VNameInvalid");
    }

    public static void RequireCard(VocabularyCard card)
    {
        if (!IsValidTerm(card.Vietnamese) || !IsValidTerm(card.English))
            throw new StudyException("VTermInvalid");
    }
}
