# .NET MAUI Platform Integration — 80/20 Guide

> Mục tiêu: hiểu **Platform Integration .NET MAUI 10** cho Support Engineer: phân biệt cross-platform/platform-specific, chọn đúng layer API/Platform/Handler và debug khác biệt Windows/Android.
>
> Tài liệu chính: Microsoft Learn — Platform integration, Android và iOS platform documentation.
>
> Ưu tiên **Windows + Android** là hai target học trước; thêm iOS/Mac Catalyst để hiểu kiến trúc tổng thể.

## 1. Platform Integration là gì?

MAUI dùng một codebase chung, vẫn truy cập chức năng riêng từng OS.

```text
                    .NET MAUI App
             ┌───────────┴───────────┐
      Cross-platform APIs      Platform-specific
             │              ┌────────┼────────┐
             ▼              ▼        ▼        ▼
      Essentials/API      Android  Windows   iOS
             ▼
       Native platform
```

Microsoft gọi **platform integration** là truy cập device features, chức năng riêng platform, native APIs và cấu hình từng platform. citeturn0search1turn0search14

Điểm quan trọng nhất:

> .NET MAUI không có nghĩa là mọi API đều phải giống nhau ở mọi platform.

Bạn có thể có:

- Shared API: works everywhere.
- Platform-specific API: works only on one platform.

# 2. 80/20 Mental Model

Requirement OS/device: hỏi 4 câu: `1. MAUI đã có API cross-platform chưa? → 2. Có platform-specific API chưa? → 3. Có cần native API trực tiếp không? → 4. Có cần Handler không?`

Decision tree:

```text
Need platform functionality
          ▼
MAUI cross-platform API?
     YES          NO
      ▼           ▼
Use MAUI       Platform-specific
API            API / native code
                   ▼
             Handler needed?
              NO      YES
               ▼       ▼
          Platform     Handler
          integration  customization
```

# 3. Các layer cần phân biệt

Đây là mô hình cốt lõi của tài liệu.

1. Layer 1: .NET MAUI cross-platform API.
2. Layer 2: Platform-specific MAUI API.
3. Layer 3: Platform code (`#if ANDROID`, `#if WINDOWS`, `#if IOS`).
4. Layer 4: Native platform API.
5. Layer 5: Handler.

Không phải issue nào cũng cần Layer 4–5.

Nguyên tắc:

> Dừng ở layer thấp nhất đủ giải quyết requirement.

# 4. Platform-specific ≠ Handler

Tránh nhầm hai khái niệm này.

### Platform-specific MAUI API

Dùng chức năng riêng platform **không cần customize Handler**.

Microsoft có platform-specific APIs cho Android/iOS; Android gồm keyboard input mode, WebView zoom/mixed content và một số TabbedPage behaviors. citeturn0search5

### Handler

Dùng để customize/can thiệp native control.

`Platform-specific feature → Try MAUI platform-specific API`

`Native control customization → Handler`

Quy tắc 80/20: `Nếu Microsoft đã có API → Dùng API đó`

`Nếu cần native control customization → Handler`

# 5. Single Project + Platform folders

Single Project gom nhiều platform vào một project.

`MyMauiApp → {App.xaml; MainPage.xaml; MauiProgram.cs; Platforms → {Android; Windows; iOS; MacCatalyst}; Resources}`

Shared code: `MainPage.xaml`; `ViewModels`; `Services`; `Models`

Platform code: `Platforms/Android`; `Platforms/Windows`; `Platforms/iOS`; `Platforms/MacCatalyst`

Đây là nền tảng tổ chức platform-specific implementation.

# 6. `#if ANDROID` / `#if WINDOWS`

Khi cần code nhỏ khác nhau giữa platform:

```csharp
#if ANDROID
    // Android implementation
#elif WINDOWS
    // Windows implementation
#endif
```

```csharp
public void DoPlatformWork()
{
#if ANDROID
    // Android
#elif WINDOWS
    // Windows
#endif
}
```

### Khi nào nên dùng?

Phù hợp khi: khác biệt rất nhỏ; API call ngắn; logic vẫn thuộc cùng một abstraction.

Không nên để một method trở thành: `200 lines Android` + `200 lines Windows` + `200 lines iOS`

Trường hợp đó nên tách platform implementation thành service.

