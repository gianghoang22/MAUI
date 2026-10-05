# .NET MAUI 10 Fundamentals — học theo phương pháp 80/20

> **Mục tiêu:** từ việc dựng một màn hình XAML, tiến tới tổ chức ứng dụng có nhiều màn hình, dữ liệu, tài nguyên dùng chung và hành vi đúng khi chạy trên thiết bị.
>
> **Cập nhật:** 16/09/2026. **Phạm vi:** **.NET MAUI 10 trên .NET 10**. Đã đối chiếu lại 13 nguồn ban đầu theo phiên bản 10, bổ sung thay đổi API và safe area [S14], [S15]. Không đưa các thay đổi riêng MAUI 11 vào kiến thức mặc định.

## 1. Học 80/20 ở phần Fundamentals nghĩa là gì?

**Ưu tiên kiến thức đem lại khả năng sử dụng thực tế cao nhất trước, không phải bỏ 80% phần còn lại.** Thứ tự học phải thay đổi nếu sản phẩm có yêu cầu đặc biệt.

- `Overview.md` trả lời: MAUI là gì, nhắm nền tảng nào, cần chuẩn bị gì?
- `XAML.md` trả lời: UI được tạo và kết nối dữ liệu như thế nào?
- **Tài liệu này** trả lời: tổ chức những phần đó thành ứng dụng ra sao, tránh lỗi phổ biến thế nào, khi nào cần mở rộng control?

Lộ trình, bài tập và ví dụ dưới đây là đề xuất của người tổng hợp, không phải thứ tự bắt buộc do Microsoft quy định. Không cần áp dụng mọi pattern vào ứng dụng đầu tiên.

## 2. Bản đồ ưu tiên của cả 13 chủ đề

| Nhóm | Chủ đề | Học tới mức nào trước? | Nguồn |
| --- | --- | --- | --- |
| A — Dựng luồng chính | Single project | Biết code, tài nguyên, cấu hình chung và phần riêng nền tảng nằm đâu. | [S10] |
| A — Dựng luồng chính | Shell | Tổ chức màn hình, tab/flyout và hình dung điều hướng bằng route. | [S9] |
| A — Dựng luồng chính | Dependency injection | Nối Page → ViewModel → Service, chọn lifetime có chủ đích. | [S5] |
| A — Dựng luồng chính | Data binding | Đồng bộ dữ liệu/UI và biết giới hạn cập nhật trên UI thread. | [S4] |
| A — Dựng luồng chính | Resource dictionaries | Tái sử dụng màu, style, template; hiểu scope và lookup. | [S8] |
| A — Dựng luồng chính | DataTemplate | Hiển thị từng item trong danh sách, không nhầm context với ViewModel trang. | [S12] |
| B — Làm đúng ngay từ đầu | App lifecycle | Phản ứng khi mất focus, chạy nền, trở lại và đóng window. | [S2] |
| B — Làm đúng ngay từ đầu | Accessibility | Giao diện có thể hiểu và thao tác bằng screen reader, bàn phím, chữ lớn. | [S1] |
| B — Làm đúng ngay từ đầu | Localization | Tách chuỗi, có fallback và kiểm tra ngôn ngữ/nền tảng. | [S6] |
| C — Mở rộng khi có nhu cầu | Triggers | Thay đổi UI theo property, dữ liệu, event hoặc visual state. | [S13] |
| C — Mở rộng khi có nhu cầu | Behaviors | Đóng gói hành vi UI tái sử dụng mà không phải kế thừa control. | [S3] |
| C — Mở rộng khi có nhu cầu | Bindable properties | Tạo API cho custom control để nhận binding, style và template. | [S7] |
| C — Mở rộng khi có nhu cầu | ControlTemplate | Tách cấu trúc hiển thị khỏi logic của custom control/page. | [S11] |

**Nhóm B không phải việc để cuối dự án.** Cách đặt nhãn, bố cục, chuỗi hiển thị và xử lý trạng thái nên được xét ngay khi dựng luồng đầu tiên. Nhóm C vẫn có thể lên trước nếu sản phẩm bắt buộc có control hoặc tương tác đặc thù.

## 3. Mô hình tư duy để nối các phần

