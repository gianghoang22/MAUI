# Hiểu bản chất .NET MAUI

> Tài liệu này giải thích MAUI như một hệ thống và cách các phần trong ứng dụng phối hợp với nhau. Mục tiêu không phải học thuộc C#, XAML hay tên API, mà là có thể giải thích một màn hình và một thao tác đi qua ứng dụng như thế nào.
>
> Phạm vi project hiện tại: .NET MAUI 10 / .NET 10, Android và Windows, ứng dụng VocabMate. MAUI cũng hỗ trợ các đích khác tùy phiên bản và môi trường build; đừng mặc định mọi project có cùng target.

## 1. Một câu để nắm MAUI

**.NET MAUI là bộ khung để xây ứng dụng giao diện cho nhiều nền tảng, với một mô hình ứng dụng chung và các cầu nối tới khả năng riêng của từng hệ điều hành.**

MAUI không phải ngôn ngữ lập trình, không phải một giao diện web chạy trong trình duyệt, và cũng không biến Android với Windows thành cùng một hệ điều hành. Mày viết phần chung bằng .NET và MAUI; khi build, project được tạo cho từng target riêng. Khi chạy, MAUI phối hợp phần chung với cách nền tảng đó tạo cửa sổ, hiển thị control, nhận thao tác và cung cấp dịch vụ thiết bị.

```text
Mã ứng dụng dùng chung
  ├─ giao diện và trạng thái màn hình
  ├─ quy tắc nghiệp vụ
  └─ dịch vụ / lưu trữ dùng chung
          │
          ▼
      .NET MAUI
  ├─ mô hình Page, Layout, Control
  ├─ binding, resources, điều hướng
  └─ handler và API nền tảng
  ╱       │          │          ╲
   ▼        ▼          ▼           ▼
Android  Windows      iOS      Mac Catalyst
target   target       target      target
 ```

**Chia sẻ code không đồng nghĩa chia sẻ một binary duy nhất**, và cũng không đảm bảo hình thức hay hành vi pixel-perfect giống nhau. Project vẫn phải được build, cài, chạy và kiểm chứng theo từng nền tảng đích.

## 2. Bốn lớp nên phân biệt

| Lớp | Câu hỏi mà lớp đó trả lời | Ví dụ trong VocabMate |
| --- | --- | --- |
| Nền tảng | App đang chạy trên hệ điều hành và cửa sổ nào? | Android hoặc Windows; permission, bàn phím, file picker. |
| MAUI | Làm sao mô tả trang, control, layout, binding và điều hướng theo cách chung? | `ContentPage`, `Grid`, `Entry`, Shell. |
| Ứng dụng | Sản phẩm cần quy tắc và trạng thái gì? | Chấm câu trả lời, lọc thẻ, xác nhận xóa bộ từ. |
| Dữ liệu / dịch vụ | Dữ liệu lấy, kiểm tra, lưu và chia sẻ như thế nào? | Repository JSON, `LearningEngine`, file Excel. |

Một lỗi dễ gặp khi học là trộn các lớp này lại. Ví dụ: “nút bấm bị lệch trên Windows” thường là layout/style/handler/platform; “đáp án bị chấm sai” là quy tắc ứng dụng; “danh sách không đổi sau khi thêm thẻ” có thể là luồng dữ liệu/binding. Xác định đúng lớp giúp tìm đúng nguyên nhân.

## 3. App khởi động thành màn hình đầu tiên như thế nào?

Ứng dụng cần một **composition root**: nơi dựng cấu hình, đăng ký dịch vụ và nối các đối tượng lớn với nhau. Trong project này, `MauiProgram` đăng ký dịch vụ và page; `App` khởi tạo tài nguyên, ngôn ngữ/theme rồi tạo `Window` với `AppShell`.

Luồng chung sau khi nền tảng đã gọi factory của MAUI:

```text
MauiProgram.CreateMauiApp()
  → builder.UseMauiApp<MauiApp1.App>() + đăng ký DI/handler
  → build MauiApp
  → tạo MauiApp1.App và nạp application resources
  → App.CreateWindow() tạo Window chứa AppShell
  → Shell hiển thị route/page hiện hành
  → Page tạo cây giao diện và kết nối trạng thái
```

Mỗi hệ điều hành có bootstrap/entry point riêng để đi tới `MauiProgram.CreateMauiApp()`. Trong project này, các file nằm dưới `MauiApp1/Platforms/`; Mac Catalyst là target MAUI cho ứng dụng chạy trên macOS.

| Target | OS/native entry point | Vai trò trong bootstrap |
| --- | --- | --- |
| Android | `MainActivity` và `MainApplication` | Android tạo `MainApplication`; MAUI gọi `MainApplication.CreateMauiApp()`. Activity có `MainLauncher = true` là màn hình native được OS mở, kế thừa `MauiAppCompatActivity` và gắn UI MAUI. |
| Windows | `Platforms/Windows/App.xaml` và `App.xaml.cs` (`MauiWinUIApplication`) | WinUI kích hoạt platform App; override `CreateMauiApp()` gọi factory chung. WinUI/MAUI host quản lý native window và nối nó với `MauiApp1.App.CreateWindow()`. |
| iOS | `Program.Main` → `UIApplication.Main` → `AppDelegate` | UIKit khởi chạy `MauiUIApplicationDelegate`; delegate override `CreateMauiApp()` để gọi factory chung. |
| Mac Catalyst | `Program.Main` → `UIApplication.Main` → `AppDelegate` | Cấu trúc bootstrap gần như iOS, nhưng được build/chạy bằng target Mac Catalyst và ứng dụng cửa sổ macOS. |

Android cần phân biệt hai vai trò: **`MainApplication` khởi tạo host MAUI/factory; `MainActivity` là Activity launcher mà Android mở để hiển thị ứng dụng.** Nói “MainApplication khởi động app” có thể đúng theo nghĩa nó tham gia tạo MAUI app, nhưng nó không phải Activity được người dùng nhìn thấy đầu tiên.

Windows cũng có hai lớp tên `App`: `MauiApp1.WinUI.App` trong `Platforms/Windows` là bootstrap WinUI; `MauiApp1.App` ở project gốc là lớp `Application` dùng chung. `CreateMauiApp()` nối bootstrap với MAUI; `MauiProgram` đăng ký cấu hình/dịch vụ; `MauiApp1.App.CreateWindow()` dựng cửa sổ nội dung của app. Đây là ba trách nhiệm khác nhau.

Trong repo hiện tại, `.csproj` chỉ target Android và Windows. Các file iOS/Mac Catalyst có thể có mặt trong source tree, nhưng hai nhánh đó chưa nằm trong build/release hiện hành cho tới khi target frameworks và toolchain được cấu hình. iOS cần môi trường build Apple; Mac Catalyst cũng cần toolchain macOS phù hợp.

Đây là sơ đồ khái niệm; callback chi tiết và cách OS kích hoạt process khác nhau. Điểm cốt yếu là phân biệt:

- **App** đại diện phần ứng dụng chung và tài nguyên cấp app.
- **Window** là cửa sổ đang hiển thị app; app có thể có lifecycle/window-specific behavior.
- **Shell** tổ chức các trang và điều hướng nếu ứng dụng chọn dùng Shell.
- **Page** là một màn hình/đơn vị giao diện; không phải toàn bộ app.
- **ViewModel/service/repository** giữ trạng thái và logic theo trách nhiệm riêng; không phải thành phần bắt buộc do MAUI tự tạo.

Trong project này, `App` chỉ resolve `AppShell` khi tạo window, sau khi tài nguyên app đã khởi tạo. Đây là một quyết định của project để dependency và resource được sẵn sàng đúng lúc, không phải quy tắc mọi app MAUI phải sao chép nguyên xi.

## 4. Giao diện là một cây đối tượng

