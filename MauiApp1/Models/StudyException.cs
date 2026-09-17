namespace MauiApp1.Models;

public sealed class StudyException(string resourceKey, Exception? innerException = null)
    : Exception(resourceKey, innerException)
{
    public string ResourceKey { get; } = resourceKey;
}