# 7. Platform-specific Service

Pattern tốt hơn cho logic lớn: `IPlatformService → {AndroidPlatformService; WindowsPlatformService}`

Shared code chỉ biết:

```csharp
IPlatformService
```

Platform implementation biết: `Android APIs`; `Windows APIs`

`ViewModel → IPlatformService → Platform implementation`

Nhờ đó ViewModel không phụ thuộc native API.

# 8. Dependency Injection + Platform Services

Có thể đăng ký implementation riêng từng platform.

```csharp
#if ANDROID
builder.Services.AddSingleton<IPlatformService, AndroidPlatformService>();
#elif WINDOWS
builder.Services.AddSingleton<IPlatformService, WindowsPlatformService>();
#endif
```

Sau đó:

```csharp
public class MyViewModel
{
    private readonly IPlatformService _platformService;

    public MyViewModel(IPlatformService platformService)
    {
        _platformService = platformService;
    }
}
```

`ViewModel → Interface → DI → Platform implementation`

Thường rõ ràng hơn để ViewModel gọi trực tiếp:

```csharp
Android.Some.Native.Api()
```

# 9. Device Information

MAUI có API lấy thông tin thiết bị/platform.

```csharp
DeviceInfo.Current
```

`App → DeviceInfo → Platform / Device / Version / Idiom`

Ví dụ use cases: `Is Android?`; `Is Windows?`; `What device idiom?`; `What platform version?`

Dùng cho: diagnostics; telemetry; feature gating; UI adaptation; troubleshooting.

Nếu kiểm tra được API trực tiếp, không dùng device detection thay capability detection.

# 10. Device Sensors

MAUI hỗ trợ sensor phổ biến qua cross-platform APIs.

Ví dụ nhóm: `Accelerometer`; `Gyroscope`; `Magnetometer`; `Compass`; `Barometer`; `Orientation`

`MAUI Sensor API → Platform sensor`

Support Engineer cần nhớ: `Sensor available?`; `Permission?`; `Hardware exists?`; `Sensor started?`; `Event subscription?`

Nếu sensor không hoạt động: `API → permission → hardware → platform implementation`

# 11. Connectivity

Connectivity xác định network state.

`App → Connectivity → Network access → Connection profiles`

Không nên hiểu: `NetworkAccess == Internet guaranteed`

Có connection không đảm bảo request tới server thành công.

Troubleshooting: `Connectivity says Internet → HTTP request fails → Check DNS TLS server proxy authentication timeout`

# 12. Permissions

Permissions là trọng tâm của mobile support.

`Feature → Requires permission? → Check → Request → User decision → Granted / Denied`

`Camera`; `Location`; `Microphone`; `Photos`; `Contacts`

Không nên chỉ: `Request permission → assume granted`

Phải xử lý: `Granted`; `Denied`; `Restricted / unavailable`

# 13. Permission Troubleshooting

Feature chạy trên device này nhưng fail trên device khác: `1. Permission required?`; `2. Permission declared?`; `3. Permission requested?`; `4. User denied?`; `5. Permission revoked?`; `6. OS version changed behavior?`; `7. Platform-specific configuration missing?`

Đặc biệt với Android: `Manifest/configuration` + `Runtime permission`

có thể đều liên quan.

# 14. Preferences

Preferences phù hợp với: `small key-value settings`

`Theme = Dark`; `Language = vi`; `FirstRun = false`

`Preferences → Key / Value`

Không dùng Preferences để lưu: `large database`; `complex relational data`; `large files`

# 15. Secure Storage

Khi dữ liệu nhạy cảm hơn: `token`; `credential`; `small secret`

dùng Secure Storage, không dùng Preferences.

- Normal setting → Preferences.
- Sensitive small value → SecureStorage.

Không dùng Secure Storage như database.

# 16. File System

File system APIs dùng cho: `files`; `cache`; `app data`; `temporary data`

`App → FileSystem → App-specific storage`

Support Engineer phải phân biệt: `App storage ≠ user-visible public storage`

Android/Windows khác storage/security model.

Nếu file path hoạt động trên Windows nhưng fail Android: `Don't assume path syntax is portable.`

# 17. Clipboard

Clipboard là platform feature có cross-platform abstraction.

Use cases: `Copy`; `Paste`; `Share text`

