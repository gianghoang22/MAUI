# .NET MAUI Handlers — 80/20 Guide

> Mục tiêu: hiểu **Handlers .NET MAUI 10** để debug UI/platform issues, customize native controls và chọn Mapper hoặc Custom Handler đúng lúc.
>
> Tài liệu gốc Microsoft Learn: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/?view=net-maui-10.0
>
> Định hướng **Support Engineer / 80/20**, ưu tiên Windows + Android.

## 1. Handler là gì?

MAUI dùng UI cross-platform, bên dưới mỗi platform vẫn là native control.

```text
.NET MAUI Control
      ▼
Virtual View / Interface
      ▼
   Handler
      ├── Property Mapper
      ├── Command Mapper
      └── Lifecycle
      ▼
Native View
      ├── Android
      ├── Windows
      ├── iOS
      └── Mac Catalyst
```

```text
MAUI Button
   ▼
IButton
   ▼
ButtonHandler
   ├── Android → MaterialButton
   └── Windows → native Windows control
```

Hai khái niệm cốt lõi:

```csharp
handler.VirtualView
handler.PlatformView
```

### `VirtualView`

Control phía .NET MAUI.

```csharp
IButton
IEntry
ILabel
```

### `PlatformView`

Native control trên platform hiện tại.

`Android → Android native View`; `Windows → Windows native control`

> `VirtualView` = MAUI side  
> `PlatformView` = native side

Theo Microsoft, Handler nối abstract/cross-platform controls với native controls. citeturn0search0

# 2. Vì sao MAUI cần Handler?

Trước đây Xamarin.Forms sử dụng **Renderers**.

Mental model cũ: `MAUI/Xamarin Control → Renderer → Native Control`

Handler architecture: `MAUI Control → Interface → Handler → Native Control`

Handler giúp: giảm coupling giữa cross-platform control và native implementation; map properties từ MAUI → native; map commands → native; customize native controls; tạo custom cross-platform controls; xử lý platform-specific behavior.

80/20 chưa cần đào sâu lịch sử renderer.

Điều quan trọng cho Support Engineer:

> UI behavior khác Windows/Android: kiểm tra Handler trong những layer đầu tiên.

# 3. Ba thứ phải hiểu trước

Ba điểm cốt lõi: `1. Handler`; `2. Mapper`; `3. Handler Lifecycle`

Chi tiết:

- Handler: bridge giữa MAUI control và native control.
- Mapper: MAUI property/command → native implementation.
- Lifecycle: khi native handler được tạo / thay đổi / tháo ra.

# 4. Property Mapper

Property Mapper quyết định:

> MAUI property đổi thì native control phải làm gì?

`MAUI Property → Property Mapper → Action → Native Property/API`

`Button.Text → ButtonHandler.Mapper → native button text`

Mapper là dictionary ánh xạ property → action.

Ví dụ custom mapping:

```csharp
EntryHandler.Mapper.AppendToMapping(
    "MyCustomMapping",
    (handler, view) =>
    {
        // customize native control
    });
```

Microsoft hiện hỗ trợ các cách: `PrependToMapping`; `ModifyMapping`; `AppendToMapping`

để customize handler mapper. citeturn0search3

# 5. Prepend / Modify / Append

Ba API cần phân biệt:

## `PrependToMapping`

Chạy custom mapping trước mapping mặc định.

`Custom → Default MAUI mapping`

Dùng khi: cần chuẩn bị native state; muốn custom logic chạy trước mapping mặc định.

## `ModifyMapping`

Thay đổi behavior của một mapping hiện có.

`Existing Mapping → Modified Mapping`

Dùng khi: cần thay đổi implementation hiện tại; cần can thiệp trực tiếp vào mapping có sẵn.

Cẩn thận: thay đổi này tác động behavior mặc định.

## `AppendToMapping`

Chạy custom mapping sau mapping mặc định.

`Default MAUI mapping → Custom mapping`

Thường dễ bắt đầu nhất khi chỉ bổ sung behavior.

# 6. Handler customization là global

Đây là điểm rất quan trọng khi debug.

Nếu bạn làm:

```csharp
EntryHandler.Mapper.AppendToMapping(...);
```

mapping áp dụng cho **Entry handler**, không chỉ một Entry.

`{Entry #1; Entry #2; Entry #3} → EntryHandler.Mapper`

Do đó:

> Customize Handler có thể ảnh hưởng mọi control cùng loại trong app.

Microsoft cũng lưu ý rằng handler customization là global. citeturn0search0turn0search3

### Support Engineer implication

