namespace MauiApp1.Services;

public interface IRefreshable
{
    Task RefreshAsync();
}

public interface IEditorCheckpoint
{
    Task FlushSafelyAsync();
}

// Theo dõi màn đang sửa/học để App có thể yêu cầu lưu khi xuống nền.
public sealed class EditorLifecycle
{
    public IEditorCheckpoint? Active { get; private set; }
    public void Enter(IEditorCheckpoint editor) => Active = editor;
    public Task FlushAsync() => Active?.FlushSafelyAsync() ?? Task.CompletedTask;

    public async Task LeaveAsync(IEditorCheckpoint editor)
    {
        await editor.FlushSafelyAsync();
        // Lúc chờ lưu, màn mới có thể đã mở; không xóa đăng ký của màn mới đó.
        if (ReferenceEquals(Active, editor)) Active = null;
    }
}
