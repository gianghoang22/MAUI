using Android.App;
using Android.Runtime;

namespace MauiBasics;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership)
    : MauiApplication(handle, ownership)
{
    // Android gọi vào đây để khởi tạo phần ứng dụng MAUI dùng chung.
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