XAML chủ yếu là cách khai báo cây đối tượng có cấu trúc cha–con. Mỗi phần tử tương ứng với một object MAUI, được gán thuộc tính và đặt vào cha của nó.

```text
Page
└─ Grid
   ├─ Label
   ├─ Entry
   └─ Button
```

Cha thường quyết định cách sắp xếp con. `Grid` chia hàng/cột; stack layout xếp theo hướng; scroll view cung cấp vùng cuộn. Vì vậy khi một control sai kích thước hoặc vị trí, hãy lần từ control lên layout cha và hỏi:

1. Ai sở hữu không gian dành cho phần tử này?
2. Layout đang đo và sắp xếp nó theo quy tắc nào?
3. Có constraint, vùng cuộn, bàn phím, safe area hay cửa sổ hẹp nào ảnh hưởng không?
4. Control có hiển thị đúng nội dung nhưng bị cắt, hay đã nhận sai dữ liệu từ trước?

Vòng đời bố cục thường được hiểu qua hai ý: **đo** xem các phần tử cần/được phép bao nhiêu không gian, rồi **sắp xếp** chúng vào vùng thực tế. Hệ điều hành, font scale, kích thước cửa sổ và control native có thể làm kết quả khác nhau giữa target. MAUI không thể đoán thay ý định thiết kế của ứng dụng.

XAML không phải markup được trình duyệt hiển thị. Nó được MAUI xử lý thành các đối tượng giao diện trong quá trình build hoặc chạy, tùy cấu hình project. Code-behind là phần C# của Page; `InitializeComponent` khởi tạo phần UI khai báo bằng XAML. XAML mô tả giao diện thuận tiện, nhưng không bắt buộc: giao diện cũng có thể được tạo bằng C#.

## 5. Control MAUI gặp giao diện native ở đâu?

Các control MAUI đưa ra một mô hình chung. **Handler** là ranh giới kết nối control/API MAUI với implementation của nền tảng. Handler quản lý native view và ánh xạ giá trị hoặc yêu cầu giữa hai bên.

```text
Entry của ứng dụng
   → API/control MAUI
   → EntryHandler và mapping
   → native view / API của target hiện tại
```

Khi người dùng gõ, sự kiện native được đưa ngược qua cầu nối để MAUI và ứng dụng nhận thay đổi. Khi ViewModel cập nhật giá trị, mapping có thể chuyển thay đổi đó xuống native control. Chi tiết nội bộ khác nhau theo control và phiên bản MAUI.

Điều quan trọng: **đừng giản lược thành “mọi control MAUI chỉ là control native giống hệt tên gọi”.** Nhiều control dựa trên thành phần native; một số phần được MAUI hỗ trợ/tổ chức theo cách riêng. Dù thế nào, hành vi cuối cùng vẫn chịu ảnh hưởng bởi target, handler, theme, kích thước, input method và API hệ điều hành.

Nên xử lý theo thứ tự:

1. Dùng property/style/layout có sẵn nếu đã đáp ứng yêu cầu.
2. Dùng template hoặc behavior nếu cần tái sử dụng trình bày/hành vi.
3. Tùy biến handler khi thực sự cần đổi cách MAUI ánh xạ sang native.
4. Gọi API native trực tiếp khi API chung không đủ.

Handler không đồng nghĩa với MVVM command. `ICommand` xử lý yêu cầu thao tác ở tầng UI/ViewModel; mapper/command của handler nối API MAUI với native ở tầng control.

## 6. Dữ liệu đến giao diện và quay lại như thế nào?

Giao diện cần biết **nó đang hiển thị object nào**. Binding lấy giá trị từ object đó và gắn với property của control. Khi người dùng nhập hoặc khi dữ liệu đổi, chiều truyền phụ thuộc vào loại binding và thông báo thay đổi.

```text
Model / repository / service
          ↓ dữ liệu
      ViewModel
  property + command
          ↕ binding
    Page / controls
          ↑ thao tác người dùng
```

