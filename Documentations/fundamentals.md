# .NET MAUI 10 — Fundamentals 80/20

> Mục tiêu: nắm **fundamentals cốt lõi sau XAML** để xây dựng, maintain và troubleshoot ứng dụng .NET MAUI 10 thực tế.
>
> Phương pháp: **20% concepts → 80% practical usage**.
>
> Thứ tự ưu tiên:
>
> ```text
> App architecture
>     ↓
> Single Project
>     ↓
> Dependency Injection
>     ↓
> Data Binding
>     ↓
> Resources
>     ↓
> Shell / Navigation
>     ↓
> Lifecycle
>     ↓
> Behaviors
>     ↓
> Templates / Triggers
>     ↓
> Accessibility / Localization
> ```
>
> Tài liệu này tiếp nối:
>
> - `overview.md` — tổng quan .NET MAUI 10
> - `xaml.md` — XAML 80/20

# 1. Fundamentals Mental Model

Ứng dụng MAUI thực tế: `MAUI App`:
- UI: XAML, Controls, Styles, Templates.
- Application: MVVM, DI, Services, API/DB.
- Platform: Android, Windows, iOS, MacCatalyst.

Các nhánh dùng `MAUI infrastructure`: Binding (Behaviors, Triggers, Templates), Shell, Lifecycle, Resources, Handlers.

Cốt lõi:

> **Fundamentals là cách các phần của app kết nối, không phải danh sách API để học thuộc.**

# 2. Single Project

MAUI dùng Single Project quản lý nhiều platform.

`MyApp → {App.xaml; AppShell.xaml; MauiProgram.cs; Pages/; ViewModels/; Services/; Models/; Resources/; Platforms/ → {Android/; Windows/; iOS/; MacCatalyst/}}`

Một `.csproj` có thể target nhiều framework:

```xml
<TargetFrameworks>
    net10.0-android;
    net10.0-windows10.0.19041.0
</TargetFrameworks>
```

# 3. Shared vs Platform Code

`Shared code`:
- Cross-platform: ViewModel, Service, Model, XAML.
- Platform-specific: Android, Windows, iOS, MacCatalyst.

Ưu tiên: `Shared code first → Platform code only when necessary`

Platform-specific code có thể dùng:

```csharp
#if ANDROID
#elif WINDOWS
#endif
```

hoặc: `Platforms/ → {Android/; Windows/}`

# 4. Dependency Injection

DI là fundamental cốt lõi.

Thay vì:

```csharp
var service = new ApiService();
```

khắp nơi, hãy đăng ký dependency ở startup:

```csharp
builder.Services.AddSingleton<ApiService>();
builder.Services.AddTransient<SettingsPage>();
builder.Services.AddTransient<SettingsViewModel>();
```

Sau đó inject:

```csharp
public class MainViewModel
{
    private readonly ApiService apiService;

    public MainViewModel(ApiService apiService)
    {
        this.apiService = apiService;
    }
}
```

`MauiProgram — Register → DI Container — Resolve → ViewModel / Service / Page`

# 5. DI Lifetime

Ba lifetime chính: `Singleton`; `Transient`; `Scoped`

## Singleton

Một instance trong DI container:

```csharp
builder.Services.AddSingleton<ApiService>();
```

Phù hợp với: `App-level services`; `Configuration`; `API service`; `Shared state`

Cẩn thận với mutable state.

## Transient

Instance mới mỗi lần resolve:

```csharp
builder.Services.AddTransient<MainViewModel>();
```

Phù hợp với: `ViewModels`; `Pages`; `Short-lived services`

## Scoped

Lifetime theo scope riêng; không phải scenario MAUI nào cũng cần.

80/20: Default mental model:
- Shared app service → Singleton.
- Page/ViewModel → Transient.
- Scoped → chỉ dùng khi architecture cần scope rõ ràng.

# 6. DI + MVVM Architecture

Pattern nên hướng tới: `Page — Binding → ViewModel — Dependency Injection → Service → {API; Database; Storage; Device}`

`MainPage → MainViewModel → IUserService → UserService → REST API`

Không nên: `Page → {HTTP request; SQLite; business logic; authentication; UI}`

# 7. Data Binding

Binding kết nối:

```text
View
  ↕
Data
```

```xml
<Label
    Text="{Binding UserName}" />
```