```text
Single project + MauiProgram
             |
             +-- DI: đăng ký và cung cấp Page / ViewModel / Service
             |
             +-- Shell: tổ chức màn hình và điều hướng
                         |
                         v
                Page / View (XAML)
                         |
                         +-- Binding <-> ViewModel -> Service / dữ liệu
                         +-- ResourceDictionary: màu, style, template
                         +-- DataTemplate: UI của từng item
                         +-- Trigger / Behavior: phản ứng UI
                         +-- Custom control: BindableProperty + ControlTemplate

Xuyên suốt: lifecycle + accessibility + localization
```

Sơ đồ là cách tổ chức kiến thức đề xuất, không có nghĩa Shell, DI hoặc custom control đều bắt buộc cho mọi ứng dụng.

## 4. Phần dùng thường xuyên: project, màn hình và dữ liệu

### 4.1. Single project: dùng chung không có nghĩa xóa khác biệt nền tảng

MAUI dùng multi-targeting trong project kiểu SDK. `MauiProgram.CreateMauiApp()` là điểm khởi tạo chung; thư mục `Platforms` chứa phần khởi chạy, code và tài nguyên đặc thù. Thông tin app chung được hợp nhất với manifest riêng của nền tảng khi build. [S10]

| Nơi/loại tài nguyên thường gặp | Build action cần nhận ra |
| --- | --- |
| `Resources/AppIcon` | `MauiIcon` |
| `Resources/Images` | `MauiImage` |
| `Resources/Fonts` | `MauiFont` |
| `Resources/Splash` | `MauiSplashScreen` |
| `Resources/Raw` | `MauiAsset` |
| XAML | `MauiXaml` |

Các thư mục trên là cách tổ chức thông thường; cấu hình project quyết định file nào thực sự được đưa vào build. Tài nguyên riêng nền tảng có thể ghi đè bản dùng chung. [S10]

**Tự kiểm tra:** thêm một ảnh/font rồi chạy trên nền tảng đích; nếu không thấy, kiểm tra tên file, build action và cấu hình include trước khi sửa UI một cách mò mẫm.

### 4.2. Shell: bộ khung tổ chức màn hình

Shell gom cấu trúc giao diện, điều hướng theo URI/route và khả năng tìm kiếm vào một nơi. Nhận diện `FlyoutItem` hoặc `TabBar` ở cấp đầu, sau đó `Tab` và `ShellContent`; page được tạo theo nhu cầu điều hướng. Chọn tab hay flyout theo luồng sản phẩm, không tạo cả hai chỉ để dùng hết framework. [S9]

**Đầu ra cần có:** vẽ được sơ đồ danh sách → chi tiết → quay lại, cùng các màn hình cấp cao. Trang Shell được cung cấp là tổng quan; cú pháp route, truyền tham số và các trường hợp back-stack cần đọc sâu phần navigation khi triển khai.

**Shell trong MAUI 10:** có thể đặt attached property `Shell.NavBarVisibilityAnimationEnabled="False"` trên page nếu cần tắt animation khi navigation bar ẩn/hiện. Đây là điều khiển animation, không thay thế `Shell.NavBarIsVisible` hoặc logic điều hướng. [S14]

### 4.3. DI: để đối tượng nhận thứ nó cần, thay vì tự tìm mọi thứ

Đăng ký dependency qua `builder.Services` trong `MauiProgram.CreateMauiApp()`, trước `builder.Build()`. Ưu tiên constructor injection để nhìn thấy quan hệ Page → ViewModel → Service. Đăng ký đủ những dependency cần được resolve. [S5]

| Lifetime | Cách hiểu thực tế |
| --- | --- |
| `AddSingleton` | Dùng lại một instance trong container; phù hợp khi thật sự cần trạng thái/service dùng chung. |
| `AddTransient` | Instance mới mỗi lần resolve; thường hữu ích cho Page/ViewModel cần trạng thái riêng. |
| `AddScoped` | Instance theo scope do bạn quản lý; **MAUI non-Blazor không tự tạo scope cho từng lần điều hướng**. |

Đừng đồng nhất một lần quay lại màn hình với một lần resolve mới. Cũng đừng mặc định `Scoped` hoạt động như HTTP request trong ASP.NET Core. [S5], [S9]

**Ví dụ đăng ký — đoạn đặt trong `CreateMauiApp`, không phải chương trình độc lập:** giả định project đã có các interface/class tương ứng và dùng namespace extension `Microsoft.Extensions.DependencyInjection`.

