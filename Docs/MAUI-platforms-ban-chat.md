# Hiểu Platforms trong .NET MAUI

> Tài liệu này tập trung vào ranh giới giữa ứng dụng MAUI dùng chung và từng nền tảng. Mục tiêu là hiểu target, build, cấu hình native, API thiết bị và cách chẩn đoán khác biệt; không phải học thuộc API Android hay Windows.
>
> Ví dụ trong repo dùng VocabMate: .NET MAUI 10 / .NET 10, targets Android và Windows. Source tree có bootstrap files iOS/Mac Catalyst, nhưng hai target Apple chưa được khai báo để build trong project hiện tại. Trong MAUI, ứng dụng desktop Apple dùng target **Mac Catalyst**; không nhầm nó với một target `macOS` độc lập.

## 1. “Platform” có mấy nghĩa?

Khi nói “platform”, người ta có thể đang nói tới những thứ khác nhau. Phân biệt chúng trước khi sửa project:

| Khái niệm | Ý nghĩa |
| --- | --- |
| Hệ điều hành đích | Hệ điều hành mà app sẽ chạy: Android, Windows, iOS, Mac Catalyst. |
| Target framework | Cấu hình build .NET cho một hệ điều hành/phiên bản đích, ví dụ `net10.0-android`. |
| Target device | Thiết bị hoặc emulator đang cài/chạy bản build: điện thoại, tablet, máy Windows. |
| Platform API | Khả năng do OS cung cấp: chọn file, quyền, thông báo, text-to-speech, cửa sổ. |
| Platform-specific code/config | Phần chỉ biên dịch hoặc được đọc trên một target, như manifest, plist, native code. |

Chọn Android emulator trong VS Code không tự thêm Android vào project; có target Android trong project cũng không đảm bảo emulator đã cài hoặc app đã chạy trên đó. Build target, môi trường build và thiết bị chạy là ba điều kiện khác nhau.

## 2. Multi-target nghĩa là build nhiều biến thể

MAUI cho phép một project định nghĩa nhiều target framework. Khi build, MSBuild xử lý project theo từng target: tập API hợp lệ, điều kiện biên dịch, tài nguyên nền tảng và đầu ra sẽ tương ứng với target đó.

```text
Một project / phần code dùng chung
       ├── Android TFM      → build Android
       ├── Windows TFM      → build Windows
       ├── iOS TFM          → build iOS (cần toolchain Apple)
       └── Mac Catalyst TFM → build app macOS (cần toolchain Apple)
```

Không có một gói cài đặt duy nhất tự chạy trên mọi OS. Mỗi bản được build cho target cụ thể. Code dùng chung giúp tránh nhân đôi nghiệp vụ, nhưng không xóa khác biệt runtime, input, quyền, giao diện native, vòng đời hoặc cách phát hành.

### Cấu hình thật trong repo

Project hiện khai báo `net10.0-android` và `net10.0-windows10.0.19041.0` trong `MauiApp1.csproj`. Cùng file đặt mức Android tối thiểu API 24 và Windows minimum version 10.0.17763.0. Đây là các quyết định của project, không phải tuyên bố rằng toàn bộ MAUI chỉ hỗ trợ đúng những mức đó.

Các folder `Platforms/Android`, `Platforms/Windows`, `Platforms/iOS`, `Platforms/MacCatalyst` có thể tồn tại do template hoặc lịch sử project. **Folder tồn tại không tự thêm target framework.** Hãy kiểm tra target frameworks trong `.csproj` để biết build nào thực sự nằm trong phạm vi project.

## 3. Mỗi platform đưa app vào MAUI như thế nào?

Điểm chung cuối cùng là mỗi bootstrap platform gọi `MauiProgram.CreateMauiApp()`. Điểm khác là OS bắt đầu ở entry point khác nhau. Đọc hai tầng riêng:

1. **Native bootstrap:** OS/host nào nhận lệnh mở app, và lớp platform nào được tạo?
2. **MAUI bootstrap:** lớp nào gọi factory chung, sau đó tạo `MauiApp1.App`, `Window`, Shell và Page?

```text
Android OS → MainApplication + MainActivity ─┐
Windows/WinUI → Platforms/Windows/App ────────┤
iOS/UIKit → Program.Main → AppDelegate ───────┼→ MauiProgram.CreateMauiApp()
Mac Catalyst/UIKit → Program.Main → AppDelegate┘       ↓
                                            MauiApp1.App
                                                 ↓
                                      App.CreateWindow() → AppShell → Page
```