Nếu một `Entry` tự nhiên có behavior lạ: `Không chỉ kiểm tra XAML của Entry.`

`Kiểm tra: → EntryHandler.Mapper → MauiProgram.cs → platform-specific code`

# 7. Command Mapper

Property Mapper xử lý: `Property changed`

Command Mapper xử lý: `Command / instruction → native view`

Ví dụ Microsoft dùng: `ScrollView → ScrollViewHandler → CommandMapper → native scroll operation`

Một command có thể truyền thêm data.

`ScrollTo` + `Scroll position`

Khác với `ICommand` trong MVVM.

### Cực kỳ quan trọng

Hai loại "command" khác nghĩa: `MVVM ICommand ≠ Handler Command Mapper command`

Handler command gửi instruction xuống native view, không phải ViewModel command.

# 8. Handler Lifecycle

Có hai lifecycle event quan trọng: `HandlerChanging`; `HandlerChanged`

## `HandlerChanging`

Xảy ra khi: handler sắp được tạo; handler hiện tại sắp bị remove.

Có:

```csharp
NewHandler
OldHandler
```

`Old native handler → HandlerChanging`:
- `OldHandler != null` → cleanup.
- `NewHandler != null` → new handler.

Khi `OldHandler != null`, cần: unsubscribe native events; cleanup native resources; tránh giữ reference cũ.

# 9. `HandlerChanged`

`HandlerChanged` xảy ra sau khi tạo handler; native control đã tồn tại.

`MAUI Control → Create Handler → HandlerChanged → PlatformView available`

Thường dùng để native initialization cần `PlatformView`.

```csharp
myEntry.HandlerChanged += (sender, args) =>
{
    var entry = (Entry)sender;

    var platformView = entry.Handler?.PlatformView;

    if (platformView == null)
        return;

    // native customization
};
```

Microsoft xác nhận `HandlerChanged` xảy ra sau khi handler được tạo và native control đã available. citeturn0search0

# 10. Thứ tự lifecycle

Nhớ: `HandlerChanging → HandlerChanged`

Nếu handler bị remove: `HandlerChanging → cleanup old native events`

Nếu handler mới được tạo: `HandlerChanging → HandlerChanged → PlatformView available`

Ngoài event, MAUI controls còn có:

```csharp
OnHandlerChanging()
OnHandlerChanged()
```

có thể override trong custom control.

# 11. Khi nào dùng Handler?

Một quy tắc thực tế: `Can solve with normal MAUI API? YES → Use normal MAUI API`

Nếu không: `Need native customization? YES → Use Handler Mapper`

Nếu cần: `Completely new cross-platform control → Create Custom Handler`

# 12. Decision Tree

```text
UI problem
   ├── MAUI property/API đã hỗ trợ?
   │       YES
   │        ↓
   │   Dùng MAUI API
   ├── Chỉ cần native customization?
   │       YES
   │        ↓
   │   Handler Mapper
   ├── Cần access PlatformView?
   │       YES
   │        ↓
   │   Handler lifecycle / Mapper
   └── Control hoàn toàn mới?
           YES
            ↓
       Custom Handler
```

# 13. Handler Mapper vs Behavior vs ViewModel

Đây là một điểm rất quan trọng trong kiến trúc MAUI.

| Problem | Nên dùng |
|---|---|
| Business logic | ViewModel |
| UI reusable logic | Behavior |
| Cross-platform visual property | MAUI property/style |
| Native platform customization | Handler |
| Native API access | Handler |
| Completely new native-backed control | Custom Handler |

### Sai hướng

`Login validation → EntryHandler`

Không nên.

Đây là business/UI behavior.

Nên: `View → ViewModel → Validation`

### Đúng hướng

`Entry → Handler → Android native EditText behavior`

Nếu vấn đề thực sự chỉ tồn tại ở native control.

# 14. Windows + Android: tư duy platform-specific

Vì bạn đang học Windows + Android trước, hãy nghĩ:

```text
MAUI API
   ▼
Handler
   ├── Windows
   └── Android
```

Cùng một:

```xml
<Entry />
```

có thể đi qua handler nhưng native implementation phía dưới khác nhau.

Do đó bug có thể có dạng: `Windows PASS`; `Android FAIL`

hoặc: `Android PASS`; `Windows FAIL`

Không nên lập tức kết luận XAML sai.

Hãy hỏi: `Cross-platform layer? → Handler? → Platform implementation?`

# 15. Ví dụ: customize Entry

```csharp
EntryHandler.Mapper.AppendToMapping(
    "CustomEntry",
    (handler, view) =>
    {
#if ANDROID
        // Android native customization
#endif

#if WINDOWS
        // Windows native customization
#endif
    });
```

