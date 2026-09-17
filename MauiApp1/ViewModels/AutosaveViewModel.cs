using MauiApp1.Localization;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public abstract class AutosaveViewModel(IUserInteraction interaction, LocalizationService localization)
    : ViewModelBase(interaction, localization), IEditorCheckpoint
{
    private readonly SemaphoreSlim saveGate = new(1, 1);
    private CancellationTokenSource? debounce;
    private int revision;
    private bool dirty;
    private string? saveError;
    protected bool AutosaveEnabled { get; set; }
    protected virtual string SavedHintKey => "VAutosaveHint";
    public string SaveStatus => Localization[saveError ?? (dirty ? "VSavingDraft" : SavedHintKey)];

    protected void MarkDirty()
    {
        if (!AutosaveEnabled) return;
        dirty = true;
        revision++;
        saveError = null;
        OnPropertyChanged(nameof(SaveStatus));
        debounce?.Cancel();
        debounce?.Dispose();
        debounce = new CancellationTokenSource();
        _ = DebounceAsync(debounce.Token);
    }

    private async Task DebounceAsync(CancellationToken cancellation)
    {
        try
        {
            await Task.Delay(400, cancellation);
            await FlushSafelyAsync();
        }
        catch (OperationCanceledException) { }
    }

    protected abstract Task PersistDraftAsync();

    protected void ResetAutosave()
    {
        debounce?.Cancel();
        dirty = false;
        saveError = null;
        revision++;
        OnPropertyChanged(nameof(SaveStatus));
    }

    public async Task FlushAsync()
    {
        await saveGate.WaitAsync();
        try
        {
            if (!dirty) return;
            var savingRevision = revision;
            await PersistDraftAsync();
            if (savingRevision == revision) dirty = false;
            saveError = null;
            OnPropertyChanged(nameof(SaveStatus));
        }
        finally { saveGate.Release(); }
    }

    public async Task FlushSafelyAsync()
    {
        try { await FlushAsync(); }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            saveError = exception is StudyException studyException ? studyException.ResourceKey : "VDraftSaveFailed";
            OnPropertyChanged(nameof(SaveStatus));
        }
    }
}