```csharp
builder.Services.AddSingleton<IWorkItemService, WorkItemService>();
builder.Services.AddTransient<WorkListViewModel>();
builder.Services.AddTransient<WorkListPage>();
```

Page nhận `WorkListViewModel` qua constructor rồi gán `BindingContext`; ViewModel nhận `IWorkItemService`. Khi để DI/Shell tạo những đối tượng này, container mới có cơ hội cung cấp dependency. Tự gọi `new` không tự động nhờ container inject. [S5]

**Bẫy khởi động:** inject trực tiếp page vào constructor `App` có thể khiến page được tạo trước khi resource trong `App.xaml` khởi tạo. Nếu page cần resource đó, đọc hướng xử lý trong [S5]: resolve page sau `InitializeComponent()`, thay vì chữa bằng cách xóa resource dùng chung.

**Khởi tạo cửa sổ theo mẫu dùng được trên MAUI 10:** giữ `InitializeComponent()` trong constructor `App`, tạo page gốc qua `CreateWindow()` thay vì chép mẫu cũ gán `Application.MainPage`. Project `MauiApp1` hiện đã dùng cách này. Đoạn sau nằm trong class `App` và giả định project có `AppShell`: [S10]

```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    return new Window(new AppShell());
}
```

Nếu `AppShell` cần constructor injection, resolve nó từ service provider đã đăng ký sau khi resource ứng dụng được khởi tạo, thay vì giữ `new AppShell()` một cách máy móc. Không dùng đồng thời hai đường tạo page gốc. [S5], [S10]

### 4.4. Binding và DataTemplate: tránh viết lại handler đồng bộ dữ liệu

Binding nối property giữa UI với đối tượng dữ liệu. Các khái niệm `BindingContext`, `x:DataType`, `INotifyPropertyChanged` và `Command` đã được trình bày trong `XAML.md`; ở đây ưu tiên điểm dễ sai khi đưa vào ứng dụng thật. [S4]

**UI thread:** cơ chế binding của MAUI chuyển cập nhật từ thông báo `PropertyChanged` sang UI thread. Điều đó **không áp dụng tự động cho `ObservableCollection<T>.Add/Remove/Clear`**. Nếu cập nhật collection đang gắn UI từ background thread, phải dispatch thao tác phù hợp về UI thread. Không suy ra rằng mọi thao tác trên control đều an toàn ở background. [S4], [S7]

`DataTemplate` mô tả giao diện của **một item dữ liệu**; `CollectionView.ItemsSource` là tập hợp, còn binding bên trong `ItemTemplate` đọc từng item. Khai báo `x:DataType` theo kiểu item, không theo ViewModel trang. Dùng template inline khi chỉ xuất hiện một lần; chuyển thành resource khi cần tái sử dụng. [S12]

### 4.5. ResourceDictionary: tái sử dụng có phạm vi

Phân biệt **file tài nguyên** như ảnh/font của single project với **object tài nguyên XAML** như màu, style, converter và template. `ResourceDictionary` lưu object theo khóa; `.resx` ở phần localization là cơ chế khác. [S8], [S10], [S6]

- **Scope:** control/layout → page → app; lookup đi từ nơi sử dụng lên cha rồi đến app. Khóa gần nơi dùng hơn sẽ được ưu tiên.
- **`StaticResource`:** tra theo khóa một lần. **`DynamicResource`:** giữ liên kết với khóa để phản ánh khi entry tương ứng được thay thế.
- **Merge:** resource khai báo trực tiếp trong dictionary ưu tiên hơn resource merge; giữa các dictionary merge, bản liệt kê sau được ưu tiên khi trùng khóa.
- Đừng gom resource chỉ dùng ở một page vào `App.xaml` một cách máy móc. [S8]

## 5. Ví dụ ngắn: ghép resource, DataTemplate, trigger và semantics

Đây là **XAML do người tổng hợp viết**, minh họa [S1], [S8], [S12], [S13]; không phải toàn bộ ứng dụng đã chạy được.