Nếu:

```csharp
public string UserName { get; set; }
```

thì Label đọc property đó.

# 8. BindingContext

Binding cần source.

```csharp
BindingContext = viewModel;
```

Sau đó:

```xml
<Label
    Text="{Binding UserName}" />
```

`Page.BindingContext → ViewModel → UserName`

Nếu binding không hoạt động:

> Kiểm tra `BindingContext` trước.

# 9. OneWay vs TwoWay

## OneWay

`ViewModel → View`

```xml
<Label
    Text="{Binding UserName}" />
```

Phù hợp: `Display-only UI`

## TwoWay

`ViewModel ↔ View`

```xml
<Entry
    Text="{Binding UserName, Mode=TwoWay}" />
```

Phù hợp: `Forms`; `Input`; `Switch`; `Picker`; `Checkbox`

# 10. Observable State

State đổi phải thông báo cho UI.

CommunityToolkit.Mvvm:

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string userName = string.Empty;
}
```

Toolkit generate property notification.

`State changes → PropertyChanged → Binding → UI updates`

# 11. Commands

UI action thường nên expose qua Command.

```xml
<Button
    Text="Save"
    Command="{Binding SaveCommand}" />
```

ViewModel:

```csharp
[RelayCommand]
private async Task SaveAsync()
{
    // Save
}
```

Architecture: `Button → Command → ViewModel → Service`

# 12. Behaviors

Behavior gắn hành vi tái sử dụng vào control mà không cần subclass.

`Entry` + `ValidationBehavior`

hoặc: `Entry` + `NumericBehavior`

`Control → {Behavior → {Adds behavior}}`

Dùng behavior khi logic: `Reusable`; `UI-related`; `Attached to controls`

`Validation`; `Formatting`; `Focus behavior`; `Input restrictions`; `Event-to-command`

# 13. Behavior vs ViewModel

Đừng nhét mọi thứ vào Behavior.

### Behavior phù hợp:

`UI-specific reusable behavior`

### ViewModel phù hợp:

`Application state`; `Business behavior`; `Commands`; `Validation rules tied to app logic`

- UI concern → Behavior.
- Application concern → ViewModel / Service.

# 14. Data Templates

DataTemplate định nghĩa UI cho từng data item.

```xml
<CollectionView
    ItemsSource="{Binding Products}">

    <CollectionView.ItemTemplate>

        <DataTemplate
            x:DataType="models:Product">

            <Grid>
                <Label
                    Text="{Binding Name}" />

                <Label
                    Text="{Binding Price}" />
            </Grid>

        </DataTemplate>

    </CollectionView.ItemTemplate>

</CollectionView>
```

```text
Collection
 ├── Product
 │    ↓
 │  DataTemplate
 │    ↓
 │  Product UI
 ├── Product
 │    ↓
 │  Product UI
 └── Product
      ↓
    Product UI
```

# 15. `x:DataType` trong DataTemplate

Rất nên sử dụng:

```xml
<DataTemplate
    x:DataType="models:Product">
```

Sau đó:

```xml
<Label
    Text="{Binding Name}" />
```

Lợi ích: `Compiled binding`; `Better tooling`; `Compile-time checking`; `Better performance`

# 16. ControlTemplate

DataTemplate: `Data → UI`

ControlTemplate: `Control → visual structure`

`Button → ControlTemplate → Custom visual representation`

Dùng khi muốn thay đổi: `Visual structure`; `Visual composition`; `Control appearance`

Không cần học ControlTemplate sâu ngay.

80/20:

> **Học DataTemplate sớm; học ControlTemplate khi cần custom control appearance.**

# 17. Triggers

Trigger đổi property khi thỏa condition.

```xml
<Trigger
    TargetType="Button"
    Property="IsEnabled"
    Value="False">

    <Setter
        Property="Opacity"
        Value="0.5" />

