# .NET MAUI 10 — 80/20 Overview

> Mục tiêu: nắm **20% kiến thức cốt lõi** để hiểu, build, debug và phát triển phần lớn ứng dụng .NET MAUI 10 thực tế.  
> Học **Windows + Android trước**, rồi mở rộng sang iOS/macOS.

## 1. .NET MAUI là gì?

**.NET MAUI (Multi-platform App UI)** là framework .NET cross-platform xây ứng dụng native mobile/desktop từ một codebase chung.

Nền tảng: Android; iOS; macOS qua Mac Catalyst; Windows qua WinUI 3; Tizen được hỗ trợ bổ sung bởi Samsung.

MAUI kế thừa, mở rộng Xamarin.Forms; không phải "viết một lần, mọi thứ giống hệt nhau", mà là:

> **Shared code + shared UI + platform-specific code khi cần.**

MAUI nối code ứng dụng với native platform API: `Your MAUI App → {Shared .NET code; XAML / UI} → .NET MAUI API → {Android native; Windows WinUI 3; iOS/macOS native APIs}`

MAUI cung cấp API/UI abstraction chung, vẫn cho phép gọi trực tiếp native API khi cần.

# 2. 80/20 Mental Model

Mô hình cốt lõi: `.NET MAUI`:
- `UI → {XAML; Controls; Layouts; Styles / Resources; Data Binding}`
- `Application → {Pages; Navigation; DI; MVVM; Services}`
- `Platform → {Android; Windows / WinUI 3; iOS; Mac Catalyst}`
- `Native integration → {Handlers; Platform APIs; Conditional compilation}`
- `Build / Deployment → {Multi-targeting; Android APK/AAB; Windows MSIX; Signing / Store}`

**7 nhóm thường chiếm 80% công việc MAUI:**

1. XAML/UI
2. Layout
3. Data Binding + MVVM
4. Navigation
5. Dependency Injection + Services
6. Platform-specific code
7. Build/debug/deployment

# 3. Single Project — khái niệm quan trọng nhất

MAUI sử dụng **Single Project**.

Thay vì: `Android project`; `iOS project`; `Windows project`; `Mac project`

ta có: `MyMauiApp → {Platforms → {Android; Windows; iOS; MacCatalyst}; Resources; App.xaml; MauiProgram.cs; MainPage.xaml; MyMauiApp.csproj}`

Một `.csproj` có thể multi-target:

```xml
<TargetFrameworks>
    net10.0-android;
    net10.0-windows10.0.19041.0
</TargetFrameworks>
```

Khi cần mở rộng:

```xml
<TargetFrameworks>
    net10.0-android;
    net10.0-ios;
    net10.0-maccatalyst;
    net10.0-windows10.0.19041.0
</TargetFrameworks>
```

### Mental model

`MyMauiApp.csproj`:
- `Android → Android APIs`
- `Windows → WinUI 3 APIs`
- `iOS → UIKit APIs`

**Không nên hiểu MAUI là "xóa hoàn toàn platform-specific code".**

Thực tế:

- Shared code → dùng chung tối đa.
- Platform code → dùng khi native behavior khác nhau.

# 4. Project Anatomy — phải đọc được project

Một project MAUI cơ bản: `MyMauiApp/`:
- `App.xaml`
- `App.xaml.cs`
- `AppShell.xaml`
- `AppShell.xaml.cs`
- `MainPage.xaml`
- `MainPage.xaml.cs`
- `MauiProgram.cs`
- `Platforms/ → {Android/; Windows/}`
- `Resources/ → {AppIcon/; Fonts/; Images/; Raw/; Splash/; Styles/}`
- `MyMauiApp.csproj`

### `MauiProgram.cs`

Đây là nơi cấu hình app:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();

    builder
        .UseMauiApp<App>();

    builder.Services.AddSingleton<MainPage>();
    builder.Services.AddSingleton<IUserService, UserService>();

    return builder.Build();
}
```

Cần hiểu: App bootstrap; Dependency Injection; Fonts; Handlers; Logging; Third-party libraries; Configuration.

# 5. XAML — UI layer

MAUI thường dùng XAML để mô tả UI.

```xml
<VerticalStackLayout Padding="24">

    <Label
        Text="Hello MAUI"
        FontSize="24" />

    <Button
        Text="Click me"
        Command="{Binding ClickCommand}" />