Troubleshooting: `Clipboard API → Platform support → Permissions/security → Foreground/background state`

Không giả định clipboard hoạt động giống nhau trên mọi OS.

# 18. Browser / Launcher

Các APIs như Browser và Launcher giúp mở: `HTTP/HTTPS URL`; `other apps`; `platform-supported URI`

`MAUI API → OS launcher → browser / app`

Nếu: `Browser.OpenAsync(...)`

fail: `1. URI valid?`; `2. Scheme supported?`; `3. OS has handler?`; `4. App state?`; `5. Platform-specific restriction?`

# 19. Share

Share API chuyển dữ liệu sang cơ chế chia sẻ của OS.

`MAUI App → Share → OS Share UI → Other app`

Đây là ví dụ điển hình của: `Cross-platform intent` + `platform implementation`

# 20. Phone / SMS / Email

Các API này nối app với khả năng native của hệ thống.

`App → MAUI API → OS capability → External application/service`

`PhoneDialer`; `Sms`; `Email`

Support Engineer cần nhớ:

> "API call succeeded" không luôn đồng nghĩa "user successfully completed the external action".

# 21. Media / Camera / Photos

Media features thường có dependency chain: `API → Permission → OS service → Hardware / provider → Result`

Nếu camera fail: `Camera available?`; `Permission?`; `OS configuration?`; `Device?`

Nếu photo picker fail: `Permission model?`; `Picker support?`; `OS version?`

# 22. Geolocation

Geolocation là ví dụ điển hình của feature phụ thuộc: `Permission` + `OS service` + `Hardware/network`

`Request location → Permission → Location service enabled? → Provider available? → Location result`

Debug geolocation phải kiểm tra cả code lẫn OS settings.

# 23. Sensors và lifecycle

Sensor subscriptions phải xét lifecycle.

Sai: `Start sensor → Never stop`

Có thể dẫn đến: `battery drain`; `callbacks after page closed`; `duplicate events`

Pattern: `Page appears → Start`

`Page disappears → Stop`

Chiến lược lifecycle cụ thể tùy architecture/feature.

# 24. MainThread

Một số platform callbacks có thể chạy ngoài UI thread.

`Native callback → background thread → UI update? → MainThread`

Nếu cập nhật UI từ sai thread: `cross-thread exception`

hoặc undefined/unexpected behavior có thể xảy ra.

Dùng MainThread để marshal UI work về UI thread khi cần.

# 25. Platform Helpers

MAUI có `Platform` helpers truy cập một số context/lifecycle riêng platform.

Ví dụ trên Android có các helper liên quan: `CurrentActivity`; `AppContext`; `Intent`; `ActivityStateChanged`

Theo Microsoft Learn, `Platform` trong `Microsoft.Maui.ApplicationModel` cung cấp helper từng platform. citeturn0search14

### Cảnh báo

Đây là low-level layer.

Chỉ xuống đây khi: `Cross-platform API không đủ`

# 26. Android App Links

Android App Links là use case platform-specific quan trọng.

Mục tiêu: `https://example.com/product/123 → Android OS → Your MAUI app → Product 123`

Android xử lý app links qua Intent system.

Microsoft mô tả flow: `1. Verify domain ownership`; `2. Host digital asset links file`; `3. Configure intent filter`; `4. Read incoming intent`; `5. Test`

citeturn0search6

# 27. Android App Links — Support Engineer View

Nếu link không mở app: `Browser → HTTPS URL → Android Intent → Intent Filter → Digital Asset Links → App`

Debug từng layer:

- [ ] URL correct
- [ ] Domain ownership verified
- [ ] assetlinks.json reachable
- [ ] package name correct
- [ ] certificate fingerprint correct
- [ ] intent filter correct
- [ ] app installed
- [ ] OS association verified

Đây là một ví dụ tốt cho tư duy:

> Platform feature gồm nhiều configuration layers, không chỉ code.

# 28. iOS Platform Features

iOS có requirement riêng platform.

`Capabilities`; `Entitlements`; `Provisioning`; `Signing`; `Apple Developer configuration`

Theo Microsoft, iOS capabilities mở rộng quyền/khả năng app, nằm trong provisioning profile và dùng khi code signing. citeturn0search0

Vì vậy: `iOS feature fails`