</Trigger>
```

`Condition → Trigger → Setter → UI changes`

# 18. Trigger Types

Các loại nên biết: `Property Trigger`; `Data Trigger`; `Event Trigger`; `MultiTrigger`

## Property Trigger

`Property = Value`

`IsEnabled = false`

## Data Trigger

Dựa trên binding value: `IsSelected = true`

## MultiTrigger

Nhiều conditions: `Condition A`; `AND`; `Condition B`

80/20: `Property Trigger`; `Data Trigger`

là hai loại nên học trước.

# 19. Trigger vs VisualState vs Behavior

Đừng dùng sai công cụ.

- Trigger → thay đổi property dựa trên condition.
- Behavior → attach reusable behavior.
- VisualState → visual state của control/UI.
- ViewModel → application state/business behavior.

- Need visual property change? → Trigger / VisualState.
- Need reusable control behavior? → Behavior.
- Need application logic? → ViewModel / Service.

# 20. Resource Dictionaries

ResourceDictionary chứa resource tái sử dụng.

```xml
<Application.Resources>

    <ResourceDictionary>

        <Color
            x:Key="PrimaryColor">
            #512BD4
        </Color>

        <Style
            TargetType="Button">
            ...
        </Style>

    </ResourceDictionary>

</Application.Resources>
```

Resources có thể là: `Color`; `Brush`; `Style`; `Converter`; `DataTemplate`; `ControlTemplate`

# 21. Resource Scope

Các scope phổ biến: `Application.Resources → Page.Resources → Control.Resources`

```xml
<ContentPage.Resources>
    ...
</ContentPage.Resources>
```

Resource chỉ dùng trong page thì đặt ở Page.

Nếu dùng toàn app:

```xml
<Application.Resources>
    ...
</Application.Resources>
```

80/20 rule:

> **Đặt resource ở scope nhỏ nhất đủ dùng.**

# 22. StaticResource

```xml
BackgroundColor="{StaticResource PrimaryColor}"
```

Dùng cho resource ít thay đổi runtime.

Phù hợp: `Colors`; `Styles`; `Converters`; `Templates`

# 23. DynamicResource

```xml
BackgroundColor="{DynamicResource PrimaryColor}"
```

Dùng khi UI cần phản ứng lúc resource đổi.

Typical use: `Theme`; `Runtime resource replacement`; `Dynamic visual configuration`

- StaticResource → lookup resource.
- DynamicResource → observe resource changes.

# 24. Shell

Shell cung cấp app navigation structure.

`App → {AppShell → {Flyout; Tabs; Routes}}`

```xml
<Shell
    x:Class="MyApp.AppShell">

    <ShellContent
        Title="Home"
        ContentTemplate="{DataTemplate local:HomePage}" />

</Shell>
```

# 25. Shell Routes

Register route:

```csharp
Routing.RegisterRoute(
    nameof(DetailPage),
    typeof(DetailPage));
```

Navigate:

```csharp
await Shell.Current.GoToAsync(
    nameof(DetailPage));
```

`Route → Page`

# 26. Route Parameters

Có thể truyền data:

```csharp
await Shell.Current.GoToAsync(
    $"{nameof(DetailPage)}?id={id}");
```

Page/ViewModel nhận parameter qua cơ chế Shell navigation.

80/20: `Route`; `Query parameter`; `Back navigation`

là đủ cho giai đoạn đầu.

# 27. Shell Navigation Mental Model

`User action → Command → Shell.GoToAsync() → Route → Page → ViewModel`

Tránh rải navigation logic ở mọi page; app lớn có thể tách thành service.

# 28. App Lifecycle

App lifecycle khác với page lifecycle.

`App Launch → Running → Background / Inactive → Resume → Running`

App có thể: `Suspend`; `Resume`; `Terminate`

Mobile OS có thể kill app trong background.

# 29. Lifecycle Events

Lifecycle events/platform APIs giúp phản ứng với: `Created`; `Activated`; `Deactivated`; `Stopped`; `Resumed`

Không nên giả định: `OnSleep → app sẽ luôn trở lại`

Mobile OS có thể terminate process.

# 30. Lifecycle State Management

Nếu app có state quan trọng: `Don't rely only on memory.`

`Unsaved form`; `Current user`; `Navigation state`; `Draft`; `Authentication state`

Cần cân nhắc persistence: `Preferences`; `SecureStorage`; `SQLite`; `Server`

`Running state → Persist important state → App killed → Restore state`

# 31. Page Lifecycle

Đừng nhầm: `App lifecycle`

với: `Page lifecycle`

Khi app vẫn chạy, page có thể vào/rời navigation stack nhiều lần.

Khi làm screen: `Appearing`; `Disappearing`