| Nền tảng | Entry point/host | Cầu nối tới MAUI | Ý cần nhớ |
| --- | --- | --- | --- |
| Android | Android tạo process/application rồi mở Activity có `MainLauncher = true`. | `MainApplication : MauiApplication` override `CreateMauiApp()`; `MainActivity : MauiAppCompatActivity` là Activity hiển thị UI. | `MainApplication` dựng host/factory MAUI; `MainActivity` mới là Activity launcher. Chúng không phải cùng một vai trò. |
| Windows | WinUI kích hoạt lớp được khai báo ở `Platforms/Windows/App.xaml`. | `Platforms/Windows/App.xaml.cs` kế thừa `MauiWinUIApplication`, override `CreateMauiApp()`. | Lớp này là `MauiApp1.WinUI.App`, khác với `MauiApp1.App` dùng chung. WinUI/MAUI host nối native window với `Application.CreateWindow()`. |
| iOS | `Platforms/iOS/Program.cs` gọi `UIApplication.Main(...)`; UIKit khởi chạy delegate. | `Platforms/iOS/AppDelegate.cs` kế thừa `MauiUIApplicationDelegate`, override `CreateMauiApp()`. | `Program.Main` là managed entry point; `AppDelegate` là điểm lifecycle/bootstrap UIKit-MAUI. |
| Mac Catalyst | `Platforms/MacCatalyst/Program.cs` gọi `UIApplication.Main(...)`; UIKit/Catalyst khởi chạy delegate. | `Platforms/MacCatalyst/AppDelegate.cs` kế thừa `MauiUIApplicationDelegate`, override `CreateMauiApp()`. | Cấu trúc giống iOS nhưng target/runtime là Mac Catalyst; đây là cách MAUI chạy app trên macOS. |

### Android: Application không phải Activity

Trên Android, OS tạo `Application` cho process trước khi tạo Activity. `MainApplication` của MAUI cung cấp `MauiApp` qua `MauiProgram`; sau đó launcher Activity `MainActivity` được Android mở để đưa app ra màn hình. Trong VocabMate, attribute `MainLauncher = true` nằm trên `MainActivity`; `OnCreate` gọi `base.OnCreate()` để MAUI khởi tạo Activity rồi cấu hình resize bàn phím. Vì vậy, diễn đạt chính xác là: **MainApplication khởi tạo MAUI host, MainActivity là cửa vào UI do Android launch.**

### Windows: WinUI App khác MAUI App

Tên `App` bị trùng nhưng namespace/trách nhiệm khác nhau:

- `MauiApp1.WinUI.App` trong `Platforms/Windows` kế thừa `MauiWinUIApplication`; đây là platform bootstrap class mà WinUI kích hoạt.
- `MauiApp1.App` ở project gốc kế thừa MAUI `Application`; nó chứa cấu hình/tài nguyên chung và override `CreateWindow()`.
- `MauiProgram` là composition root chung, nơi đăng ký handler, font, service và page rồi trả về `MauiApp`.

Không đọc `Platforms/Windows/App.xaml` như thể đó là page đầu tiên của người dùng. XAML đó khai báo WinUI application host; nội dung MAUI hiển thị được tạo sau khi factory chạy và `MauiApp1.App` tạo Window chứa Shell.

### iOS và Mac Catalyst: UIKit khởi chạy delegate

Hai target có `Program.Main` gọi `UIApplication.Main` với loại `AppDelegate`. Delegate MAUI override factory giống `MainApplication` ở vai trò nối tới `MauiProgram`, nhưng không phải Android Application và không có `MainActivity`. Sau bootstrap, cả hai đi vào application model dùng chung của MAUI. Phần UIKit lifecycle, window/scene và cấu hình native vẫn thuộc từng target.

Source files tồn tại không đồng nghĩa chúng đang được compile: VocabMate hiện chỉ khai báo Android/Windows TFM trong `.csproj`. Muốn build iOS hoặc Mac Catalyst phải thêm target phù hợp, cài workload/toolchain và kiểm chứng cấu hình Apple; đặc biệt build target Apple cần môi trường macOS phù hợp.

Sau khi `MauiProgram.CreateMauiApp()` trả về, luồng shared tiếp tục qua `UseMauiApp<MauiApp1.App>()`, resolve application class, nạp resources và cuối cùng tạo MAUI `Window`/Shell/Page. Trình tự native callback trước điểm hội tụ có khác biệt; không nên giả định bốn OS gọi cùng một callback hoặc cùng một thứ tự lifecycle.