- **Model** mô tả dữ liệu hoặc khái niệm nghiệp vụ.
- **ViewModel** cung cấp trạng thái và thao tác mà màn hình cần, thường không biết control cụ thể.
- **View** trình bày trạng thái và chuyển thao tác tới command hoặc sự kiện phù hợp.
- **Binding** nối property của View với dữ liệu; nó không tự tìm đúng dữ liệu nếu context/path sai.
- **Command** biểu diễn thao tác như lưu, tìm kiếm hoặc bắt đầu học. Logic có thể quyết định command đang chạy được hay không.

MVVM là một pattern phổ biến, **không phải điều kiện bắt buộc để MAUI hoạt động**. Có thể dùng code-behind cho điều phối giao diện phù hợp; điều nên tránh là để một màn hình vừa vẽ UI, vừa chứa toàn bộ nghiệp vụ, lưu file và điều khiển API native.

Binding cũng không quan sát mọi object/property một cách thần kỳ. UI chỉ biết dữ liệu đổi nếu cơ chế thông báo phù hợp được phát ra. Ví dụ, observable collection báo việc thêm/bớt item, nhưng property bên trong mỗi item cần cơ chế thông báo riêng nếu nó có thể đổi sau khi hiển thị.

### Một thao tác mẫu: lưu thẻ từ

1. Người dùng gõ hai mặt từ; control hiển thị text và binding chuyển dữ liệu vào trạng thái chỉnh sửa.
2. Người dùng bấm Lưu; view chuyển yêu cầu đến command/ViewModel.
3. ViewModel kiểm tra trạng thái, gọi quy tắc và service/repository thích hợp.
4. Repository kiểm tra ràng buộc dữ liệu rồi ghi vào kho lưu trữ.
5. Kết quả được trả về; ViewModel cập nhật trạng thái/thông báo.
6. Binding phản ánh trạng thái mới; điều hướng có thể đưa người dùng về màn trước.

Nếu bước 4 thành công nhưng màn hình không đổi, xem bước 5-6. Nếu nội dung sai ngay trước khi lưu, xem dữ liệu và validation ở bước 1-3. Nếu file không ghi được, đó là nhánh lưu trữ/lỗi, không phải lỗi layout.

## 7. Shell và điều hướng không phải cùng một thứ với dữ liệu

Shell cung cấp cách tổ chức trang/route và điều hướng. Nút bấm hoặc command yêu cầu điều hướng; Shell quản lý việc mở route và navigation stack theo cấu hình ứng dụng.

```text
Library → Deck → Card Editor
              ← Back
```

Điều hướng **không tự động định nghĩa vòng đời dữ liệu**. Một page quay lại có thể vẫn giữ trạng thái hoặc được tạo mới tùy cách ứng dụng tạo/đăng ký đối tượng, route và logic. Cũng không nên giả định mỗi lần Back đồng nghĩa “tạo lại màn hình trước”.

Khi chuyển dữ liệu qua route, thường nên truyền định danh nhỏ (như `deckId`) rồi để màn hình lấy dữ liệu cần thiết, thay vì giả định cả object truyền qua navigation sẽ luôn là nguồn chân lý mới nhất. Tuy nhiên đây là quyết định kiến trúc, không phải giới hạn bắt buộc của Shell.

Trong VocabMate, Shell giữ các tab gốc; route chi tiết nhận ID lớp/bộ/thẻ. ViewModel transient của trang chi tiết được tạo riêng theo lần resolve; một số ViewModel gốc là singleton để chia sẻ trạng thái. Lifetimes này là lựa chọn cụ thể của ứng dụng.

## 8. Dependency Injection: tạo đúng đồ vật, đúng chỗ

Ứng dụng gồm nhiều object phụ thuộc lẫn nhau. DI container là bộ đăng ký/nhà máy giúp tạo object và cung cấp dependency qua constructor.

```text
Page cần ViewModel
ViewModel cần service
Service cần repository
```