Pattern: `MauiProgram → EntryHandler.Mapper → Platform-specific native API`

### Lưu ý

Nếu mapper được đăng ký global: `Entry A`; `Entry B`; `Entry C`

đều có thể bị ảnh hưởng.

Do đó cần tránh:

```csharp
EntryHandler.Mapper.AppendToMapping(
    "RandomFix",
    ...
);
```

mà không hiểu phạm vi ảnh hưởng.

# 16. Conditional platform code

Handler customization thường cần platform-specific code.

```csharp
#if ANDROID

#endif

#if WINDOWS

#endif
```

Nhưng nên giữ architecture rõ: `Shared → {Handler registration; Shared configuration}`

`Platforms → {Android → {Native implementation}; Windows → {Native implementation}}`

Tránh nhét nhiều native code vào ViewModel/Page.

# 17. `PlatformView` và `VirtualView`

Cần nắm chắc hai property này.

`handler`:
- VirtualView: MAUI control.
- PlatformView: native control.

Ví dụ debug:

```csharp
var handler = entry.Handler;

var virtualView = handler?.VirtualView;
var platformView = handler?.PlatformView;
```

Nếu: `VirtualView != null`; `PlatformView == null`

có thể do truy cập quá sớm/handler chưa được tạo.

Nếu: `PlatformView != null`

thì native control đã sẵn sàng cho platform-specific work.

# 18. Handler registration

Phải đăng ký Custom Handler với MAUI app.

```csharp
builder.ConfigureMauiHandlers(handlers =>
{
    handlers.AddHandler<MyControl, MyControlHandler>();
});
```

`MyControl → MyControlHandler → native control`

Nếu quên registration: `Custom Control → Handler not found → runtime problem`

# 19. Custom Handler architecture

Một custom control có thể có cấu trúc:

```text
MyControl
    ▼
IMyControl
    ▼
MyControlHandler
    ├── Property Mapper
    ├── Command Mapper
    └── Platform View
            ├── Android
            └── Windows
```

Microsoft's custom-handler documentation follows essentially this flow: create cross-platform control → handler → property mapper → command mapper → platform controls → register handler → consume control → disconnect/cleanup. citeturn0search4

# 20. Disconnect / cleanup

Không cleanup native events có thể gây memory leak/stale references.

`HandlerChanging → OldHandler != null → unsubscribe events → cleanup references`

`Subscribe native event → HandlerChanged → use native control → HandlerChanging → unsubscribe → cleanup`

Đặc biệt kiểm tra lifecycle nếu: page mở/đóng nhiều lần; handler được recreated; event callback chạy nhiều lần; memory tăng dần; duplicate event callbacks xuất hiện.

# 21. Các dấu hiệu bug liên quan Handler

## Symptom 1: Chỉ một platform bị lỗi

`Windows PASS`; `Android FAIL`

Kiểm tra: `Handler → Mapper → Platform implementation`

## Symptom 2: Tất cả Entry đều bị ảnh hưởng

`Entry #1`; `Entry #2`; `Entry #3`

đều có behavior lạ.

Kiểm tra: `EntryHandler.Mapper`

vì mapper customization có phạm vi global.

## Symptom 3: Native API bị null

`handler.PlatformView == null`

Kiểm tra timing: `HandlerChanged`

Có thể code chạy trước khi native view sẵn sàng.

## Symptom 4: Event chạy nhiều lần

`Expected: 1 callback`; `Actual: 3 callbacks`

Kiểm tra: `HandlerChanged → event subscription`

và: `HandlerChanging → unsubscribe`

## Symptom 5: Native customization không có tác dụng

Kiểm tra theo thứ tự: `1. Handler có tồn tại?`; `2. Mapper có được register?`; `3. Key mapping có đúng?`; `4. Platform code có compile?`; `5. PlatformView có available?`; `6. Native property có bị MAUI mapping ghi đè?`; `7. Mapping có bị register nhiều lần?`

# 22. Support Engineer Troubleshooting Flow

Khi gặp:

> "Control hoạt động trên Android nhưng không hoạt động trên Windows."

Đừng sửa XAML ngay.

Dùng flow: `1. Reproduce → 2. Confirm platform → 3. Check normal MAUI API → 4. Check Handler → 5. Check Mapper → 6. Check PlatformView → 7. Check platform-specific code → 8. Check lifecycle → 9. Isolate minimal control → 10. Compare native behavior`

# 23. Handler Debug Checklist

Khi debug Handler:

- [ ] Control có Handler?
- [ ] VirtualView đúng?
- [ ] PlatformView đã available?
- [ ] Mapper có bị customize?
- [ ] Mapper registration nằm ở đâu?
- [ ] Mapping chạy trước hay sau default mapping?
- [ ] Có ModifyMapping không?
- [ ] Có global customization không?
- [ ] Có #if ANDROID / WINDOWS không?
- [ ] Có native event subscription không?
- [ ] Có cleanup ở HandlerChanging không?
- [ ] Handler có bị recreate không?

# 24. Mapper debugging

Nếu dùng:

```csharp
AppendToMapping(...)
```

hãy kiểm tra: `MauiProgram.cs → ConfigureMauiHandlers → Mapper registration → Mapping callback → PlatformView`

Một cách debug đơn giản:

```csharp
EntryHandler.Mapper.AppendToMapping(
    "DebugEntry",
    (handler, view) =>
    {
        System.Diagnostics.Debug.WriteLine(
            $"Handler: {handler.GetType().Name}");

        System.Diagnostics.Debug.WriteLine(
            $"View: {view.GetType().Name}");
    });
```

Xác nhận mapping thực sự chạy.

# 25. Property Mapper vs Command Mapper

| | Property Mapper | Command Mapper |
|---|---|---|
| Trigger | Property change | Command/instruction |
| Data | Property state | Có thể truyền data |
| Ví dụ | Text, Color | ScrollTo |
| Mục tiêu | Update native property | Instruct native view |
| MVVM `ICommand`? | Không | Không |

> Handler Command Mapper không phải MVVM Command.

# 26. Handler vs Renderer

Nếu đọc code/documentation cũ: `Renderer`

thì đừng nhầm với architecture hiện tại.

Mental mapping:

- Old Xamarin.Forms → Renderer.
- .NET MAUI → Handler.

Khi xử lý issue hiện tại, ưu tiên tìm: `Handler`; `Mapper`; `PlatformView`; `VirtualView`

# 27. 80/20 Learning Order

Học theo thứ tự:

### Level 1 — Must Know

`1. Handler là gì`; `2. VirtualView`; `3. PlatformView`; `4. Property Mapper`; `5. HandlerChanged`; `6. HandlerChanging`

### Level 2 — Support Engineer

`7. AppendToMapping`; `8. PrependToMapping`; `9. ModifyMapping`; `10. Command Mapper`; `11. Global customization`; `12. Platform-specific handler code`

### Level 3 — Advanced

`13. Custom Handler`; `14. Custom control interface`; `15. Platform control`; `16. Handler registration`; `17. Native event lifecycle`; `18. Cleanup / disconnect`

# 28. Những thứ chưa cần học sâu

Ban đầu chưa cần nhớ hết: tất cả handler classes; tất cả mapper keys; implementation nội bộ của từng platform; source code toàn bộ MAUI handler; mọi native API của Android/Windows; mọi custom control pattern.

Thay vào đó, phải biết cách **tra cứu đúng layer**.

# 29. Cheat Sheet

## Handler

`MAUI Control → Interface → Handler → Native View`

## Native access

```csharp
handler.VirtualView
handler.PlatformView
```

## Mapper

```csharp
Handler.Mapper.AppendToMapping(...)
Handler.Mapper.PrependToMapping(...)
Handler.Mapper.ModifyMapping(...)
```

## Lifecycle

`HandlerChanging → HandlerChanged`

## Cleanup

`HandlerChanging → OldHandler != null → unsubscribe / cleanup`

## Custom Handler

`Control → Handler → Mapper → Platform View → Register`

# 30. Broken Lab #1 — Global Entry customization

### Goal

Chứng minh Handler Mapper có thể ảnh hưởng toàn bộ control cùng loại.

### Setup

Tạo: `Page → {Entry A; Entry B; Entry C}`

Sau đó đăng ký global mapping.

### Expected investigation

`Why did all Entry controls change? → EntryHandler.Mapper → Global handler customization`

### Pass

Bạn giải thích được: `Mapper customization ≠ one specific Entry`

# 31. Broken Lab #2 — PlatformView too early

### Setup

Cố truy cập:

```csharp
entry.Handler?.PlatformView
```

trước khi handler được tạo.

### Expected symptom

`PlatformView == null`

### Debug path

`Page creation → Handler not ready → PlatformView null`

### Fix direction

Dùng: `HandlerChanged`

để chạy native initialization khi handler đã available.

# 32. Broken Lab #3 — Duplicate native event

### Setup

Subscribe event mỗi lần: `HandlerChanged`

nhưng không unsubscribe.

### Symptom