## 4. Ranh giới dùng chung và riêng nền tảng

Hãy đặt mỗi yêu cầu vào đúng một trong ba vùng:

| Vùng | Dùng khi | Ví dụ |
| --- | --- | --- |
| MAUI/.NET dùng chung | Hành vi và API đủ đáp ứng các target của app. | Binding, layout, validation, Shell navigation, repository, theme logic. |
| API đa nền tảng có điều kiện | Có API MAUI chung nhưng khả năng/cấu hình khác nhau theo OS. | File picker, permissions, connectivity, text-to-speech. |
| Native platform | Cần capability hoặc hành vi riêng mà API chung không cung cấp. | Android intent/configuration hoặc tùy biến native control. |

Ưu tiên tầng dùng chung nếu nó diễn đạt đúng yêu cầu. Đi xuống tầng native khi có bằng chứng cụ thể: thiếu API, thiếu cấu hình, khác biệt UX bắt buộc, hoặc cần tích hợp OS. Đừng tạo code riêng chỉ vì “app có nhiều platform”; cũng đừng ép mọi thứ vào shared code nếu các nền tảng thật sự khác nhau.

**Một phép thử tốt:** nếu quy tắc sản phẩm giống nhau ở Android và Windows thì giữ quy tắc đó ngoài code native. Nếu cách mở file, xin quyền hoặc nhận kết quả khác nhau, bọc phần khác biệt sau một service/adapter có hợp đồng chung.

## 5. API chung không có nghĩa hành vi/điều kiện giống nhau

Một API MAUI có thể cung cấp cùng điểm gọi trên nhiều target, nhưng mỗi hệ điều hành vẫn có thể:

- yêu cầu permission hoặc manifest/configuration khác nhau;
- hiện UI hệ thống khác nhau;
- hỗ trợ capability, giới hạn số lượng hoặc định dạng khác nhau;
- trả kết quả theo dạng URI/stream khác nhau;
- bị người dùng từ chối, hủy hoặc không có app/provider phù hợp;
- có khác biệt theo OS version hoặc thiết bị.

Vì vậy, khi chọn API, hãy hiểu hợp đồng chứ không chỉ tên method:

1. API hỗ trợ target nào và yêu cầu cấu hình gì?
2. Kết quả thành công có thể rỗng hoặc nhiều phần tử không?
3. Người dùng hủy/từ chối thì nhận kết quả hay exception?
4. Có cần permission, và quyền có thể bị thu hồi không?
5. Dữ liệu trả về có thể đọc trực tiếp bằng đường dẫn không, hay phải dùng stream?
6. Có cách xử lý khi capability không tồn tại không?

Các chi tiết cho từng API nằm trong [Platform.md](Platform.md). Tài liệu này tập trung vào cách suy nghĩ xuyên nền tảng.

## 6. Cấu hình OS khác với logic của ứng dụng

Hệ điều hành thường cần metadata để biết app có thể làm gì và được khởi chạy thế nào. Cấu hình tương ứng thường nằm ở manifest hoặc project metadata:

| Nền tảng | Cấu hình thường gặp | Ví dụ loại thông tin |
| --- | --- | --- |
| Android | `AndroidManifest.xml`, resources, activity/application attributes | Permission, intent query/filter, theme/resource native, app identity. |
| iOS | `Info.plist`, entitlements, asset catalogs, signing settings | Usage description, URL scheme, capability, signing. |
| Mac Catalyst | iOS-family metadata cộng thiết lập Mac Catalyst | Capability, app identity, window/distribution behavior. |
| Windows | package manifest, app properties, package/signing settings | Identity, capabilities, visual assets, deployment model. |

Tên file/cách cấu hình phụ thuộc template, workload và target. Đừng chép cấu hình một nền tảng sang nền tảng khác chỉ vì tên tính năng giống nhau. Cũng đừng nhầm permission khai báo với permission runtime: một số quyền cần metadata build và còn cần người dùng chấp thuận lúc chạy.

### Ví dụ đang có: Android manifest overlay

VocabMate có `PlatformConfiguration/AndroidManifest.xml` khai báo khả năng truy vấn dịch vụ Text-to-Speech. `MauiApp1.csproj` chỉ ghép overlay này vào Android build. Project cũng thay native color resources của Android bằng file riêng. Đây là ví dụ về **metadata/tài nguyên riêng target**, không phải logic học từ vựng.

Khi Android không tìm thấy service, kiểm tra lần lượt:

1. API/adapter đang gọi gì?
2. Có yêu cầu khai báo package visibility/permission/capability không?
3. File cấu hình có được đưa vào đúng Android build action/overlay không?
4. Kết quả khác nhau trên emulator và thiết bị thật không?

Không chữa lỗi manifest bằng cách thêm khai báo tùy đoán; xác nhận điều kiện API và kiểm tra package đã build.

## 7. Ba cách tạo khác biệt theo platform

Khi yêu cầu khác theo hệ điều hành, có thể tổ chức khác biệt ở vài mức:

### A. Chọn hành vi lúc chạy

Dùng thông tin platform/capability để chọn nhánh khi app đang chạy. Hợp khi cùng binary target chứa được các nhánh, và cần quyết định theo OS version hoặc capability thật. Không dùng tên OS như bằng chứng rằng một capability chắc chắn có.

### B. Chọn code lúc build

Điều kiện biên dịch theo platform tạo ra code khác nhau cho mỗi target. Hợp khi một nhánh phụ thuộc kiểu/API native chỉ tồn tại ở một target. Đổi lại, phải bảo đảm mỗi target vẫn có implementation hợp lệ và vẫn biên dịch được.

### C. Tách adapter/partial implementation

Đưa chi tiết native vào một service/adapter hoặc implementation riêng theo target; phần dùng chung gọi qua hợp đồng chung. Hợp khi khác biệt có nhiều bước, cần kiểm thử riêng hoặc không muốn ViewModel biết API native.

Chọn cách đơn giản nhất còn giữ được ranh giới rõ. Một nhánh nhỏ có thể chỉ cần điều kiện build; tích hợp lớn thường đáng được bọc thành adapter. Tránh rải điều kiện OS trong nhiều ViewModel, vì lúc đó logic sản phẩm bị trộn với chi tiết hệ điều hành.

## 8. Native control, handler và API hệ điều hành

Có hai hướng khác nhau thường bị gọi chung là “platform code”:

- **Tùy biến control hiện có:** handler/map MAUI control sang native view; dùng khi cần thay đổi cách control được thể hiện hoặc hoạt động.
- **Gọi capability của OS:** dùng API thiết bị như file picker, permission, share, TTS; thường đặt sau service/adapter nếu phần nghiệp vụ cần dễ kiểm thử.

Handler liên quan đến vòng đời native view. Native object chỉ hợp lệ khi handler tương ứng còn gắn; nếu đăng ký native event, cần gỡ khi handler cũ bị thay/tháo. Việc cấu hình mapper có thể ảnh hưởng mọi control cùng loại nên phải giới hạn phạm vi và thử cả control đối chứng.

Đừng tạo custom handler chỉ để gọi một API thiết bị. Đừng gọi API native trực tiếp từ ViewModel nếu một service nhỏ có thể cô lập dependency và chuyển đổi kết quả thành hợp đồng dùng chung.

Xem [Handler.md](Handler.md) cho mô hình handler và [Fundamentals.md](Fundamentals.md) cho lifecycle.

## 9. Vòng đời: OS, Window và control là các cấp khác nhau

Mỗi OS quản lý process, activity/window, focus và native control theo quy tắc riêng. MAUI đưa ra một số abstraction chung, nhưng không làm mọi callback của các OS trở thành tương đương tuyệt đối.

| Cấp | Câu hỏi | Mẫu phản ứng |
| --- | --- | --- |
| App/process | Process còn sống không? Có thể bị OS dừng mà không báo trước không? | Persist trạng thái quan trọng thường xuyên, có thể phục hồi. |
| Window | Cửa sổ đang active, stopped hay resumed? | Tạm dừng/tiếp tục công việc gắn với cửa sổ, refresh khi phù hợp. |
| Page/navigation | Màn hình nào đang được hiển thị? Dữ liệu nhập có cần giữ không? | Lưu draft, tải lại hoặc không ghi đè form đang sửa. |
| Handler/native view | Control native nào đang gắn? Handler có bị thay chưa? | Quản lý native event và tham chiếu theo handler lifecycle. |

Không coi sự kiện “app sắp đóng” là cơ hội duy nhất để ghi dữ liệu. Trên mobile, OS có thể kết thúc process khi app ở nền; trên desktop, window có thể resize, minimize hoặc đóng theo tương tác người dùng. Hãy thiết kế persist và resume theo mức bảo đảm thực tế của API.

## 10. Android và Windows: khác nhau ở trải nghiệm nào?