có thể dùng để refresh/release UI resources.

Tránh heavy business logic trong lifecycle event.

# 32. Lifecycle + Services

`App resumes → ViewModel → RefreshService → API`

Nếu logic này cần reuse: `Lifecycle → Service`

thay vì: `Lifecycle event → 100 lines of business logic`

# 33. Accessibility

Thiết kế accessibility từ đầu, không đợi cuối project.

Semantic properties của MAUI giúp assistive technologies hiểu UI.

`Visual UI + Semantic information → Accessibility`

# 34. Semantic Properties

Các khái niệm cần biết: `SemanticProperties.Description`; `SemanticProperties.Hint`; `SemanticProperties.HeadingLevel`

```xml
<Image
    Source="profile.png"
    SemanticProperties.Description="User profile picture" />
```

Mục tiêu: `Screen reader`; `Assistive technology`; `Keyboard / alternative input`

hiểu control tốt hơn.

# 35. Accessibility 80/20 Rules

### Rule 1

Interactive controls cần mô tả có nghĩa.

### Rule 2

Không dùng text mơ hồ: `"Click here"`

thay vì: `"Save expense"`

### Rule 3

Ảnh trang trí không mang thông tin thì không cần semantic description.

### Rule 4

Heading cần semantic structure rõ ràng.

# 36. Localization

Localization = app hỗ trợ nhiều ngôn ngữ/culture.

Không nên:

```xml
<Button
    Text="Save" />
```

rải hard-coded text nếu app cần đa ngôn ngữ; dùng resource localization.

`UI → Localized resource key → Resource file → Current culture → Localized text`

# 37. Localization Resource

`Resources/`; `Strings.resx`; `Strings.vi.resx`; `Strings.ja.resx`

`Save → {English → Save; Vietnamese → Lưu; Japanese → 保存}`

80/20: `Resource file`; `Culture`; `Localized strings`; `Date / number formatting`

# 38. Localization Beyond Text

Localization không chỉ dịch text; cần xét: `Date format`; `Time format`; `Number format`; `Currency`; `Pluralization`; `Right-to-left languages`

`1,234.50`

có thể khác theo culture.

# 39. Bindable Properties

BindableProperty là nền tảng của MAUI controls.

Một normal CLR property:

```csharp
public string Text { get; set; }
```

không tự cung cấp đủ cơ chế cho MAUI UI.

BindableProperty hỗ trợ: `Data binding`; `Default values`; `Property changed callbacks`; `Property validation`; `Property coercion`; `Styles`; `Triggers`

`BindableProperty → {Binding; Style; Trigger; Property change}`

# 40. Khi nào cần BindableProperty?

Khi tạo **custom MAUI control**, nếu property cần: `Bind`; `Style`; `Trigger`; `Observe changes`

thì cần chú ý BindableProperty.

```csharp
public static readonly BindableProperty
    TitleProperty =
    BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(MyControl));
```

Sau đó:

```csharp
public string Title
{
    get => (string)GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
}
```

# 41. BindableProperty vs ObservableProperty

Rất dễ nhầm.

## BindableProperty

Thường dùng cho: `MAUI Control`; `Custom Control`; `Bindable UI property`

## `[ObservableProperty]`

CommunityToolkit.Mvvm dùng cho: `ViewModel state`; `Model/ViewModel notification`

- Custom Control → BindableProperty.
- ViewModel → ObservableProperty.

# 42. Custom Control Mental Model

`MyCard → {TitleProperty; SubtitleProperty; IconProperty}`

Property cần binding nên dùng BindableProperty:

```xml
<controls:MyCard
    Title="{Binding Title}"
    Subtitle="{Binding Subtitle}" />
```

# 43. DataTemplate vs ControlTemplate

Cần phân biệt:

- DataTemplate: How DATA is displayed.
- ControlTemplate: How CONTROL is visually structured.

`CollectionView → DataTemplate → Product card`

Trong khi: `Custom control → ControlTemplate → Visual structure`

# 44. Triggers + Styles

Triggers có thể đặt trong Style:

```xml
<Style TargetType="Button">

    <Setter
        Property="Opacity"
        Value="1" />

    <Style.Triggers>

        <Trigger
            Property="IsEnabled"
            Value="False">

            <Setter
                Property="Opacity"
                Value="0.5" />

        </Trigger>

    </Style.Triggers>

</Style>
```