</VerticalStackLayout>
```

Cần học sâu:

### Controls

Ưu tiên: `Label`; `Button`; `Entry`; `Editor`; `Image`; `ImageButton`; `Switch`; `CheckBox`; `Picker`; `DatePicker`; `CollectionView`; `ScrollView`; `WebView`.

### Layouts

Ưu tiên: `Grid`; `VerticalStackLayout`; `HorizontalStackLayout`; `FlexLayout`; `ScrollView`.

Đặc biệt phải hiểu: `Grid → {RowDefinitions; ColumnDefinitions; Row; Column}`

và: `HorizontalOptions`; `VerticalOptions`; `Margin`; `Padding`; `WidthRequest`; `HeightRequest`

# 6. Binding — một trong những phần quan trọng nhất

Thay vì viết UI logic trực tiếp:

```csharp
label.Text = user.Name;
```

MAUI thường binding:

```xml
<Label Text="{Binding Name}" />
```

ViewModel:

```csharp
public partial class UserViewModel : ObservableObject
{
    [ObservableProperty]
    private string name;
}
```

`View — Binding → ViewModel → Application state → Service / API / Database`

Cần hiểu: `BindingContext`; OneWay binding; TwoWay binding; `Command`; `ObservableObject`; `ObservableCollection`; `INotifyPropertyChanged`; `x:DataType` / compiled binding.

# 7. MVVM — pattern nên học sớm

MVVM không bắt buộc nhưng rất quan trọng trong hệ sinh thái MAUI.

`View XAML → ViewModel state + commands → Service business/data access → API / DB / device`

```csharp
public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string username = string.Empty;

    [RelayCommand]
    private async Task LoginAsync()
    {
        // Call service
    }
}
```

XAML:

```xml
<Button
    Text="Login"
    Command="{Binding LoginCommand}" />
```

### CommunityToolkit.Mvvm

Nên học: `ObservableObject`; `[ObservableProperty]`; `[RelayCommand]`; `WeakReferenceMessenger`.

Trong .NET MAUI 10, `MessagingCenter` đã internal; code mới cần messaging nên ưu tiên `WeakReferenceMessenger`.

# 8. Navigation

Phân biệt: `Navigation → {Shell navigation; NavigationPage / lower-level navigation}`

Học Shell trước.

```xml
<Shell
    x:Class="MyMauiApp.AppShell">

    <ShellContent
        Title="Home"
        ContentTemplate="{DataTemplate local:MainPage}" />

</Shell>
```

Route:

```csharp
Routing.RegisterRoute(
    nameof(DetailPage),
    typeof(DetailPage));
```

Navigate:

```csharp
await Shell.Current.GoToAsync(nameof(DetailPage));
```

Cần hiểu: Route; Query parameters; Back navigation; Tab; Flyout; Navigation state; Lifecycle của page.

# 9. Dependency Injection

MAUI sử dụng Microsoft.Extensions.DependencyInjection.

```csharp
builder.Services.AddSingleton<ApiService>();
builder.Services.AddTransient<SettingsPage>();
builder.Services.AddSingleton<IStorageService, StorageService>();
```

```text
MauiProgram
    ├── Register Services
    ├── Register ViewModels
    └── Register Pages
             ▼
        Constructor Injection
```

```csharp
public class MainViewModel
{
    private readonly ApiService api;

    public MainViewModel(ApiService api)
    {
        this.api = api;
    }
}
```

### Cần phân biệt lifetime

`Singleton`; `→ một instance trong app`

`Transient`; `→ instance mới mỗi lần resolve`

`Scoped`; `→ scope-based lifetime; dùng khi kiến trúc của app cần scope`

App MAUI nhỏ thường dùng Singleton và Transient nhất.

# 10. Services — đừng nhét logic vào Page

Không nên: `MainPage.xaml.cs → {HTTP; database; validation; business logic; UI}`

Nên:

```text
MainPage
ViewModel
Services
   ├── ApiService
   ├── StorageService
   ├── DatabaseService
   └── DeviceService
```

```csharp
public interface IWeatherService
{
    Task<WeatherResult> GetWeatherAsync();
}
```

Implementation:

```csharp
public class WeatherService : IWeatherService
{
    private readonly HttpClient httpClient;

    public WeatherService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
}
```

# 11. HTTP / REST — kỹ năng thực tế

MAUI app thường giao tiếp backend.

Cần biết:

```csharp
HttpClient
GET
POST
PUT
DELETE
JSON
Authentication
Headers
Timeout
CancellationToken
Error handling
```

```csharp
var response = await httpClient.GetAsync("users");

