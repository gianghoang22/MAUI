using System.Collections;
using System.Globalization;
using System.Resources;

namespace MauiApp1.Localization;

public sealed class LocalizationService
{
    private readonly ResourceManager resources = new("MauiApp1.Localization.Strings", typeof(LocalizationService).Assembly);
    private readonly WeakEventManager languageEvents = new();

    public string LanguageCode { get; private set; } = "vi";
    public CultureInfo Culture => CultureInfo.GetCultureInfo(LanguageCode == "en" ? "en-US" : "vi-VN");

    public event EventHandler LanguageChanged
    {
        add => languageEvents.AddEventHandler(value);
        remove => languageEvents.RemoveEventHandler(value);
    }

    public string this[string key] => resources.GetString(key, Culture) ?? $"[{key}]";

    public string Format(string key, params object[] arguments) => string.Format(Culture, this[key], arguments);

    public void SetLanguage(string languageCode)
    {
        if (languageCode is not ("vi" or "en"))
            throw new ArgumentOutOfRangeException(nameof(languageCode));
        if (LanguageCode == languageCode)
            return;
        LanguageCode = languageCode;
        languageEvents.HandleEvent(this, EventArgs.Empty, nameof(LanguageChanged));
    }

    public IEnumerable<KeyValuePair<string, string>> GetResources()
    {
        var neutral = resources.GetResourceSet(CultureInfo.InvariantCulture, true, true)
            ?? throw new MissingManifestResourceException("VocabMate localization resources are missing.");
        foreach (DictionaryEntry entry in neutral)
        {
            var key = (string)entry.Key;
            yield return new KeyValuePair<string, string>(key, this[key]);
        }
    }
}