Cách tạo visual behavior tái sử dụng.

# 45. Trigger + Binding

DataTrigger có thể theo ViewModel state.

```xml
<DataTrigger
    TargetType="Label"
    Binding="{Binding IsError}"
    Value="True">

    <Setter
        Property="TextColor"
        Value="Red" />

</DataTrigger>
```

`ViewModel state → Binding → Trigger → Visual change`

# 46. Fundamentals Architecture

Một production-style screen: `Page → XAML → {Binding; Style; Behavior} → View → ViewModel → {Command; State} → Service → {API; DB; Device}`

# 47. Feature Implementation Flow

Khi xây một feature mới: `1. Define Model → 2. Define Service → 3. Define ViewModel → 4. Register DI → 5. Create Page → 6. Create XAML → 7. Add Binding → 8. Add Navigation → 9. Add Validation → 10. Test Android → 11. Test Windows`

Nên dùng workflow này thường xuyên.

# 48. Support Engineer Debugging Flow

Khi user báo:

> "Button doesn't work."

Đừng sửa ngay.

Làm: `Reproduce → Button visible? → IsEnabled? → Command bound? → BindingContext? → RelayCommand generated? → CanExecute? → Exception? → Service failure?`

# 49. Support Engineer: "App loses data"

Kiểm tra: `App lifecycle → Was app killed? → Was state persisted? → Preferences / SQLite / SecureStorage? → Restore logic?`

Đừng chỉ nhìn UI.

# 50. Support Engineer: "Works on Windows but not Android"

Phân loại: `Shared XAML? → Shared ViewModel? → Service? → Platform API? → Permission? → Handler? → Android lifecycle? → Android SDK?`

`Same app ≠ Same platform behavior`

# 51. Support Engineer: "Only one language is broken"

Kiểm tra: `Culture → Resource file → Resource key → Fallback → Formatting`

Không mặc định localization bug là lỗi dịch.

# 52. 80/20 Learning Order

## Level 1 — Architecture

Học: `Single Project`; `Platforms/`; `MauiProgram.cs`; `Shared code`; `Platform code`

Checkpoint:

> Có thể giải thích project MAUI được build cho Windows và Android như thế nào.

## Level 2 — DI

Học: `Register`; `Resolve`; `Constructor injection`; `Singleton`; `Transient`

Checkpoint:

> Tạo được Page → ViewModel → Service bằng DI.

## Level 3 — Binding

Học: `BindingContext`; `OneWay`; `TwoWay`; `ObservableObject`; `Command`

Checkpoint:

> UI phản ứng đúng với ViewModel state.

## Level 4 — Resources

Học: `ResourceDictionary`; `StaticResource`; `DynamicResource`; `Style`; `Scope`

Checkpoint:

> Tạo được app-level design resources.

## Level 5 — Shell

Học: `Shell`; `Route`; `GoToAsync`; `Query`; `Back navigation`

Checkpoint:

> Tạo được multi-page app.

## Level 6 — Lifecycle

Học: `App lifecycle`; `Page lifecycle`; `State persistence`; `Resume`; `Background`

Checkpoint:

> Không mất critical state khi app background/killed.

## Level 7 — Reusable UI

Học: `Behavior`; `DataTemplate`; `ControlTemplate`; `Trigger`; `BindableProperty`

Checkpoint:

> Tạo được reusable UI component.

## Level 8 — Production Fundamentals

Học: `Accessibility`; `Localization`; `Platform differences`; `Diagnostics`

Checkpoint:

> Feature chạy đa nền tảng, có accessibility/localization cơ bản.

# 53. Mini Project — Fundamentals Checkpoint

## Project: Support Dashboard

Tạo app:

```text
┌──────────────────────────────┐
│ Support Dashboard            │
├──────────────────────────────┤
│ Device                       │
│ Windows / Android            │
│ Network                      │
│ Connected                    │
│ API Status                   │
│ Healthy                      │
│ [ Run Diagnostics ]          │
└──────────────────────────────┘
```

Architecture:

```text
DashboardPage
      ↓
DashboardViewModel
      ↓
DiagnosticsService
      ↓
 ┌────┼─────┐
Network Device Storage
```