response.EnsureSuccessStatusCode();

var result =
    await response.Content.ReadFromJsonAsync<User[]>();
```

Cần hiểu thêm: `UI → ViewModel → Service → HttpClient → REST API`

Không gọi API trực tiếp từ XAML code-behind nếu logic cần tái sử dụng/test.

# 12. Local Data

80/20 nên học 3 mức:

### Preferences

Key/value đơn giản: `theme = dark`; `username = giang`; `firstLaunch = false`

### SecureStorage

Thông tin cần bảo vệ: `access token`; `refresh token`; `credentials`

### SQLite

Dữ liệu có cấu trúc: `Users`; `Expenses`; `Tasks`; `Notes`; `Products`

```text
Small settings     → Preferences
Sensitive settings → SecureStorage
Structured data    → SQLite
Remote data        → REST/API
```

# 13. Platform-specific code

Quan trọng với Support Engineer.

MAUI không che giấu 100% platform.

```csharp
#if ANDROID

// Android-specific code

#elif WINDOWS

// Windows-specific code

#endif
```

Hoặc: `Platforms/ → {Android/ → {MainActivity.cs}; Windows/ → {App.xaml}}`

Khi gặp bug:

```text
Shared MAUI code?
       ├── YES → debug MAUI layer
       └── NO
           ▼
      Native platform
      ┌────┴─────┐
      Android   Windows
```

Đặc biệt trên Windows:

> **.NET MAUI UI cuối cùng sử dụng WinUI 3.**

Kiến thức WinUI 3 giúp support MAUI Windows.

# 14. Handlers — hiểu concept, chưa cần học thuộc

MAUI Controls không trực tiếp là native controls.

Có abstraction: `MAUI Control → Handler → Native View`

```text
Entry
 ▼
EntryHandler
 ├── Android native view
 └── Windows native view
```

Handlers dùng khi cần: Customize native control; Thay đổi native property; Fix platform-specific behavior; Integrate native APIs.

80/20:

> **Ban đầu, chỉ cần biết Handler tồn tại và khi nào dùng.**

# 15. Resources & Styling

Học: `Resources/ → {Styles/; Images/; Fonts/; AppIcon/}`

Đặc biệt:

```xml
<ResourceDictionary>
    <Style TargetType="Button">
        ...
    </Style>
</ResourceDictionary>
```

Cần hiểu: StaticResource; DynamicResource; Styles; ResourceDictionary; Colors; Fonts; Images; App icon; Splash screen.

# 16. Device APIs

MAUI có cross-platform APIs cho nhiều chức năng: `Connectivity`; `Clipboard`; `FilePicker`; `Preferences`; `SecureStorage`; `Geolocation`; `Sensors`; `Battery`; `Browser`; `Launcher`; `MediaPicker`; `Permissions`; `TextToSpeech`

```csharp
var status = Connectivity.Current.NetworkAccess;
```

Nhóm API này rất hữu ích cho mobile app thực tế.

# 17. App Lifecycle

Cần hiểu app không chỉ có: `Start → Run → Exit`

Mobile app thường: `Launch → Running → Background → Resume → Background → Killed`

Cần biết: App lifecycle; Page lifecycle; Window lifecycle; Background limitations; State restoration.

Đây là nguồn của nhiều bug mobile.

# 18. Permissions

Mobile platform yêu cầu permission.

`Location`; `Camera`; `Microphone`; `Bluetooth`; `Photos`; `Notifications`; `Storage`

`App requests permission → OS decides → Granted / Denied → App handles result`

Không giả định permission luôn được cấp.

# 19. Android — 20% cần biết

Học sâu Android trước, tập trung: `Android SDK`; `ADB`; `Emulator`; `Manifest`; `Permissions`; `Activity`; `Intent`; `Application lifecycle`; `Logcat`; `Gradle-related build errors`; `APK / AAB`

Các lệnh:

```powershell
adb devices
adb install app.apk
adb logcat
adb shell
```

Debug flow: `MAUI app → Android build → APK → ADB / Emulator → Logcat`

# 20. Windows — 20% cần biết

MAUI Windows sử dụng WinUI 3.

Cần hiểu: `MAUI → Windows target → WinUI 3 → Windows App SDK → Windows`

Tập trung: Window; WinUI controls; App lifecycle; MSIX; Windows App SDK runtime; Packaging; App manifest; Windows-specific APIs; Debugging native Windows issues.

Biết WinUI 3 sẽ dễ tiếp cận MAUI Windows hơn.

# 21. Build System — cực kỳ quan trọng cho Support Engineer

Một project có thể target:

```xml
net10.0-android
net10.0-windows10.0.19041.0
```

Mỗi target có toolchain riêng.

```text
.NET SDK
.NET MAUI workload
   ├── Android SDK + JDK
   └── Windows App SDK + Windows SDK