**Điều kiện để tích hợp:** project dùng namespace `FundamentalsDemo`; có `WorkListPage.xaml.cs` gọi `InitializeComponent()` và gán ViewModel nhận qua DI. ViewModel cung cấp `Items`. Mỗi `FundamentalsDemo.Models.WorkItem` cung cấp `Title`, `IsCompleted` và `StatusText`. Nếu các giá trị đổi sau khi hiển thị, item phải thông báo thay đổi, kể cả property phụ thuộc như `StatusText`.

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:FundamentalsDemo.Models"
             xmlns:vm="clr-namespace:FundamentalsDemo.ViewModels"
             x:Class="FundamentalsDemo.WorkListPage"
             x:DataType="vm:WorkListViewModel"
             SafeAreaEdges="All"
             Title="Công việc">
    <ContentPage.Resources>
        <Color x:Key="AccentColor">#2563EB</Color>
    </ContentPage.Resources>

    <Grid Padding="20" RowDefinitions="Auto,*" RowSpacing="12">
        <Label Text="Danh sách công việc"
               FontSize="24"
               SemanticProperties.HeadingLevel="Level1" />
        <CollectionView Grid.Row="1" ItemsSource="{Binding Items}">
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:WorkItem">
                    <VerticalStackLayout Padding="0,8" Spacing="4">
                        <Label Text="{Binding Title}"
                               TextColor="{DynamicResource AccentColor}">
                            <Label.Triggers>
                                <DataTrigger TargetType="Label"
                                             Binding="{Binding IsCompleted}"
                                             Value="True">
                                    <Setter Property="TextDecorations"
                                            Value="Strikethrough" />
                                </DataTrigger>
                            </Label.Triggers>
                        </Label>
                        <Label Text="{Binding StatusText}" />
                    </VerticalStackLayout>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Grid>