Phải sử dụng: `DI`; `Binding`; `MVVM`; `Command`; `ResourceDictionary`; `Style`; `Shell`; `Behavior`; `DataTemplate`; `Trigger`

# 54. Broken Lab 01 — DI

Cố tình không register:

```csharp
builder.Services.AddSingleton<IDiagnosticsService,
                              DiagnosticsService>();
```

Nhưng inject:

```csharp
public DashboardViewModel(
    IDiagnosticsService service)
{
}
```

Nhiệm vụ: `1. Reproduce`; `2. Read exception`; `3. Identify missing registration`; `4. Register dependency`; `5. Verify`

Root cause: `Dependency not registered`

# 55. Broken Lab 02 — Binding

XAML:

```xml
<Label
    Text="{Binding ConnectionStatus}" />
```

ViewModel:

```csharp
public string NetworkStatus { get; }
```

Root cause: `Binding path mismatch`

# 56. Broken Lab 03 — Shell Route

Navigate:

```csharp
await Shell.Current.GoToAsync(
    nameof(DetailsPage));
```

Nhưng không register:

```csharp
Routing.RegisterRoute(
    nameof(DetailsPage),
    typeof(DetailsPage));
```

Root cause: `Unknown route`

# 57. Broken Lab 04 — Resource

XAML:

```xml
BackgroundColor="{StaticResource Primary}"
```

Resource:

```xml
x:Key="PrimaryColor"
```

Root cause: `Resource key mismatch`

# 58. Broken Lab 05 — Lifecycle

Cố tình chỉ giữ draft trong memory: `User enters text → App background → OS kills process → App starts → Draft disappears`

Nhiệm vụ: `Identify lifecycle/state problem`; `Choose persistence strategy`; `Implement restore`; `Test again`

# 59. Broken Lab 06 — Accessibility

Cố tình tạo:

```xml
<Image
    Source="warning.png" />
```

nhưng không cung cấp semantic information cho image có ý nghĩa.

Nhiệm vụ: `Identify accessibility gap`; `Add semantic description`; `Test with accessibility tooling`

# 60. Broken Lab 07 — Localization

Cố tình:

```xml
<Button Text="Save" />
```

trong app cần hỗ trợ: `English`; `Vietnamese`; `Japanese`

Nhiệm vụ: `Move string to resource`; `Create localized resources`; `Switch culture`; `Verify UI`

# 61. Broken Lab 08 — BindableProperty

Tạo custom control: `SupportCard`

với:

```csharp
public string Title { get; set; }
```

Sau đó:

```xml
Title="{Binding Title}"
```

không behave như một MAUI bindable property.

Nhiệm vụ: `Convert property to BindableProperty`; `Test binding`; `Test Style`; `Test Trigger`

# 62. 80/20 Cheat Sheet

## DI

```csharp
AddSingleton<T>();
AddTransient<T>();
```

## Binding

```xml
Text="{Binding Name}"
```

## TwoWay

```xml
Text="{Binding Name, Mode=TwoWay}"
```

## Command

```xml
Command="{Binding SaveCommand}"
```

## Resource

```xml
"{StaticResource PrimaryColor}"
```

## Dynamic resource

```xml
"{DynamicResource PrimaryColor}"
```

## Shell

```csharp
await Shell.Current.GoToAsync("details");
```

## Route

```csharp
Routing.RegisterRoute(
    "details",
    typeof(DetailsPage));
```

## DataTemplate

```xml
<DataTemplate
    x:DataType="models:Item">
```

## Behavior

`Control + reusable UI behavior`

## Trigger

`Condition → Setter`

## BindableProperty

`Custom Control property`

## Accessibility

```xml
SemanticProperties.Description="..."
```

# 63. What to Learn Deeply

Đây là phần nên dành phần lớn thời gian: `★★★★★ Data Binding`; `★★★★★ MVVM`; `★★★★★ Dependency Injection`; `★★★★★ Shell Navigation`; `★★★★★ ResourceDictionary / Styles`; `★★★★★ Lifecycle`; `★★★★☆ Single Project`; `★★★★☆ Behaviors`; `★★★★☆ DataTemplate`; `★★★★☆ BindableProperty`; `★★★★☆ Triggers`; `★★★☆☆ Accessibility`; `★★★☆☆ Localization`; `★★☆☆☆ ControlTemplate`