DI giúp quan hệ phụ thuộc nhìn thấy được và dễ thay thế/kiểm thử. Nhưng DI không làm cho ứng dụng tự động có kiến trúc tốt: đăng ký sai lifetime, service làm quá nhiều việc, hoặc dùng service locator ở khắp nơi vẫn tạo coupling khó hiểu.

Ba lifetime phổ biến nên hiểu bằng **thời gian sống của instance**:

- Singleton: một instance dùng chung trong container.
- Transient: instance mới khi được resolve.
- Scoped: một instance trong một scope được quản lý; MAUI non-Blazor không tự tạo scope mỗi lần điều hướng.

Hỏi: “Object này cần giữ trạng thái chung bao lâu?” trước khi chọn lifetime. Singleton có trạng thái chỉnh sửa màn hình có thể khiến dữ liệu cũ lọt sang lần mở sau; transient của service giữ cache không chủ ý có thể tạo nhiều bản. Lifetime là quyết định về trạng thái, không chỉ tối ưu hiệu năng.

## 9. Tài nguyên, style và ngôn ngữ

Tài nguyên là giá trị/đối tượng dùng lại: màu, style, template, font, hình ảnh, chuỗi dịch. MAUI có nhiều cơ chế riêng cho từng loại; không phải mọi “resource” đều cùng một kho.

- Resource dictionary XAML giữ các object như màu/style/converter/template theo khóa và phạm vi.
- Ảnh/font/app icon được cấu hình thành tài nguyên build của project.
- Chuỗi đa ngôn ngữ thường nằm trong resource localization, có quy tắc chọn ngôn ngữ/fallback riêng.
- Theme hoặc ngôn ngữ có thể thay đổi lúc chạy nếu app thiết kế để cập nhật; không phải mọi giá trị resource đều tự đổi.

Phạm vi resource ảnh hưởng nơi tra được khóa: gần control/page hơn hay cấp app. Khi một màu/style không được tìm thấy hoặc giá trị không đổi theo theme, kiểm tra nơi khai báo, phạm vi, loại lookup và thời điểm thay đổi.

## 10. Lifecycle, async và UI thread

App không chỉ có trạng thái “đang mở” và “đã đóng”. Window có thể mất focus, bị đưa xuống nền, trở lại hoặc bị hệ điều hành thu hồi. **Không được dựa vào một callback đóng app chắc chắn xảy ra để lưu dữ liệu quan trọng.** Lưu thường xuyên và phục hồi trạng thái là trách nhiệm của ứng dụng.

Page lifecycle, window/app lifecycle và handler lifecycle là ba phạm vi khác nhau:

- Page: một màn hình xuất hiện/rời khỏi luồng hiển thị.
- Window/app: cửa sổ hoặc ứng dụng đổi trạng thái foreground/background.
- Handler: control gắn/tháo native view.

Tác vụ I/O như đọc file, truy cập dịch vụ mạng hay xử lý lâu nên bất đồng bộ để tránh khóa UI. Tuy vậy, **async không tự biến mọi thao tác thành thread-safe**. UI control thường chỉ được cập nhật trên UI thread; collection gắn với UI cũng cần cập nhật đúng ngữ cảnh. Khi người dùng rời màn hình, tác vụ còn chạy có thể cần hủy hoặc bỏ kết quả cũ.

Luôn nghĩ tới các nhánh: đang tải, thành công, lỗi, người dùng hủy, mất kết nối, app xuống nền, người dùng bấm lặp. Một màn hình chỉ đúng ở nhánh “thành công nhanh” chưa phải luồng hoàn chỉnh.

## 11. Lưu trữ và ranh giới với MAUI

MAUI cung cấp API nền tảng chung cho một số khả năng như file system, preferences, secure storage, file picker, connectivity, permissions. Nhưng **quy tắc dữ liệu của sản phẩm là của ứng dụng**.

Ví dụ, MAUI có thể giúp ghi file; nó không tự quyết định:

- một bộ từ có được trùng tên hay không;
- xóa lớp có xóa bộ từ/thẻ/kết quả liên quan không;
- phiên học đang dở phải được khôi phục thế nào;
- dữ liệu cũ hỏng thì có được ghi đè không.

Trong VocabMate, repository và JSON chịu trách nhiệm lưu/kiểm tra dữ liệu; `LearningEngine` chịu trách nhiệm logic học/chấm; adapter riêng gọi API file picker, chia sẻ và text-to-speech. Đây là phân chia theo nhu cầu sản phẩm, không phải bộ phận bắt buộc có tên đó trong MAUI.

Đừng chọn nơi lưu chỉ vì dễ gọi. Preferences hợp với thiết lập nhỏ; file/database hợp với dữ liệu có cấu trúc hoặc dung lượng đáng kể; dữ liệu nhạy cảm cần đánh giá API/bảo vệ phù hợp. Xem xét backup, hỏng dữ liệu, gỡ app, quyền truy cập và đồng thời ghi.

## 12. Build và khác biệt nền tảng

Single project gom phần dùng chung nhưng build theo target framework riêng. Mỗi target có điều kiện biên dịch, manifest, tài nguyên và khả năng runtime phù hợp. Một dòng code dùng API Android thuần không tự nhiên trở thành hợp lệ trên Windows.

Khác biệt có thể đến từ:

- API/permission hoặc phần cứng chỉ có trên một nền tảng;
- implementation native, theme, font rendering, bàn phím và accessibility;
- lifecycle, kích thước cửa sổ, safe area, file URI;
- cấu hình build, manifest, signing và workload.

Quy tắc thực dụng: giữ logic chung khi hành vi thật sự chung; tách phần platform khi API hoặc UX yêu cầu; kiểm thử target nào cũng thuộc phạm vi phát hành. Điều kiện biên dịch theo nền tảng là một công cụ, không nên dùng để che một thiết kế chưa rõ.

Project này hiện cấu hình Android và Windows. Tài liệu chung của MAUI có thể nói cả iOS/Mac Catalyst, nhưng các yêu cầu build và hành vi chưa được kiểm chứng trên target đó trong repo hiện tại.

## 13. Tự lần dấu một chức năng

Đừng đọc project theo kiểu lật từng file từ đầu đến cuối. Chọn một luồng người dùng, rồi trả lời lần lượt:

1. **Điểm bắt đầu:** người dùng chạm gì? Control nào nhận thao tác?
2. **Chuyển yêu cầu:** sự kiện/command nào được gọi? Điều kiện nào khóa hoặc cho phép nó?
3. **Logic:** ViewModel, engine hay service nào quyết định kết quả?
4. **Dữ liệu:** đọc từ đâu, kiểm tra ở đâu, lưu ở đâu?
5. **Phản hồi UI:** property/collection nào đổi và cơ chế nào báo cho giao diện?
6. **Điều hướng:** có đổi route/stack không? Trạng thái trước đó còn sống không?
7. **Nền tảng:** có dùng native API, permission, handler hay code riêng target nào không?
8. **Lỗi và lifecycle:** chuyện gì nếu tác vụ lỗi, bị hủy hoặc app xuống nền?

Ví dụ các luồng thích hợp trong VocabMate:

- Tìm lớp ở Home: input → ViewModel → lọc/chuẩn hóa → danh sách → điều hướng.
- Lưu thẻ: editor → draft → validation → repository → file JSON → quay về bộ từ.
- Bắt đầu lượt học: lựa chọn chế độ → engine tạo snapshot → repository lưu phiên → route Learning.
- Chọn file Excel: adapter mở picker → parser đọc workbook → preview/validation → repository commit.
- Đổi ngôn ngữ: preference → localization service → resource thay đổi → giao diện phản ánh.

## 14. Lộ trình hiểu sâu, không cần học thuộc code

### Chặng 1: Vẽ bản đồ

Chỉ cần xác định App, Window, Shell, Page, ViewModel, service và repository đang nằm ở đâu. Vẽ mũi tên giữa chúng. Chưa cần hiểu mọi method.