```

Khi build fail, trước tiên xác định: `1. .NET SDK?`; `2. MAUI workload?`; `3. Target framework?`; `4. Platform SDK?`; `5. Device/emulator?`; `6. Project configuration?`; `7. NuGet?`; `8. Native toolchain?`

# 22. Debugging — kỹ năng quan trọng nhất cho Support Engineer

Không chỉ học API; cần biết phân loại lỗi: `Build error → {Project / MSBuild; NuGet; MAUI workload; Android SDK/JDK; Windows SDK/App SDK; XAML compilation}`

`Runtime error → {MAUI; Platform; API/network; Permission; Lifecycle; Native crash}`

`UI bug → {Layout; Binding; Resource; Handler; Platform difference}`

### Debug checklist

`1. Reproduce`; `2. Identify target platform`; `3. Identify exact layer`; `4. Read exception`; `5. Check inner exception`; `6. Check platform logs`; `7. Minimize reproduction`; `8. Compare platform behavior`; `9. Fix`; `10. Regression test`

# 23. .NET MAUI 10 — những thứ cần biết

.NET 10 là **LTS của .NET**; MAUI 10 chú trọng chất lượng, cập nhật controls, XAML, Shell, Window, diagnostics và platform APIs.

Một số điểm đáng chú ý:

### XAML Source Generator

.NET MAUI 10 hỗ trợ XAML source generation:

```xml
<PropertyGroup>
    <MauiXamlInflator>SourceGen</MauiXamlInflator>