Không cần học toàn bộ nội bộ OS ngay từ đầu. Hãy bắt đầu từ những khác biệt người dùng nhìn thấy và API mà app thực sự dùng.

| Chủ đề | Android | Windows |
| --- | --- | --- |
| Không gian hiển thị | Nhiều kích thước/mật độ màn hình, thanh hệ thống, gesture và bàn phím ảo. | Cửa sổ thay đổi kích thước, độ phân giải, DPI, chuột, touch và bàn phím. |
| Lifecycle | App thường chuyển foreground/background; process có thể bị thu hồi. | Người dùng có thể minimize, chuyển cửa sổ, đóng cửa sổ; app có thể chạy packaged hoặc unpackaged. |
| Tích hợp | Permission, intent, activity, provider/service, package visibility. | File picker/system integration, windowing, package identity và deployment model. |
| Kiểm thử | Emulator/device với API level và quyền khác nhau. | Các kích thước cửa sổ, bàn phím/chuột/touch, packaged/unpackaged nếu phát hành cả hai. |

Đây là nhóm câu hỏi để kiểm chứng, không phải mô tả đầy đủ mọi phiên bản OS. Luồng cốt lõi VocabMate nên giữ chung; bố cục, input, picker, TTS, theme/system bars và lifecycle cần thử theo target.

## 11. Tài nguyên và identity cũng có phạm vi platform

Một ảnh dùng trong UI chung có thể được khai báo như MAUI image resource; icon launcher, splash screen, native color resource và package identity có thể cần cấu hình build riêng hoặc biến đổi theo target. **File nằm trong repo chưa chắc đã được đóng gói**: build action và metadata `.csproj` quyết định vai trò.

Application ID/package identity cũng có tác động thực tế: OS dùng nó để nhận diện cài đặt, lưu dữ liệu app, quyền và cập nhật. Đổi ID thường có nghĩa app được xem là sản phẩm/cài đặt khác; không giả định dữ liệu cũ tự di chuyển. Signing và phát hành còn phụ thuộc hệ sinh thái riêng của mỗi OS.

VocabMate khai báo Android manifest overlay và native colors theo điều kiện target Android; icon, splash, fonts và images khai báo trong project MAUI. Release checklist của project có thêm các lưu ý riêng về application ID, Android signing và Windows packaging.

## 12. Cách chẩn đoán một khác biệt platform

Khi một tính năng chạy trên target này nhưng hỏng/khác trên target kia, đi theo thứ tự sau:

1. **Xác nhận thực tế:** target framework, OS version, thiết bị/emulator, build configuration.
2. **Phân loại hiện tượng:** build failure, startup failure, UI khác, API không hỗ trợ, permission, dữ liệu hoặc lifecycle.
3. **Tách lớp:** shared logic, MAUI abstraction, handler/native control, OS API, cấu hình/package.
4. **Đối chiếu hợp đồng:** capability, return value, exception, cancellation, permission và thread.
5. **Xem build artifacts/log:** xác nhận manifest/resource/asset có thực sự nằm trong package của target đó.
6. **Tạo phép thử nhỏ:** cùng input và cùng thao tác trên hai target, thay một biến mỗi lần.
7. **Sửa nơi sở hữu hành vi:** không thêm nhánh platform ở tầng cao hơn nếu lỗi nằm ở cấu hình hoặc adapter.

| Triệu chứng | Chỗ nên kiểm tra đầu tiên |
| --- | --- |
| Không biên dịch trên target kia | API native lọt vào shared code, target condition, package reference. |
| App build nhưng capability vắng mặt | Manifest/permission, API support, provider trên thiết bị, user consent. |
| Giao diện khác kích thước | Layout constraints, DPI/font scale, safe area, keyboard, window size. |
| Event chạy nhiều lần hoặc dùng control cũ | Handler attach/detach và đăng ký event lặp lại. |
| App resume nhưng dữ liệu cũ | Window lifecycle, nguồn dữ liệu, quy tắc refresh và draft đang sửa. |
| Hoạt động trên emulator nhưng không có thiết bị thật | OS version/provider/hardware/permission hoặc giả định về đường dẫn file. |

## 13. Ma trận kiểm thử theo tính năng

Không nhất thiết chạy mọi tổ hợp OS/device cho mọi thay đổi. Chọn ma trận theo rủi ro và capability mà tính năng sử dụng.