Đây là **ưu tiên học 80/20**, không phải xếp hạng chất lượng.

# 64. What NOT to Learn Yet

Chưa cần đào sâu ngay: `❌ Custom XAML markup extensions`; `❌ Complex ControlTemplate systems`; `❌ Advanced MultiTrigger architectures`; `❌ Custom handlers implementation`; `❌ Deep lifecycle internals`; `❌ Advanced localization frameworks`; `❌ Complex accessibility automation`; `❌ Custom rendering internals`

Học khi project cần.

# 65. Fundamentals Troubleshooting Matrix

| Symptom | First place to inspect |
|---|---|
| UI value blank | BindingContext |
| UI doesn't update | PropertyChanged / ObservableObject |
| Button doesn't work | Command + BindingContext |
| Page navigation fails | Shell route |
| Service injection fails | DI registration |
| Style missing | ResourceDictionary / scope |
| Resource not found | `x:Key` / resource scope |
| List item wrong | DataTemplate / `x:DataType` |
| Custom control won't bind | BindableProperty |
| UI changes conditionally | Trigger / VisualState |
| App loses state | Lifecycle + persistence |
| Android differs from Windows | Platform-specific layer |
| Screen reader issue | Semantic properties |
| Wrong language | Localization resources / culture |

# 66. Definition of Done

**Nắm MAUI Fundamentals** khi làm được:

- [ ] Explain Single Project
- [ ] Explain shared vs platform code
- [ ] Register services with DI
- [ ] Use Singleton / Transient correctly
- [ ] Build Page → ViewModel → Service
- [ ] Use BindingContext
- [ ] Use OneWay / TwoWay binding
- [ ] Use ObservableObject
- [ ] Use Commands
- [ ] Use ResourceDictionary
- [ ] Use StaticResource / DynamicResource
- [ ] Build reusable Styles
- [ ] Configure Shell navigation
- [ ] Register routes
- [ ] Pass navigation data
- [ ] Understand app lifecycle
- [ ] Persist critical state
- [ ] Create a Behavior
- [ ] Create a DataTemplate
- [ ] Understand ControlTemplate
- [ ] Use Triggers
- [ ] Create a BindableProperty
- [ ] Add semantic accessibility properties
- [ ] Localize UI strings
- [ ] Debug DI errors
- [ ] Debug binding errors
- [ ] Debug navigation errors
- [ ] Debug resource errors
- [ ] Explain platform-specific failures

# 67. Final Mental Model

Flow tổng hợp fundamentals: `MAUI App → Single Project`:
- Platform → Android / Windows.
- Shared → XAML → {Binding; Style; Behavior} → ViewModel → {DI; Commands; State} → Services → {API; DB; Device} → Lifecycle → Persist / Restore → Accessibility → Localization.

# 68. Recommended Study Sequence

Sau `overview.md` và `xaml.md`, học file này theo thứ tự: `overview.md → xaml.md → fundamentals.md → Checkpoint Project → Broken Labs → Real-world debugging`

Không cần thuộc hết fundamentals trước khi code.

Cách học hiệu quả: `Read concept → Build 20–50 lines → Break it → Observe error → Debug → Fix → Write root cause`

# 69. Official Microsoft Learn

Nguồn tài liệu chính:

- [Accessibility](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/accessibility?view=net-maui-10.0)
- [App lifecycle](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle?view=net-maui-10.0)
- [Behaviors](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/behaviors?view=net-maui-10.0)
- [Data binding](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding?view=net-maui-10.0)
- [Dependency injection](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection?view=net-maui-10.0)
- [Localization](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/localization?view=net-maui-10.0)
- [Bindable properties](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/bindable-properties?view=net-maui-10.0)
- [Resource dictionaries](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/resource-dictionaries?view=net-maui-10.0)
- [.NET MAUI Shell](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell?view=net-maui-10.0)
- [Single project](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/single-project?view=net-maui-10.0)
- [Control templates](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/controltemplate?view=net-maui-10.0)
- [Data templates](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/datatemplate?view=net-maui-10.0)
- [Triggers](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/triggers?view=net-maui-10.0)

> Các links được chuyển sang `view=net-maui-10.0` để thống nhất với lộ trình .NET MAUI 10.