</PropertyGroup>
```

Mục tiêu là cải thiện build performance và tooling.

### Animation API

Các API animation dạng synchronous cũ như: `FadeTo`; `ScaleTo`; `RotateTo`; `TranslateTo`

được thay thế/deprecate bởi các API: `FadeToAsync`; `ScaleToAsync`; `RotateToAsync`; `TranslateToAsync`

### MessagingCenter

`MessagingCenter` đã được làm internal.

Nếu cần messaging trong code mới, ưu tiên: `CommunityToolkit.Mvvm → WeakReferenceMessenger`

### CollectionView / CarouselView

Một số handler cải thiện hiệu năng/ổn định từ .NET 9 mặc định trong .NET 10.

### Window

.NET 10 cho phép bật/tắt nút minimize/maximize trên Windows.

### HybridWebView

`HybridWebView` tiếp tục cải thiện việc kết hợp native app + JavaScript.

### .NET Aspire

MAUI 10 có integration/template .NET Aspire service defaults cho telemetry, service discovery và HttpClient.

# 24. Không cần học ngay

Đây là phần quan trọng của 80/20.

**Chưa cần đào sâu ngay:**

`❌ Custom Handlers phức tạp`; `❌ Graphics API nâng cao`; `❌ Native rendering internals`; `❌ iOS provisioning chi tiết`; `❌ Mac Catalyst internals`; `❌ Tizen`; `❌ Native AOT internals`; `❌ Custom platform renderers`; `❌ Advanced Shell internals`; `❌ Advanced animation`; `❌ .NET MAUI source code`

Học khi project thực sự cần.

# 25. Thứ tự học 80/20

## Level 1 — Foundation

`1. MAUI architecture`; `2. Single Project`; `3. Project structure`; `4. MauiProgram.cs`; `5. App / MainPage`

**Checkpoint:**

> Có thể giải thích một project MAUI chạy từ đâu đến đâu.

## Level 2 — UI

`6. XAML`; `7. Controls`; `8. Layout`; `9. Styles`; `10. Resources`

**Checkpoint:**

> Có thể tự xây một màn hình UI hoàn chỉnh.

## Level 3 — MVVM

`11. Binding`; `12. BindingContext`; `13. ViewModel`; `14. ObservableObject`; `15. RelayCommand`; `16. ObservableCollection`

**Checkpoint:**

> UI có thể tương tác với ViewModel mà không nhét business logic vào code-behind.

## Level 4 — Application Architecture

`17. DI`; `18. Services`; `19. Navigation`; `20. REST API`; `21. Error handling`

**Checkpoint:**

> Có thể xây một app CRUD/API nhỏ.

## Level 5 — Local + Device

`22. Preferences`; `23. SecureStorage`; `24. SQLite`; `25. FilePicker`; `26. Permissions`; `27. Connectivity`; `28. Device APIs`

**Checkpoint:**

> Có thể xây app mobile có local storage + device integration.

## Level 6 — Platform

`29. Android`; `30. ADB`; `31. Emulator`; `32. Logcat`; `33. Windows / WinUI 3`; `34. Platform-specific code`; `35. Handlers`

**Checkpoint:**

> Có thể phân biệt MAUI bug và native platform bug.

## Level 7 — Production / Support

`36. Build`; `37. Packaging`; `38. Signing`; `39. Deployment`; `40. Diagnostics`; `41. Crash investigation`; `42. Performance`

**Checkpoint:**

> Có thể reproduce → debug → fix → verify → explain một issue thực tế.

# 26. Learning Strategy — 20% Theory / 80% Hands-on

Không nên: `Docs → Docs → Docs → Docs → "đã học MAUI"`

Nên: `Learn concept → Build tiny feature → Break it intentionally → Debug → Fix → Write down root cause`

### Binding

`Learn Binding → Build User List → Break BindingContext → Debug → Fix`

### API

`Learn HttpClient → Build GET screen → Break endpoint → Handle 404 / timeout → Fix`

### Android

`Build → ADB → Install → Logcat → Break permission → Debug`

### Windows

`Build Windows → Run → Change window behavior → Inspect WinUI → Break configuration → Debug`

# 27. 2 Project Checkpoints

## Checkpoint 1 — MAUI Fundamentals

### Project: Personal Expense Tracker

Platforms: `Windows`; `Android`

Features: `Dashboard`; `Expenses`; `Add / Edit / Delete`; `Search`; `Category`; `Date`; `Monthly total`; `Local SQLite`; `Preferences`; `MVVM`; `Navigation`

Architecture: `Pages → ViewModels → Services → SQLite`

Mục tiêu kiến thức: `XAML`; `Layout`; `Binding`; `MVVM`; `DI`; `Navigation`; `SQLite`; `Validation`; `Android/Windows`

# 28. Checkpoint 2 — Support Engineer Lab

## Project: Device & API Diagnostic Lab

Ứng dụng mô phỏng một utility/support tool.

Modules: `Dashboard → {Device Information; Network Diagnostics; API Tester; Permissions; Storage; Logs; Troubleshooting}`

### Device

Hiển thị: `Platform`; `OS version`; `Device model`; `Screen`; `Network`; `Battery`

### Network

`Connectivity`; `DNS test`; `HTTP GET`; `HTTP POST`; `Timeout`; `Status code`; `Response time`

### API Tester

`Method`; `URL`; `Headers`; `JSON body`; `Response`; `Status`; `Timing`

### Diagnostics

`Permission state`; `Storage state`; `Network state`; `Device state`

### Deliberately broken labs

`Lab 01 — Binding broken`; `Lab 02 — DI registration missing`; `Lab 03 — Navigation route missing`; `Lab 04 — API timeout`; `Lab 05 — Permission denied`; `Lab 06 — Android build failure`; `Lab 07 — Windows-specific issue`; `Lab 08 — Platform-specific behavior mismatch`; `Lab 09 — SQLite migration issue`; `Lab 10 — Release-only issue`

Mỗi lab phải có: `Symptom → Reproduction → Evidence → Root Cause → Fix → Regression Test`

Format phù hợp để luyện Support Engineer.

# 29. MAUI Support Engineer Cheat Sheet

## Khi UI không hiển thị

`BindingContext?`; `Binding path?`; `x:DataType?`; `Property notification?`; `Visibility?`; `Layout size?`; `Resource?`

## Khi Command không chạy

`Command binding?`; `RelayCommand generated?`; `BindingContext?`; `CanExecute?`; `Button enabled?`

## Khi API fail

`URL?`; `DNS?`; `Connectivity?`; `HTTP status?`; `TLS?`; `Timeout?`; `Headers?`; `Authentication?`; `Serialization?`

## Khi Android build fail

`.NET SDK`; `MAUI workload`; `Android SDK`; `JDK`; `API level`; `ADB`; `Gradle`; `NuGet`

## Khi Windows build fail

`.NET SDK`; `MAUI workload`; `Windows SDK`; `Windows App SDK`; `TargetFramework`; `MSIX`; `Manifest`; `Architecture`

## Khi app crash

`Exception`; `InnerException`; `StackTrace`; `Platform`; `Lifecycle`; `Native log`; `Reproduction`

# 30. Documentation Strategy

Không cần đọc Microsoft Learn từ đầu đến cuối.

Ưu tiên: `1. What is .NET MAUI?`; `2. Supported platforms`; `3. Get started`; `4. Single project`; `5. XAML`; `6. Controls`; `7. Layout`; `8. Data binding`; `9. MVVM`; `10. Dependency Injection`; `11. Shell navigation`; `12. Platform APIs`; `13. App lifecycle`; `14. Android`; `15. Windows`; `16. Debugging`; `17. Deployment`; `18. What's new in .NET 10`

