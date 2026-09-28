using System.Globalization;
using MauiApp1.Models;

namespace MauiApp1.Services;

public static class VocabularyLanguages
{
    private static readonly (string Code, string Vietnamese, string English, string Native)[] Known =
    [
        ("vi", "Tiếng Việt", "Vietnamese", "Việt"),
        ("en", "Tiếng Anh", "English", "Anh"),
        ("ja", "Tiếng Nhật", "Japanese", "日本語"),
        ("ko", "Tiếng Hàn", "Korean", "한국어"),
        ("zh", "Tiếng Trung", "Chinese", "中文"),
        ("fr", "Tiếng Pháp", "French", "Français"),
        ("de", "Tiếng Đức", "German", "Deutsch"),
        ("es", "Tiếng Tây Ban Nha", "Spanish", "Español"),
        ("th", "Tiếng Thái", "Thai", "ไทย"),
        ("ru", "Tiếng Nga", "Russian", "Русский"),
        ("ar", "Tiếng Ả Rập", "Arabic", "العربية")
    ];

    public static bool IsValid(string? language) => !string.IsNullOrWhiteSpace(language) && language.Trim().Length <= 80;

    public static string Canonicalize(string language)
    {
        if (!IsValid(language)) throw new StudyException("VLanguageRequired");
        var key = VocabularySearch.Normalize(language);
        foreach (var item in Known)
            if (new[] { item.Code, item.Vietnamese, item.Vietnamese.Replace("Tiếng ", ""), item.English, item.Native }.Any(alias => VocabularySearch.Normalize(alias) == key))
                return item.Code;
        var culture = CultureInfo.GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures)
            .FirstOrDefault(item => item.Name.Length > 0 && new[] { item.Name, item.EnglishName, item.NativeName }
                .Any(alias => VocabularySearch.Normalize(alias) == key));
        return culture?.Name ?? language.Trim();
    }

    public static bool Same(string first, string second) =>
        VocabularyRules.Normalize(Canonicalize(first)) == VocabularyRules.Normalize(Canonicalize(second));

    public static string DisplayName(string language, string uiLanguage)
    {
        if (!IsValid(language)) return uiLanguage == "vi" ? "Chưa chọn ngôn ngữ" : "Language not set";
        var canonical = Canonicalize(language);
        var known = Known.FirstOrDefault(item => item.Code == canonical);
        if (known.Code is not null) return uiLanguage == "vi" ? known.Vietnamese : known.English;
        return CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(item => item.Name == canonical)?.NativeName ?? canonical;
    }

    public static string Direction(string first, string second, LearningDirection direction, string uiLanguage) =>
        direction == LearningDirection.EnglishToVietnamese
            ? $"{DisplayName(second, uiLanguage)} → {DisplayName(first, uiLanguage)}"
            : $"{DisplayName(first, uiLanguage)} → {DisplayName(second, uiLanguage)}";
}