không nhất thiết là C# code bug.

Có thể là: `Apple Developer configuration` + `Entitlement` + `Provisioning` + `Signing`

# 29. iOS vs Android Permission Mental Model

Không cần học toàn bộ chi tiết ngay.

Nhớ:

- Android: Manifest + runtime permission + OS behavior.
- iOS: Entitlements/capabilities + Info.plist + permission/user consent.

Chi tiết thực tế phụ thuộc feature.

# 30. Platform Configuration

Một platform feature có thể cần cấu hình nhiều nơi.

`C# API` + `MauiProgram` + `Platforms/*` + `Manifest / Info.plist / Windows config` + `OS settings`

Khi support issue xảy ra:

> Đừng chỉ đọc stack trace của C#.

Hãy kiểm tra configuration layer.

# 31. `Platforms/Android`

Android-specific implementation thường nằm trong: `Platforms/Android`

Các loại file thường gặp: `MainActivity`; `MainApplication`; `AndroidManifest`; `Resources`

Đặt Android-specific startup/configuration tại đây.

Không đặt toàn bộ business logic tại đây.

# 32. `Platforms/Windows`

Windows-specific implementation nằm trong: `Platforms/Windows`

`MAUI → WinUI / Windows platform layer`

Khi Windows-only issue: `Shared code → Platforms/Windows → Windows API`

# 33. Platform-specific UI

Có 3 cách phổ biến: `1. OnPlatform / OnIdiom`; `2. Platform-specific APIs`; `3. Handler customization`

Decision:

### Chỉ khác visual value

`OnPlatform`

`Padding Android = 12`; `Padding Windows = 16`

### Có API platform-specific

`Platform-specific API`

### Cần native control customization

`Handler`

# 34. Platform-specific UI vs Handler

> "Android Entry cần IME option khác."

Trước tiên: `Android platform-specific API`

Microsoft có sẵn Android platform-specific API cho Entry IME options. citeturn0search5

Không nên lập tức: `EntryHandler.Mapper`

Nguyên tắc: `Built-in platform-specific API`; `>`; `Handler customization`

khi API đáp ứng requirement.

# 35. Platform-specific API vs `#if`

Nếu chỉ có: `small compile-time difference`

có thể dùng:

```csharp
#if ANDROID
#elif WINDOWS
#endif
```

Nếu logic lớn: `IPlatformService`

Nếu MAUI đã có abstraction: `Use MAUI abstraction`

Decision tree: `MAUI API exists? → YES → Use it`

`NO → Platform-specific API? → YES → Use it`

`NO → Small difference? → YES → #if`

`NO → Platform service`

`Native control? → Handler`

# 36. Platform Detection vs Capability Detection

Không nên viết:

```csharp
if (DeviceInfo.Platform == DevicePlatform.Android)
{
    // assume feature exists
}
```

rồi coi đó là capability check.

Platform: `Which OS?`

Capability: `Can this operation be performed?`

`Android`

không đồng nghĩa: `Camera definitely available`

hoặc: `GPS definitely enabled`

# 37. Common Platform Bug Categories

| Symptom | Layer cần kiểm tra |
|---|---|
| Feature không tồn tại trên OS | Platform API |
| Permission denied | Permission |
| Native control khác nhau | Handler / platform UI |
| Deep link không mở app | Intent / configuration |
| File path lỗi | FileSystem / OS storage |
| Sensor không hoạt động | Sensor / permission / hardware |
| Network request fail | Connectivity / HTTP / OS |
| iOS feature bị reject/fail | Capability / signing |
| UI update crash | MainThread |
| Android startup issue | `Platforms/Android` |
| Windows-only behavior | `Platforms/Windows` |

# 38. Support Engineer Troubleshooting Flow

Khi ticket nói:

> "It works on Windows but doesn't work on Android."

Không bắt đầu bằng: `Rewrite code`

Dùng flow: `1. Reproduce on both platforms → 2. Identify feature category → 3. Check MAUI cross-platform API → 4. Check permission → 5. Check platform-specific API → 6. Check platform configuration → 7. Check Platforms/Android → 8. Check native OS behavior → 9. Isolate minimal reproduction`

# 39. Support Ticket Example — Permission

### Ticket

> "Camera works on Windows but crashes on Android."