### Chặng 2: Theo UI và dữ liệu

Chọn một màn hình. Vẽ cây layout cha–con. Xác định BindingContext, property hiển thị, command và nơi dữ liệu được tạo. Giải thích được một thay đổi dữ liệu sẽ cập nhật UI như thế nào.

### Chặng 3: Theo một thao tác hoàn chỉnh

Chọn Lưu, Xóa hoặc Bắt đầu học. Lần từ control đến quy tắc, lưu trữ, kết quả và phản hồi. Tìm một nhánh lỗi và giải thích UI làm gì khi lỗi xảy ra.

### Chặng 4: So sánh hai target

Chạy cùng luồng trên Android và Windows. Ghi điều gì dùng chung, điều gì khác, và khác biệt thuộc layout, handler, API hay cấu hình. Không sửa cho giống bằng cảm tính trước khi tìm được lớp gây khác biệt.

### Chặng 5: Học đúng phần cần

Khi đã có câu hỏi cụ thể, mới đọc sâu XAML/binding, Shell, handler, lifecycle, platform API hoặc build. Dùng tài liệu nền trong repo làm tài liệu tra cứu, không cần đọc thuộc tất cả một lượt.

**Thước đo tiến bộ:** mày có thể dự đoán điều gì sẽ xảy ra khi đổi một property, bấm một nút, quay lại trang, app xuống nền hoặc chạy trên target khác; sau đó kiểm tra dự đoán bằng ứng dụng.

## 15. Những hiểu nhầm nên bỏ sớm

| Hiểu nhầm | Mô hình đúng hơn |
| --- | --- |
| “Viết một lần là chạy y chang mọi nơi.” | Chia sẻ logic/UI nhiều nhất có thể; từng target vẫn build và cần kiểm chứng riêng. |
| “MAUI là web app.” | MAUI là framework app UI; XAML không phải HTML và handler không phải browser DOM. |
| “MVVM là MAUI.” | MVVM là cách tổ chức ứng dụng thường dùng, không phải điều kiện của framework. |
| “Binding tự cập nhật mọi thứ.” | Cần đúng context/path/mode và cơ chế thông báo thay đổi. |
| “Shell quản lý toàn bộ vòng đời và dữ liệu.” | Shell hỗ trợ cấu trúc/điều hướng; ứng dụng quyết định trạng thái, lưu trữ và lifetime object. |
| “Async nghĩa là chạy an toàn ở nền.” | Async giúp không chặn luồng chờ; UI và dữ liệu chia sẻ vẫn phải đúng thread/lifecycle. |
| “Handler chỉ cần biết khi tự viết control.” | Hiểu handler giải thích khác biệt native và là điểm mở rộng khi API chung không đủ. |
| “Có callback đóng app để lưu lần cuối.” | Hệ điều hành có thể kết thúc process; persist quan trọng nên diễn ra trong luồng bình thường. |

## 16. Tài liệu tiếp theo trong repo

File này là bản đồ khái niệm, không thay thế tài liệu tra cứu chi tiết:

- [Overview.md](Overview.md): MAUI là gì, target, môi trường build và lộ trình tổng quan.
- [XAML.md](XAML.md): cây giao diện, binding, MVVM và cú pháp XAML.
- [Fundamentals.md](Fundamentals.md): Shell, DI, resources, lifecycle, accessibility và localization.
- [Handler.md](Handler.md): cầu nối control MAUI với native và khi nào tùy biến.
- [Platform.md](Platform.md): API thiết bị và tích hợp nền tảng.
- [VocabMate-architecture.md](../MauiApp1/Docs/VocabMate-architecture.md): kiến trúc và luồng đang triển khai trong chính ứng dụng này.

Đọc file này lần đầu để dựng mô hình trong đầu; sau đó theo một luồng trong VocabMate và chỉ mở tài liệu chuyên đề khi câu hỏi xuất hiện.