`1st open → callback 1 time`; `2nd open → callback 2 times`; `3rd open → callback 3 times`

### Root cause

`native event subscription` + `missing cleanup`

### Fix

`HandlerChanging → unsubscribe`

# 33. Broken Lab #4 — Windows vs Android

### Scenario

`Entry behavior:`; `Android → PASS`; `Windows → FAIL`

### Investigation

`XAML → MAUI property → Handler → Mapper → PlatformView → Windows implementation`

### Pass condition

Không sửa ViewModel hoặc business logic nếu issue nằm ở native UI layer.

# 34. Broken Lab #5 — Wrong customization mechanism

### Scenario

Developer muốn đổi native visual behavior nhưng đặt code trong: `ViewModel`

### Expected correction

- Business behavior → ViewModel.
- Reusable UI behavior → Behavior.
- Native UI behavior → Handler.

# 35. Mini Project — Handler Support Lab

Tạo một page:

```text
Handler Lab
────────────────────
Entry
[________________]
Button
[ Test Handler ]
Status
Handler: ?
PlatformView: ?
Platform: ?
```

Khi nhấn: `Test Handler`

hiển thị: `VirtualView: Entry`; `Handler: EntryHandler`; `PlatformView: <native type>`; `Platform: Windows / Android`

Mục tiêu:

> Nhìn vào một MAUI control và biết cách lần từ cross-platform layer xuống native layer.

# 36. Support Engineer Scenario

### Ticket

> "The Entry looks different on Windows and Android."

### Không nên bắt đầu bằng

`Change XAML`

### Nên hỏi

`1. Is the difference expected platform behavior?`; `2. Does MAUI expose the property?`; `3. Is a Handler Mapper customized?`; `4. Is there platform-specific code?`; `5. What is PlatformView on each platform?`; `6. Is the native property being overwritten later?`

### Investigation tree

`Visual difference → {Expected MAUI behavior?; XAML/style?; Handler mapping?; Platform-specific implementation?; Native control behavior?}`

# 37. Khi nào Handler là "đúng tool"?

Dùng Handler khi requirement có ngôn ngữ kiểu: `"native"`; `"platform-specific"`; `"Android native"`; `"Windows native"`; `"access underlying control"`; `"change native behavior"`; `"customize native control"`

> "Tôi cần thay đổi native behavior của Entry trên Android."

→ Handler là ứng viên mạnh.

Ngược lại:

> "Tôi cần validate email."

→ ViewModel / validation.

> "Tôi muốn reuse hover behavior."

→ Behavior hoặc platform-specific UI mechanism tùy requirement.

# 38. Mental Model cuối cùng

Hãy nhớ đúng chuỗi này: `.NET MAUI → Cross-platform View → Interface {IButton; IEntry; ILabel} → Handler → {Property Mapper; Command Mapper} → PlatformView → {Android; Windows}`

Hiểu sơ đồ này là nắm phần lớn Handler concept cần cho Support Engineer.

# 39. Definition of Done

Hoàn thành Handler checkpoint khi làm được:

- [ ] Giải thích Handler bằng diagram.
- [ ] Phân biệt `VirtualView` và `PlatformView`.
- [ ] Giải thích Property Mapper.
- [ ] Giải thích Command Mapper.
- [ ] Phân biệt Handler Mapper với MVVM `ICommand`.
- [ ] Biết `HandlerChanging` và `HandlerChanged`.
- [ ] Biết lúc nào `PlatformView` available.
- [ ] Biết cleanup native events.
- [ ] Biết Handler customization có thể global.
- [ ] Biết `AppendToMapping`, `PrependToMapping`, `ModifyMapping`.
- [ ] Debug được issue chỉ xảy ra trên một platform.
- [ ] Biết khi nào dùng Handler thay vì Behavior/ViewModel.
- [ ] Tạo được một basic custom Handler.
- [ ] Register được custom Handler.
- [ ] Trace được một control từ MAUI → Handler → native view.
- [ ] Hoàn thành ít nhất 3 Broken Labs.

# 40. Official Microsoft Learn

- [Handlers](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/?view=net-maui-10.0)
- [Customize controls with handlers](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/customize?view=net-maui-10.0)
- [Create a custom control using handlers](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/create?view=net-maui-10.0)
- [Microsoft.Maui.Handlers namespace](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.handlers?view=net-maui-10.0)

> Lưu ý: trang bạn gửi đang ở `view=net-maui-9.0`, nhưng nội dung Microsoft Learn hiện có trang tương ứng cho `net-maui-10.0`. File này dùng MAUI 10 làm target học tập. citeturn0search0turn0search1