Investigation: `Camera API → Android permission → Manifest → Runtime permission → Camera service → Device`

Potential root causes: `permission not declared`; `permission not requested`; `user denied`; `permission revoked`; `camera unavailable`; `platform configuration incorrect`

Không kết luận chỉ từ exception message.

# 40. Support Ticket Example — Storage

### Ticket

> "File path works on Windows but not Android."

Investigation: `Path → FileSystem API → App storage → Android storage model`

Check: `Is path app-specific?`; `Is directory created?`; `Does app have access?`; `Is path hardcoded?`; `Does API return a platform-safe location?`

Không hardcode path kiểu Windows trong shared code.

# 41. Support Ticket Example — Deep Link

### Ticket

> "Clicking our website URL doesn't open the Android app."

Investigation: `HTTPS URL → Intent filter → Digital Asset Links → Domain verification → Package/certificate → Android app`

Mỗi layer phải pass độc lập.

# 42. Broken Lab #1 — Wrong Layer

### Scenario

Developer muốn sửa native Android control dù behavior đã có MAUI platform-specific API.

### Mistake

`Handler Mapper`

### Expected fix

`Check platform-specific MAUI API first.`

### Pass

Giải thích được: `Platform-specific API`; `>`; `Handler`

khi API đã đáp ứng requirement.

# 43. Broken Lab #2 — Permission

### Scenario

Feature: `Camera`

Code chạy nhưng Android trả về failure.

### Investigation

`API → Permission → Manifest → Runtime request → User decision`

### Pass

Không sửa ViewModel trước khi xác minh permission layer.

# 44. Broken Lab #3 — Hardcoded Path

### Scenario

Code:

```csharp
var path = @"C:\MyApp\data.json";
```

### Symptom

`Windows → PASS`; `Android → FAIL`

### Root cause

Shared code chứa platform-specific path.

### Fix direction

Dùng: `FileSystem`

hoặc abstraction/service phù hợp.

# 45. Broken Lab #4 — UI Thread

### Scenario

Native callback cập nhật: `Label.Text`

trực tiếp.

### Symptom

Có thể xảy ra: `cross-thread access`

### Investigation

`Native callback → Which thread? → MainThread? → UI update`

# 46. Broken Lab #5 — Android App Link

### Scenario

`https://example.com/product/42`

không mở app.

### Investigation checklist

- [ ] URL
- [ ] intent filter
- [ ] domain
- [ ] assetlinks.json
- [ ] package ID
- [ ] certificate fingerprint
- [ ] app installation
- [ ] Android association

### Pass

Xác định được lỗi nằm ở configuration layer thay vì C# navigation.

# 47. Broken Lab #6 — Platform Service

### Scenario

ViewModel chứa:

```csharp
#if ANDROID
    // 100 lines native code
#elif WINDOWS
    // 100 lines native code
#endif
```

### Problem

Platform logic trộn lẫn application logic.

### Refactor

`ViewModel → IPlatformService → AndroidPlatformService WindowsPlatformService`

### Pass

ViewModel không còn biết native implementation.

# 48. Mini Project — Platform Support Lab

Tạo page:

```text
Platform Lab
────────────────────────
Platform:
[ Windows / Android ]
Device:
[ ... ]
Connectivity:
[ ... ]
Permission:
[ ... ]
Storage:
[ ... ]
Platform Service:
[ Available ]
Native integration:
[ Test ]
```

Mục tiêu: `Shared UI → Cross-platform APIs → Platform service → Platform implementation`

# 49. 80/20 Learning Order

## Level 1 — Must Know

`1. Single Project`; `2. Platforms folder`; `3. Cross-platform API`; `4. Platform-specific API`; `5. Permissions`; `6. DeviceInfo`; `7. Connectivity`; `8. FileSystem`; `9. Preferences`; `10. SecureStorage`

## Level 2 — Support Engineer

`11. #if ANDROID / WINDOWS`; `12. Platform services`; `13. DI + platform implementation`; `14. MainThread`; `15. Sensors`; `16. Browser / Launcher / Share`; `17. Media / Camera`; `18. Geolocation`

## Level 3 — Platform Integration

`19. Android intents`; `20. Android App Links`; `21. Android Manifest`; `22. iOS capabilities`; `23. Entitlements`; `24. Platform helpers`; `25. Native APIs`; `26. Handler integration`

