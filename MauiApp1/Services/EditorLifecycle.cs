namespace MauiApp1.Services;

public interface IRefreshable
{
    Task RefreshAsync();
}

public interface IEditorCheckpoint
{
    Task FlushSafelyAsync();
}

public sealed class EditorLifecycle
{
    public IEditorCheckpoint? Active { get; private set; }
    public void Enter(IEditorCheckpoint editor) => Active = editor;
    public Task FlushAsync() => Active?.FlushSafelyAsync() ?? Task.CompletedTask;

    public async Task LeaveAsync(IEditorCheckpoint editor)
    {
        await editor.FlushSafelyAsync();
        if (ReferenceEquals(Active, editor)) Active = null;
    }
}
