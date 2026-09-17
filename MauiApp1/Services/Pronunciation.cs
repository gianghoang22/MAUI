using MauiApp1.Models;

namespace MauiApp1.Services;

public interface IPronunciation
{
    Task SpeakAsync(string english);
}

public sealed class Pronunciation : IPronunciation
{
    public async Task SpeakAsync(string english)
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        var locale = locales.FirstOrDefault(item => item.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            ?? throw new StudyException("VVoiceUnavailable");
        await TextToSpeech.Default.SpeakAsync(english, new SpeechOptions { Locale = locale });
    }
}