# 50. Những thứ chưa cần học sâu

80/20 chưa cần học thuộc: toàn bộ Android APIs; toàn bộ Win32/WinUI APIs; toàn bộ iOS APIs; mọi permission; mọi Android Intent; mọi iOS capability; toàn bộ native lifecycle; platform internals.

Thay vào đó:

> Biết **feature nằm ở layer nào** và biết **tra đúng Microsoft Learn page**.

# 51. Cheat Sheet

## Architecture

`Shared MAUI → Platform API → Platform implementation → Native OS`

## Small platform difference

```csharp
#if ANDROID
#elif WINDOWS
#endif
```

## Large platform difference

`IPlatformService → Android implementation Windows implementation`

## Device

```csharp
DeviceInfo.Current
```

## Connectivity

`Connectivity.Current`

## Permissions

`Permissions`

## Storage

`FileSystem`; `Preferences`; `SecureStorage`

## Native UI

`Handler`

## Android deep linking

`Intent` + `Intent Filter` + `Digital Asset Links`

## iOS capabilities

`Capability` + `Entitlement` + `Provisioning` + `Signing`

# 52. Platform Debugging Matrix

| Question | Check |
|---|---|
| Is API supported? | MAUI docs |
| Is platform supported? | Supported platforms |
| Is permission needed? | Permissions |
| Is device feature available? | Device / hardware |
| Is configuration needed? | Manifest / Info.plist / platform config |
| Is native UI involved? | Handler |
| Is callback on UI thread? | MainThread |
| Is behavior OS-specific? | Platform-specific docs |
| Is logic too large for `#if`? | Platform service |
| Is deep link failing? | Intent/filter/domain association |

# 53. Platform Integration Decision Tree

```text
Requirement
    ▼
Is there a MAUI cross-platform API?
 ┌──┴──┐
YES    NO
 ▼      ▼
Use    Is there a platform-specific
MAUI   MAUI API?
API       │
       ┌──┴──┐
      YES    NO
       ▼      ▼
      Use   Is native code required?
           ┌──┴──┐
          NO     YES
           ▼      ▼
       Platform   Handler /
       service    native API
```

# 54. Definition of Done

Hoàn thành Platform checkpoint khi làm được:

- [ ] Giải thích Platform Integration.
- [ ] Phân biệt cross-platform API và platform-specific API.
- [ ] Biết vai trò của `Platforms/Android`.
- [ ] Biết vai trò của `Platforms/Windows`.
- [ ] Dùng được `#if ANDROID` / `#if WINDOWS` đúng chỗ.
- [ ] Biết khi nào cần Platform Service.
- [ ] Kết hợp DI với platform-specific implementation.
- [ ] Debug được permission issue.
- [ ] Debug được storage/path issue.
- [ ] Debug được connectivity issue.
- [ ] Hiểu `MainThread`.
- [ ] Biết dùng DeviceInfo.
- [ ] Biết nhóm API của Sensors/Geolocation/Media.
- [ ] Phân biệt Platform-specific API với Handler.
- [ ] Hiểu Android Intent/App Links ở mức troubleshooting.
- [ ] Hiểu iOS capabilities ở mức architecture/troubleshooting.
- [ ] Trace được feature từ MAUI API → platform layer → native OS.
- [ ] Hoàn thành ít nhất 4 Broken Labs.

# 55. Final Mental Model

Đừng nhớ Platform Integration như một danh sách API.

Hãy nhớ:

```text
                 MAUI APP
                    ▼
          Cross-platform API
          ┌─────────┴─────────┐
          ▼                   ▼
 Platform-specific API     Shared abstraction
          ▼                   ▼
      Platform code        DI Service
          └─────────┬─────────┘
                    ▼
              Native platform
          ┌─────────┼─────────┐
          ▼         ▼         ▼
       Android    Windows     iOS
       Manifest   WinUI     Capability
       Intent     Config    Entitlement
       Runtime              Signing
       Permission
```

Gặp platform issue, Support Engineer không nên hỏi trước:

> "Sửa dòng code nào?"

Mà là:

> "Issue này đang nằm ở **shared layer, MAUI platform layer, configuration layer hay native OS layer**?"

Đó là mô hình cốt lõi của Platform Integration.