</ContentPage>
```

**Điều cần tự giải thích:**

1. Page đọc `Items` từ ViewModel; template đọc `Title`/`IsCompleted`/`StatusText` từ từng item.
2. `AccentColor` được tra ở resource của page; thay entry này là tình huống để thử `DynamicResource`.
3. `DataTrigger` thay cách trình bày khi item hoàn thành; không tự lưu trạng thái vào backend.
4. `StatusText` cung cấp thông tin bằng chữ, không chỉ dựa vào màu hoặc gạch ngang.
5. Khi đưa vào sản phẩm đa ngôn ngữ, chuyển các chuỗi hiển thị cố định sang resource localization; đổi đồng bộ namespace nếu project mang tên khác.
6. `SafeAreaEdges="All"` thể hiện chủ đích tránh thanh hệ thống và bàn phím; đây là API MAUI 10, đọc mục 6.4 và kiểm thử trên target thật. [S15]

Chưa build/chạy mẫu trên MAUI trong phiên tổng hợp này. Không coi kiểm tra cú pháp XML là bằng chứng rằng ứng dụng native đã hoạt động.

## 6. Runtime và chất lượng: không để cuối dự án

### 6.1. Lifecycle: theo dõi Window, không nhầm với điều hướng page

Các sự kiện đa nền tảng nằm trên `Window`. Có thể đăng ký khi tạo window trong `App.CreateWindow()`, hoặc override các phương thức lifecycle của lớp kế thừa `Window`. [S2]

| Sự kiện | Điều cần phân biệt | Việc cân nhắc |
| --- | --- | --- |
| `Created` | Native window đã tạo, chưa chắc hiển thị. | Khởi tạo phần cần window/handler. |
| `Activated` | Window nhận focus. | Khôi phục tương tác cần thiết. |
| `Deactivated` | Mất focus nhưng vẫn có thể còn nhìn thấy. | Không mặc định ứng dụng đã chạy nền hoàn toàn. |
| `Stopped` | Window không còn hiển thị; không bảo đảm sẽ resume. | Dừng/hủy công việc tốn tài nguyên không còn cần thiết. |
| `Resumed` | Trở lại sau `Stopped`, không phải lần chạy đầu. | Làm mới dữ liệu, đăng ký lại thứ cần thiết mà tránh trùng lặp. |
| `Destroying` | Native window đang bị hủy. | Dọn các đăng ký gắn với native window. |

**Áp dụng 80/20:** ghi log các chuyển trạng thái trước, thử đổi app/khóa màn hình/quay lại. Thiết kế lưu dữ liệu quan trọng đủ sớm, không chỉ chờ đến lúc đóng ứng dụng. Chỉ dùng `ConfigureLifecycleEvents` và callback riêng nền tảng khi sự kiện chung không đủ. [S2]

### 6.2. Accessibility: dùng được, không chỉ nhìn đẹp

Ưu tiên `SemanticProperties.Description`, `Hint`, `HeadingLevel`. Kiểm thử trên screen reader thực tế: TalkBack, VoiceOver hoặc Narrator theo nền tảng. Hỗ trợ chữ lớn, bố cục linh hoạt và luồng focus hợp lý; thông tin quan trọng không được chỉ biểu đạt bằng màu/âm thanh. [S1]

**Không thêm mô tả một cách máy móc:**

- `Label` đã có text: đặt thêm `Description` có thể thay lời mà screen reader đọc.
- Trên Android, tránh `Description` trên `Entry`/`Editor`; lưu ý `Hint` và `Entry.Placeholder` dùng chung một property nền tảng.
- Trên iOS, đặt mô tả vào control chứa con có thể làm screen reader không tiếp cận được các con.
- Localize cả nội dung mô tả trợ năng. [S1]

**Tiêu chí thực hành:** thực hiện được luồng chính bằng screen reader và bàn phím khi nền tảng hỗ trợ, không chỉ nhìn giao diện rồi kết luận đã accessible.

### 6.3. Localization: tách chuỗi và có đường fallback

Dùng file `.resx` mặc định cho các khóa và file theo culture cho bản dịch, ví dụ `AppResources.resx`, `AppResources.vi.resx`, `AppResources.en.resx`. Runtime tra từ culture cụ thể về ngôn ngữ rồi resource mặc định; khai báo `NeutralLanguage` phù hợp. Truy cập chuỗi qua class resource được sinh, có thể dùng `x:Static` trong XAML. [S6]

**Với VS Code:** kiểm tra phần *VS Code Setup* trong [S6] để cấu hình sinh và compile class resource qua project; đừng mặc định designer của Visual Studio đã chạy. Chỉ file resource mặc định cần sinh class dùng để truy cập; không tạo một class trùng tên cho mỗi bản dịch.

Còn phải xét khai báo ngôn ngữ trong cấu hình nền tảng, ảnh/tên app địa phương hóa và RTL khi cần. Kiểm thử bằng cách đổi ngôn ngữ thiết bị; nguồn không khuyến nghị chỉ đổi `CurrentUICulture` trong code để kết luận đúng trên mọi nền tảng. [S6]

### 6.4. MAUI 10: safe area là một phần của thiết kế màn hình

`SafeAreaEdges` có trên `ContentPage`, layout như `Grid`, `ContentView`, `Border` và `ScrollView`. Phân biệt các lựa chọn, không sửa chồng nhiều giá trị `Padding` để che triệu chứng: [S15]

| Giá trị | Ý nghĩa |
| --- | --- |
| `None` | Cho nội dung tràn tới cạnh, kể cả vùng thanh hệ thống/bàn phím. |
| `Container` | Tránh thanh hệ thống/notch, không tự tránh bàn phím. |
| `SoftInput` | Tránh bàn phím, không tự tránh thanh hệ thống/notch. |
| `All` | Tôn trọng cả vùng hệ thống và bàn phím. |
| `Default` | Theo hành vi mặc định của control/nền tảng; không đồng nghĩa với `All`. |

Trong MAUI 10, `ContentPage` mặc định là `None`, layout mặc định là `Container`. Vì vậy cùng một page có thể có vùng nền và vùng nội dung xử lý safe area khác nhau. Với form nhập liệu, đặt chính sách rõ ràng rồi thử khi bàn phím mở/đóng, xoay màn hình và thay kích thước chữ. [S15]

**Riêng `ScrollView`:** chỉ `Container`/`None` có tác dụng trực tiếp theo nguồn; để tránh bàn phím, bọc nó trong layout và đặt `SoftInput` hoặc `All` trên container. Không mặc định `SafeAreaEdges="All"` trực tiếp trên `ScrollView` giải quyết được mọi trường hợp. [S15]

### 6.5. Khi đọc ví dụ dùng API cũ

| API/control gặp trong tài liệu cũ | Cách làm cho code mới trên MAUI 10 |
| --- | --- |
| `ListView`, các cell như `ViewCell`, `TableView` | Đã deprecated; ưu tiên `CollectionView` và template gồm view/layout, không bọc `ViewCell`. |
| `Page.IsBusy` | Đã obsolete; biểu diễn trạng thái tải bằng `ActivityIndicator` hoặc UI tương ứng. |
| `MessagingCenter` | Đã chuyển thành internal, không còn là API public để dùng trực tiếp; cân nhắc `WeakReferenceMessenger` từ package `CommunityToolkit.Mvvm`. |
| `ClickGestureRecognizer` | Đã bị loại bỏ; dùng `TapGestureRecognizer`. |
| `Compatibility.Layout` | Đã bị loại khỏi bản phát hành; dùng layout MAUI. |
| `Accelerator` | Đã bị loại bỏ; dùng `KeyboardAccelerator`. |

Các trạng thái deprecated/obsolete và bị loại bỏ **không giống nhau**; bảng dựa trên [S14]. Không cần thêm toolkit/messaging cho bài học nhỏ nếu DI, binding và command đã đủ. Nếu dùng messenger, phải cài package phù hợp và thiết kế việc đăng ký/hủy đăng ký có chủ đích.

## 7. Mở rộng UI: chọn đúng công cụ

### 7.1. BindableProperty: dành cho API của custom control

Khi property của control cần làm **target của binding**, nhận style/template hoặc hỗ trợ default/callback, dùng `BindableProperty`. Không cần biến mọi property của ViewModel thành `BindableProperty`; ViewModel thường dùng property CLR và thông báo thay đổi như trong `XAML.md`. [S7]

**Mẫu khai báo API, chưa định nghĩa phần hiển thị của control:**

```csharp
using Microsoft.Maui.Controls;