Khi gặp vấn đề thực tế: `Issue → Search exact API / exception → Microsoft Learn → GitHub issue → Minimal reproduction → Test`

Tránh học bằng cách đọc docs tuần tự toàn bộ.

# 31. 80/20 Knowledge Map

```text
                         .NET MAUI 10
              ┌───────────────┼────────────────┐
             UI          Application        Platform
        ┌─────┼─────┐    ┌────┼────┐      ┌───┴────┐
      XAML Layout Binding MVVM DI API   Android  Windows
        └─────┴─────┴────┴────┴────┴──────┴────────┘
                         Debug / Support
                    Build → Reproduce → Debug
                       Fix → Verify → Deploy
```

# 32. Final 80/20 Checklist

Hoàn thành checklist này là đủ nền tảng để làm project MAUI thực tế:

### Core

- [ ] Biết .NET MAUI là gì
- [ ] Hiểu Single Project
- [ ] Hiểu multi-targeting
- [ ] Đọc được `.csproj`
- [ ] Hiểu `MauiProgram.cs`

### UI

- [ ] XAML
- [ ] Controls
- [ ] Grid
- [ ] StackLayout
- [ ] CollectionView
- [ ] Styles
- [ ] Resources

### MVVM

- [ ] Binding
- [ ] BindingContext
- [ ] ObservableObject
- [ ] ObservableCollection
- [ ] RelayCommand
- [ ] CommunityToolkit.Mvvm

### App

- [ ] Shell
- [ ] Navigation
- [ ] DI
- [ ] Services
- [ ] HttpClient
- [ ] JSON
- [ ] Error handling

### Data

- [ ] Preferences
- [ ] SecureStorage
- [ ] SQLite

### Platform

- [ ] Android SDK
- [ ] ADB
- [ ] Emulator
- [ ] Logcat
- [ ] Windows / WinUI 3
- [ ] Platform-specific code
- [ ] Handlers

### Support

- [ ] Build troubleshooting
- [ ] Runtime troubleshooting
- [ ] XAML debugging
- [ ] Binding debugging
- [ ] Network debugging
- [ ] Platform debugging
- [ ] Deployment troubleshooting

### .NET 10

- [ ] What's new
- [ ] XAML Source Generator
- [ ] Animation async APIs
- [ ] CollectionView changes
- [ ] MessagingCenter migration
- [ ] Window improvements
- [ ] .NET Aspire integration

# 33. Definition of Done

**Nắm nền tảng .NET MAUI 10** khi tự làm được, không copy tutorial: `Create project → Design UI → Build MVVM → Register DI → Navigate → Call REST API → Store local data → Use device API → Run Android → Run Windows → Debug platform-specific issue → Package application`

Quan trọng hơn:

> Khi có issue, không chỉ sửa code mà phải xác định **MAUI layer, application layer, build layer hay native platform layer**.

Đây là năng lực 80/20 giá trị nhất cho **Support Engineer .NET MAUI**.

## Official Microsoft Learn

- [What is .NET MAUI?](https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui)
- [Supported platforms](https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms)
- [Learning resources](https://learn.microsoft.com/en-us/dotnet/maui/get-started/resources)
- [.NET MAUI documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [What's new in .NET MAUI for .NET 10](https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10)
- [What's new in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview)