| Loại thay đổi | Kiểm tra tối thiểu |
| --- | --- |
| Shared business rule/repository | Unit tests; build mọi target trong phạm vi release. |
| Layout/input | Android và Windows; màn hẹp/rộng; bàn phím, font scale và focus. |
| Permission/file picker/share/TTS | Mỗi target hỗ trợ; hủy, từ chối, không có provider; thiết bị thật nếu API phụ thuộc OS. |
| Lifecycle/autosave | Chuyển nền/quay lại, đóng/mở window, thao tác khi save đang chạy. |
| Handler/native code | Target cụ thể, nhiều instance, attach/detach, control thường không thuộc nhóm tùy biến. |
| Package/signing/release | Build Release và cài đúng artifact phân phối; kiểm tra update/identity. |

Một build thành công chứng minh code/config biên dịch được, không chứng minh permission, UI hay native capability hoạt động đúng trên thiết bị thật.

## 14. Lộ trình học Platforms

### Bước 1: Đọc target trước

Mở `.csproj`, ghi target frameworks, minimum OS versions, package references và item condition. Nói thành lời: “Hiện project phát hành cho target nào; target nào chỉ có folder nhưng chưa build?”

### Bước 2: Nhận diện ranh giới

Chọn một chức năng dùng thiết bị, ví dụ TTS hoặc file picker. Tìm nơi ViewModel yêu cầu chức năng, adapter nào gọi API, và cấu hình nào OS cần. Vẽ ba hộp: shared logic → adapter → OS.

### Bước 3: Chạy cùng luồng trên hai target

Ghi lại hành vi chung và khác biệt. Không sửa ngay. Phân loại mỗi khác biệt thuộc UI, API contract, permission, lifecycle, resource hay packaging.

### Bước 4: Kiểm tra native integration

Chọn một ví dụ nhỏ như Android manifest overlay hoặc Windows handler registration. Giải thích vì sao chỉ target đó cần nó, lúc nào được nạp và ảnh hưởng tới phạm vi nào.

### Bước 5: Thêm target mới chỉ khi có yêu cầu

Thêm target không chỉ là thêm chuỗi TFM. Cần workload/toolchain, bootstrap/configuration, build thành công, xử lý API khác biệt, thiết kế UX và kiểm thử/phát hành target đó. iOS/macOS còn cần môi trường Apple phù hợp.

**Đích đến:** phân biệt được “logic app dùng chung”, “API MAUI có abstraction” và “phần OS-specific”; dự đoán được file/cấu hình/build target nào cần thay khi thêm một nền tảng.

## 15. Hiểu nhầm thường gặp

| Hiểu nhầm | Cách hiểu chính xác hơn |
| --- | --- |
| Có `Platforms/iOS` là app đã hỗ trợ iOS. | `.csproj` phải có target và môi trường build phù hợp; sau đó còn phải kiểm thử/phát hành. |
| API MAUI chung đảm bảo mọi OS làm giống nhau. | API chung giảm khác biệt bề mặt; support, quyền, UI hệ thống và kết quả vẫn cần kiểm tra. |
| Mọi khác biệt nên dùng `#if`. | Chỉ dùng build condition khi khác biệt thực sự cần compile-time; cân nhắc adapter/API runtime cho capability. |
| Android permission chỉ là checkbox trong manifest. | Có quyền cần khai báo, xin lúc chạy, xử lý từ chối/thu hồi và test lại. |
| Native code làm mất lợi ích MAUI. | Native integration có thể cô lập ở biên; nghiệp vụ và phần lớn UI vẫn chia sẻ. |
| Build được là chạy đúng trên thiết bị. | Build kiểm tra compile/package; runtime permission, provider, UI và lifecycle cần kiểm tra riêng. |
| Một lần thêm target là xong đa nền tảng. | Mỗi target tạo trách nhiệm về UX, cấu hình, toolchain, kiểm thử, signing và support. |

## 16. Đọc tiếp trong repo

- [MAUI-ban-chat.md](MAUI-ban-chat.md): mô hình tổng thể từ app startup tới luồng dữ liệu và UI.
- [Platform.md](Platform.md): API thiết bị, App Links và các chủ đề platform cụ thể.
- [Handler.md](Handler.md): native control, mapper và lifecycle của handler.
- [Fundamentals.md](Fundamentals.md): lifecycle Window, DI, resource, accessibility và localization.
- [Release-Android-Windows.md](../MauiApp1/Docs/Release-Android-Windows.md): checklist release thực tế của hai target hiện có.
- [VocabMate-architecture.md](../MauiApp1/Docs/VocabMate-architecture.md): service adapter và các luồng native mà app đang dùng.