namespace FundamentalsDemo.Controls;

public class StatusBadge : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(StatusBadge),
            string.Empty);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
```

Nhớ quy ước `Text` ↔ `TextProperty`, đúng kiểu owner và wrapper `GetValue`/`SetValue`. Tạo/truy cập bindable property trên UI thread. Khi cần phản ứng với mọi đường cập nhật của hệ property, dùng callback `propertyChanged`; học `validateValue`, `coerceValue`, `defaultValueCreator` khi có yêu cầu tương ứng. [S7]

### 7.2. ControlTemplate khác DataTemplate ở đối tượng được mô tả

| Công cụ | Mô tả cái gì? | Khi dùng? |
| --- | --- | --- |
| `DataTemplate` | Giao diện của một item dữ liệu. | Một dòng trong danh sách, một thẻ dữ liệu. |
| `ControlTemplate` | Cấu trúc hiển thị của custom control/page. | Thay phần vỏ hiển thị nhưng giữ API/logic của control hoặc page. |

Control template dùng một view gốc. `TemplateBinding` nối tới property của **templated parent**, không mặc định tới ViewModel trang; `ContentPresenter` đánh dấu nơi chèn nội dung riêng. Chỉ lấy phần tử template bằng `GetTemplateChild` sau khi template được áp dụng, thường trong `OnApplyTemplate()`. [S11], [S12]

Nếu chỉ cần đổi màu, font hoặc khoảng cách, hãy xem resource/style có đủ không trước khi tự dựng một hệ template.

### 7.3. Behavior: tái sử dụng hành vi, không phải thay cấu trúc UI

`Behavior<T>` thêm chức năng cho control tương thích mà không phải subclass control đó. Đăng ký event trong `OnAttachedTo`, hủy đăng ký trong `OnDetachingFrom`. Attached behavior dùng attached property; platform behavior xử lý native control. [S3]

**Các bẫy đáng nhớ:** MAUI không tự gán `BindingContext` cho behavior; behavior có state không nên dùng chung tùy tiện; đừng mặc định pop page sẽ tháo behavior và cleanup. Nếu cần tháo, quản lý việc remove/clear và unsubscribe rõ ràng. [S3]

### 7.4. Trigger: khai báo phản ứng UI theo điều kiện

| Loại | Điều kiện kích hoạt | Ví dụ tư duy |
| --- | --- | --- |
| `Trigger` | Property của control đạt giá trị cụ thể. | Entry được focus thì đổi phần trình bày. |
| `DataTrigger` | Giá trị từ binding thỏa điều kiện. | Item hoàn thành thì gạch ngang tiêu đề. |
| `EventTrigger` | Một event xảy ra. | Chạy `TriggerAction<T>` khi có sự kiện. |
| `MultiTrigger` | Tất cả điều kiện đều đúng. | Nhiều tiêu chí UI đồng thời thỏa mãn. |
| State trigger | Điều kiện để áp dụng `VisualState`. | Đổi bố cục theo kích thước cửa sổ hoặc trạng thái. |

`EventTrigger` không tự hoàn tác hành động và không dùng `EnterActions`/`ExitActions`. Với binding `Text.Length`, xét giá trị ban đầu `null`. Trigger được chia sẻ qua resource có thể chia sẻ cả state; đừng nhầm một instance dùng chung thành bản độc lập cho từng control. [S13]

**Ranh giới:** trigger diễn tả phản ứng UI; quy tắc nghiệp vụ, lưu dữ liệu và xử lý lỗi vẫn cần vị trí thích hợp trong code/service.

## 8. Bài thực hành xuyên suốt: ứng dụng danh sách công việc

Không cần tự xây mọi tính năng của framework. Dùng một ứng dụng nhỏ để kiểm chứng quan hệ giữa các phần.

| Chặng | Việc làm | Bằng chứng đã hiểu |
| --- | --- | --- |
| 1 — Cấu trúc | Tạo project, xác định `MauiProgram`, `Resources`, `Platforms`; chọn nền tảng đầu tiên. | Thêm được một asset và hiểu vì sao nó có mặt trong gói build. |
| 2 — Luồng màn hình | Tổ chức danh sách/chi tiết bằng Shell; phác thảo route trước. | Đi tới chi tiết và quay lại đúng trạng thái mong muốn. |
| 3 — Phụ thuộc | Nối Page → ViewModel → Service bằng DI. | Thay service dữ liệu mẫu mà không sửa phần trình bày. |
| 4 — Dữ liệu/UI | Bind danh sách, đặt template theo kiểu item, gom màu/style dùng chung. | Thêm/sửa item làm UI đổi đúng; biết thread nào đang cập nhật collection. |
| 5 — Vòng đời | Ghi log lifecycle, thử đưa app ra nền rồi trở lại. | Không đăng ký event hoặc tải dữ liệu trùng ngoài ý muốn. |
| 6 — Chất lượng | Thử screen reader, chữ lớn, bản dịch thứ hai và một culture không hỗ trợ. | Có thể hoàn thành luồng chính; fallback và bố cục vẫn dễ hiểu. |
| 7 — Chỉ khi cần | Thêm trigger hoặc một behavior; tách custom control nếu xuất hiện nhu cầu tái sử dụng rõ. | Giải thích được tại sao công cụ đơn giản hơn chưa đủ. |

Không coi việc hoàn thành bài tập là đã sẵn sàng production: vẫn cần kiểm thử nền tảng, bảo mật, dữ liệu, đóng gói và phát hành theo sản phẩm thực tế.

## 9. Bảng gỡ lỗi có ROI cao

Đây là checklist suy ra từ các cơ chế trong nguồn, không phải danh sách đầy đủ mọi nguyên nhân.

| Hiện tượng | Kiểm tra đầu tiên |
| --- | --- |
| DI không tạo được page/ViewModel | Constructor và chuỗi đăng ký dependency, đã đăng ký trước `Build()` chưa? |
| Trạng thái màn hình bị dùng chung ngoài ý muốn | Lifetime singleton/scoped, đối tượng thực tế có được resolve lại không? |
| `StaticResource` không tìm thấy | Khóa, scope, thứ tự merge và thời điểm khởi tạo `App.xaml` so với page. |
| Danh sách hiện tên class hoặc binding sai | `ItemTemplate`, property của item và `x:DataType` bên trong template. |
| Collection gây lỗi UI khi tải dữ liệu | Có mutate collection đang bind từ background thread không? |
| Custom control không nhận binding như mong muốn | `BindableProperty` identifier, owner/type, wrapper và callback. |
| Behavior không nhận command hoặc còn giữ event | `BindingContext` và quá trình attach/detach/unsubscribe. |
| EventTrigger chạy nhưng không trở về trạng thái cũ | Nó không có cơ chế tự hoàn tác như trigger trạng thái. |
| Screen reader bỏ sót control con | Semantics của cha, thứ tự cây UI và khác biệt nền tảng. |
| VS Code không nhận class chuỗi dịch | Cấu hình sinh resource class, `EmbeddedResource` và file đưa vào `Compile`. |

## 10. Học sâu sau — có dấu hiệu quay lại rõ ràng

| Phần chuyên sâu | Khi nào cần? |
| --- | --- |
| Native/custom lifecycle events | Sự kiện `Window` chung chưa đáp ứng tích hợp nền tảng. |
| Scope DI tự quản lý | Cần một nhóm object có vòng đời được tạo/hủy rõ ràng. |
| DataTemplateSelector | Một loại dữ liệu cần nhiều kiểu trình bày; tái sử dụng template thay vì tạo mới ở mỗi lần chọn. |
| ControlTemplate và binding qua nhiều tầng | Có nhiều control/page chung cấu trúc nhưng khác dữ liệu hoặc nội dung chèn. |
| Platform behavior, validation/coercion callbacks | Hành vi native hoặc bất biến của custom control buộc phải xử lý sâu. |
| Localization ảnh/tên app/RTL đầy đủ | Các thị trường được hỗ trợ thực sự yêu cầu. |
| Nhiều state trigger | Cần bố cục thích ứng phức tạp; phải hiểu điều kiện và thứ tự ưu tiên. |

**Đích đến của 80/20:** biết chọn phần cần học tiếp dựa trên vấn đề đang giải quyết, không học theo số lượng API đã nhớ.

### Checklist kết thúc vòng đầu

- [ ] Phân biệt được file tài nguyên, resource XAML và resource localization.
- [ ] Giải thích được Shell tổ chức page, DI cung cấp object và binding nối UI với dữ liệu.
- [ ] Chọn lifetime và quản lý cập nhật collection trên UI thread có chủ đích.
- [ ] Phân biệt DataTemplate, ControlTemplate, Trigger, Behavior và BindableProperty.
- [ ] Kiểm tra được app khi mất focus, chạy nền và quay lại.
- [ ] Đã thử screen reader, chữ lớn, culture khác và fallback trên nền tảng đích.
- [ ] Biết phần nào đang học đủ dùng và điều kiện để quay lại học sâu.

## 11. Nguồn và phạm vi tổng hợp

| Mã | Tài liệu Microsoft Learn | Phần chính trong bản tổng hợp |
| --- | --- | --- |
| [S1] | Accessibility | Semantics, giới hạn nền tảng, kiểm thử khả năng tiếp cận. |
| [S2] | App lifecycle | Sự kiện Window, chuyển trạng thái, cleanup và mở rộng native. |
| [S3] | Behaviors | Hành vi tái sử dụng, BindingContext, attach/detach. |
| [S4] | Data binding | Đồng bộ dữ liệu và giới hạn auto-marshaling lên UI thread. |
| [S5] | Dependency injection | Registration, resolution, lifetime và bẫy khởi tạo resource. |
| [S6] | Localization | RESX, fallback, VS Code, platform setup và kiểm thử. |
| [S7] | Bindable properties | API của custom control, wrapper, callbacks, UI thread. |
| [S8] | Resource dictionaries | Scope, lookup, static/dynamic và merge. |
| [S9] | Shell overview | Cấu trúc màn hình, điều hướng và tìm kiếm. |
| [S10] | Single project | Resources, build actions, manifest, code nền tảng và entry point. |
| [S11] | Control templates | Vỏ hiển thị, templated parent và ContentPresenter. |
| [S12] | Data templates | UI của item, phạm vi tái sử dụng và template selector. |
| [S13] | Triggers | Property/data/event/multi/state triggers và giới hạn chia sẻ state. |

Nguồn bổ sung cho lần cập nhật MAUI 10: [S14] đối chiếu API/control thay đổi; [S15] giải thích safe area và giới hạn theo control.

**Lưu ý đọc nguồn:** trang có thể trả về nhiều khối `moniker`. Bản này giữ nội dung chung và phần áp dụng MAUI 10, không đưa thay đổi riêng MAUI 11 như Android Shell handler hoặc Windows UI Automation mới thành mặc định. Các trang con được nguồn dẫn tới không tự động được coi là đã đọc; cần mở tiếp khi triển khai chi tiết.

[S1]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/accessibility?view=net-maui-10.0
[S2]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle?view=net-maui-10.0
[S3]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/behaviors?view=net-maui-10.0
[S4]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/?view=net-maui-10.0
[S5]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection?view=net-maui-10.0
[S6]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/localization?view=net-maui-10.0
[S7]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/bindable-properties?view=net-maui-10.0
[S8]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/resource-dictionaries?view=net-maui-10.0
[S9]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/?view=net-maui-10.0
[S10]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/single-project?view=net-maui-10.0
[S11]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/controltemplate?view=net-maui-10.0
[S12]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/datatemplate?view=net-maui-10.0
[S13]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/triggers?view=net-maui-10.0
[S14]: https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0
[S15]: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/safe-area?view=net-maui-10.0
