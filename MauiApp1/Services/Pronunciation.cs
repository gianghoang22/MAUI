using MauiApp1.Models;

namespace MauiApp1.Services;

public interface IPronunciation
{
    Task SpeakAsync(string text, string language);
}

public sealed class Pronunciation : IPronunciation
{
    public async Task SpeakAsync(string text, string language)
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        var canonical = VocabularyLanguages.Canonicalize(language);
        var locale = locales.FirstOrDefault(item => item.Language.Equals(canonical, StringComparison.OrdinalIgnoreCase))
            ?? locales.FirstOrDefault(item => item.Language.StartsWith(canonical + "-", StringComparison.OrdinalIgnoreCase) ||
                item.Language.StartsWith(canonical + "_", StringComparison.OrdinalIgnoreCase))
            ?? throw new StudyException("VVoiceUnavailable");
        await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions { Locale = locale });
    }
}
