# XAML trong .NET MAUI 10 — học theo phương pháp 80/20

> **Mục tiêu:** đọc hiểu một màn hình XAML, sửa UI, nối dữ liệu và xử lý thao tác bằng MVVM trước khi đào sâu các cơ chế ít gặp.
>
> **Cập nhật:** 16/09/2026. **Phạm vi:** **.NET MAUI 10 trên .NET 10**. Đã đối chiếu lại 7 nguồn ban đầu theo phiên bản 10, bổ sung tài liệu XAML compilation, compiled bindings, thay đổi phiên bản và safe area. Tính năng preview được ghi riêng, không trộn mặc định của MAUI 11 vào bài học.

## 1. Đúng tinh thần 80/20

**Ưu tiên phần kiến thức tạo ra nhiều khả năng sử dụng thực tế nhất, không phải chỉ học 20% rồi bỏ phần còn lại.** Tỷ lệ là định hướng, không phải cam kết định lượng.

Lộ trình, ví dụ và bài tập dưới đây là đề xuất của người tổng hợp. Dùng một màn hình nhỏ để kiểm chứng từng khái niệm, rồi mở rộng khi sản phẩm yêu cầu.

| Học trước | Đầu ra cần đạt | Nguồn ưu tiên |
| --- | --- | --- |
| Cấu trúc XAML và code-behind | Biết markup nối với class C# nào. | [S1], [S2] |
| Namespace và cú pháp thuộc tính | Đọc được control, layout, resource và thuộc tính gắn kèm. | [S3], [S6] |
| Binding và hướng dữ liệu | Hiểu UI lấy dữ liệu ở đâu, cập nhật theo chiều nào. | [S4] |
| MVVM và thông báo thay đổi | Sửa dữ liệu trong ViewModel và thấy UI cập nhật. | [S5] |
| Command | Đưa thao tác vào ViewModel, điều khiển lúc nào được thực hiện. | [S5] |
| Hot Reload | Rút ngắn vòng sửa UI, nhưng biết khi nào phải build lại. | [S7] |

## 2. Mô hình tư duy: XAML tạo đối tượng, C# xử lý logic

XAML là ngôn ngữ dựa trên XML để tạo đối tượng, gán thuộc tính và mô tả cây cha–con. Trong MAUI, nó thường định nghĩa giao diện; không bắt buộc dùng XAML nhưng đây là cách được tài liệu khuyến nghị. Không coi XAML là nơi viết vòng lặp hoặc toàn bộ nghiệp vụ. [S1]

```text
MainPage.xaml + MainPage.xaml.cs
               |
               v
    Cùng định nghĩa class MainPage
               |
               v
 InitializeComponent() khởi tạo cây UI từ XAML
               |
               v
 BindingContext cung cấp đối tượng dữ liệu cho binding
```

- `x:Class` phải khớp namespace và tên class `partial` trong code-behind.
- `InitializeComponent()` nối các phần XAML/C# và khởi tạo các đối tượng giao diện.
- `ContentPage` có một nội dung trực tiếp; muốn nhiều control, đặt chúng trong một layout.
- `Clicked="OnClicked"` nối tới handler C#; không phải một biểu thức binding. [S2]

**Không cực đoan hóa MVVM:** code-behind không bị cấm. Trong lộ trình này, thao tác gắn với dữ liệu đi qua ViewModel/Command để dễ theo dõi; không ép mọi hành vi UI thành nghiệp vụ.

### 2.1. MAUI 10: phân biệt ba cách tạo cây UI từ XAML

| Cơ chế | Cách hiểu | Cấu hình cần nhớ |
| --- | --- | --- |
| Runtime inflation | Đọc XAML và tạo object lúc chạy. | Mặc định của cấu hình `Debug` khi không override inflator. |
| XamlC | Biên dịch XAML thành IL. | Mặc định của cấu hình `Release` khi không override inflator. |
| XAML Source Generation | Sinh C# từ XAML lúc build. | MAUI 10 cho phép bật `MauiXamlInflator=SourceGen` cho cả Debug và Release. |

Nguồn [S8] phân biệt các cơ chế trên. **SourceGen chưa phải mặc định của mọi project MAUI 10**, nhưng project `MauiApp1` trong repo này đã bật:

```xml
<PropertyGroup>
    <MauiXamlInflator>SourceGen</MauiXamlInflator>
</PropertyGroup>
```

Vẫn dùng `partial`, `x:Class` và `InitializeComponent()`; không tự sửa code được sinh. Khi cần ngoại lệ cho một file, dùng metadata của `MauiXaml` thay vì mặc định tìm hướng dẫn `[XamlCompilation]` cũ: [S8]

```xml
<ItemGroup>
    <MauiXaml Update="Views/LegacyPage.xaml" Inflator="Default" />
</ItemGroup>
```

Đây là ví dụ cấu hình; thay đường dẫn bằng file thật. `Default` trả file đó về mặc định theo cấu hình build, không tắt XAML cho toàn project. Đổi inflator cần build lại. **XAML Source Generation không đồng nghĩa với compiled bindings**: một bên tạo cây UI, bên kia biên dịch đường binding theo kiểu dữ liệu. [S8], [S9]

## 3. Bộ cú pháp đáng học thuộc trước

### 3.1. Namespace và các tên dễ nhầm

| Cú pháp | Ý nghĩa cần nhớ |
| --- | --- |
| `xmlns="http://schemas.microsoft.com/dotnet/2021/maui"` | Các thẻ không có prefix, như `Label`, thuộc namespace MAUI. |
| `xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"` | Khai báo các thành phần của ngôn ngữ XAML. |
| `xmlns:vm="clr-namespace:MauiXamlDemo.ViewModels"` | Cho phép tham chiếu class C# qua prefix `vm`. |
| `x:Class="MauiXamlDemo.MainPage"` | Class được XAML này góp phần định nghĩa. |
| `x:Name="volumeSlider"` | Tên một đối tượng UI để tham chiếu. |
| `x:DataType="vm:GreetingViewModel"` | Kiểu dữ liệu dự kiến cho binding; **không tạo ViewModel hoặc gán BindingContext**. |
| `x:Key="AccentColor"` | Khóa để tra resource, không phải tên control. |

Các URI trong `xmlns` là định danh namespace, không phải địa chỉ XAML phải tải về lúc chạy. Prefix như `vm`/`local` là tên quy ước; quan trọng là ánh xạ đúng namespace. Class ở assembly khác cần thêm `;assembly=TenAssembly`, không kèm `.dll`, và project phải tham chiếu assembly đó. [S2], [S6]

### 3.2. Nhìn cú pháp để nhận ra vai trò

| Ví dụ | Cách đọc |
| --- | --- |
| `<Label Text="Xin chào" />` | Tạo object `Label`, đặt property bằng attribute. |
| `<Label.TextColor>Blue</Label.TextColor>` | Property element: đặt property bằng một phần tử XML. |
| `<Label Grid.Row="1" Grid.Column="0" />` | Attached property: `Grid` định nghĩa thuộc tính nhưng nó được đặt trên control con. |
| Control nằm trực tiếp bên trong `ContentPage` | Được gán vào content property; không phải lúc nào cũng cần viết `<ContentPage.Content>`. |
| `{Binding Name}` | Markup extension: lấy dữ liệu qua binding, không phải chuỗi chữ cố định. |
| `{StaticResource AccentColor}` | Tra object/value theo khóa trong resource dictionary. |
| `{x:Reference volumeSlider}` | Tham chiếu đối tượng mang `x:Name` tương ứng. |
| `{x:Static local:Palette.All}` | Lấy thành viên static; không có nghĩa giá trị tự cập nhật liên tục. |

Property element hữu ích khi giá trị là object hoặc collection phức tạp. Với `Grid`, hàng/cột bắt đầu từ 0; `Grid.RowSpan`/`Grid.ColumnSpan` dùng để chiếm nhiều ô. [S3] Các dạng markup extension được dùng trong binding và MVVM ở [S4], [S5].

**Nếu cần khác biệt nền tảng:** đọc `OnPlatform` + `On`, dùng đúng tên `Android`, `iOS`, `MacCatalyst`, **`WinUI` — không phải `Windows`**. Giá trị phân biệt hoa/thường. `x:TypeArguments` chỉ kiểu của giá trị; bốn số `Padding`/`Margin` có thứ tự trái, trên, phải, dưới. [S3]

### 3.3. Global và implicit namespaces: tùy chọn preview của MAUI 10

Không cần đổi các namespace tường minh ở mục 3.1 để học MAUI 10. Chúng vẫn hợp lệ và được giữ trong các mẫu của bộ docs này. Có hai tùy chọn khác nhau: [S2], [S6]

- **Global XML namespace:** dùng `http://schemas.microsoft.com/dotnet/maui/global` ở root và các assembly attribute `XmlnsDefinition` trong `GlobalXmlns.cs` để gom namespace CLR. Nguồn [S2] đánh dấu tính năng này là preview và yêu cầu `EnablePreviewFeatures=true`. Global namespace không tự xóa mọi khai báo hoặc tự giải quyết tên class trùng nhau.
- **Implicit namespace declarations:** cho phép bỏ hai khai báo chuẩn `xmlns` và `xmlns:x`. Trong MAUI 10, đây là preview phải opt-in, không phải mặc định. Namespace của class/thư viện tùy biến vẫn cần khai báo khi chưa được ánh xạ phù hợp. [S6]

Chỉ khi chủ động thử implicit namespaces mới thêm cấu hình sau; không cần bật để chạy các ví dụ trong tài liệu:

```xml
<PropertyGroup>
    <DefineConstants>$(DefineConstants);MauiAllowImplicitXmlnsDeclaration</DefineConstants>
    <EnablePreviewFeatures>true</EnablePreviewFeatures>
</PropertyGroup>
```

**Ranh giới phiên bản:** implicit namespaces mặc định của MAUI 11 không áp dụng cho MAUI 10. Khi xem nguồn có nhiều khối `moniker`, đọc đúng phần opt-in của phiên bản 10. [S6]

## 4. Binding: luôn hỏi bốn câu

1. **Source là object nào?** Thường là `BindingContext`, có thể được kế thừa từ cha xuống con.
2. **Path là property nào?** `{Binding Name}` là cách viết ngắn của `{Binding Path=Name}`.
3. **Dữ liệu đi theo hướng nào?** Xem `Mode`, đừng mặc định mọi binding đều hai chiều.
4. **Khi dữ liệu đổi, ai thông báo?** ViewModel thường phát `PropertyChanged` để binding cập nhật UI. [S4], [S5]

| Mode | Chiều dữ liệu | Cách dùng trong lúc học |
| --- | --- | --- |
| `OneWay` | Source → target | Hiển thị thông tin. |
| `TwoWay` | Source ↔ target | Nhập/sửa dữ liệu. |
| `OneWayToSource` | Target → source | Chỉ đẩy giá trị về source. |
| `OneTime` | Đọc từ source khi BindingContext được thiết lập/thay đổi | Không theo dõi từng lần property nguồn đổi. |
| `Default` | Theo mặc định của property đích | Không đồng nghĩa với `TwoWay`. |

Target của binding phải là **bindable property**; không phải mọi property trên control đều bind được. Property nguồn trên ViewModel không cần biến thành `BindableProperty`. [S4], [S5]

### Phân biệt những thứ trông giống nhau

- **`BindingContext` ≠ `x:DataType`:** cái đầu là object thực tế lúc chạy, cái sau mô tả kiểu cho binding. Mẫu ở mục 6 dùng cả hai. [S4], [S6]
- **Định dạng ≠ chuyển đổi:** dùng `StringFormat` cho cách trình bày, chẳng hạn `StringFormat='Giá trị: {0:F1}'`; dùng `IValueConverter` khi cần đổi giá trị/kiểu. `Convert` đi source → target, `ConvertBack` đi chiều ngược lại. [S4]
- **Đổi collection ≠ đổi item:** `ObservableCollection<T>` thông báo thay đổi tập hợp; property bên trong từng item vẫn cần thông báo riêng qua `INotifyPropertyChanged`. [S4]

Với danh sách, học ba điểm: `ItemsSource` cung cấp dữ liệu, `ItemTemplate` dùng `DataTemplate` để mô tả từng dòng, binding trong template trỏ tới **item hiện tại**. `x:DataType` của template phải phù hợp kiểu item, không vô tình giữ kiểu ViewModel của trang. [S4]

**Viết mới theo MAUI 10:** dùng `CollectionView` với view/layout trực tiếp trong `DataTemplate`, không bọc bằng `ViewCell`. Một số ví dụ ở [S4] vẫn dùng `ListView`/`ViewCell` để giải thích binding, nhưng các control này đã deprecated trong MAUI 10. Mẫu danh sách trong `Fundamentals.md` dùng `CollectionView`. [S10]

**Compiled bindings:** khai báo `x:DataType` đúng ở mỗi nơi context đổi để kiểm tra đường binding lúc build; không dùng nó thay `BindingContext`. Với XamlC, binding đặt `Source` tường minh cần bật `MauiEnableXamlCBindingWithSourceCompilation=true` nếu muốn biên dịch loại binding đó, rồi kiểm tra lại kiểu source. Không mặc định mọi binding được biên dịch chỉ vì đã bật SourceGen. Khi dùng full trimming/NativeAOT, đọc kỹ giới hạn của [S9].

## 5. MVVM tối thiểu để làm được việc

```text
View (XAML) ← binding → ViewModel (property + command) → Model
```

- **View:** trình bày và tương tác.
- **ViewModel:** trạng thái và thao tác mà màn hình sử dụng; không cần biết tên control.
- **Model:** dữ liệu/nghiệp vụ nền. Ví dụ học nhỏ có thể chưa cần một lớp Model riêng. [S5]

Hai cơ chế cần nắm:

| Cơ chế | Khi nào dùng? | Lỗi dễ gặp |
| --- | --- | --- |
| `INotifyPropertyChanged` | Property nguồn thay đổi và UI cần biết. | Chỉ đổi backing field, không phát thông báo; quên thông báo property phụ thuộc. |
| `ICommand` / `Command` | Người dùng yêu cầu thực hiện thao tác. | Có `CanExecute` nhưng không gọi `ChangeCanExecute()` khi điều kiện thay đổi. |

`Command` của MAUI triển khai `ICommand`: `Execute` thực hiện thao tác, `CanExecute` quyết định có được chạy, `CanExecuteChanged` báo phải đánh giá lại. `CommandParameter` truyền dữ liệu cho command. So sánh giá trị cũ/mới trước khi phát `PropertyChanged` giúp tránh cập nhật lặp không cần thiết. [S5]

## 6. Ví dụ xuyên suốt: nhập tên → lời chào → nút xóa

**Ví dụ do người tổng hợp viết**, nhằm nối các khái niệm ở [S2], [S4], [S5], [S6]; không phải nguyên văn source. Đặt trong project MAUI có namespace `MauiXamlDemo`, giữ cấu hình template để mở `MainPage`. Nếu project mang tên khác, đổi đồng bộ namespace C#, `x:Class` và `xmlns:vm`.

### 6.1. `MainPage.xaml` — View

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:MauiXamlDemo.ViewModels"
             x:Class="MauiXamlDemo.MainPage"
             x:DataType="vm:GreetingViewModel"
             SafeAreaEdges="All"
             Title="XAML 80/20">
    <ContentPage.Resources>
        <Color x:Key="AccentColor">#2563EB</Color>
    </ContentPage.Resources>

    <VerticalStackLayout Padding="24" Spacing="12">
        <Entry Placeholder="Nhập tên"
               Text="{Binding Name, Mode=TwoWay}" />
        <Label Text="{Binding Greeting}"
               TextColor="{StaticResource AccentColor}"
               FontSize="24" />
        <Button Text="Xóa tên"
                Command="{Binding ClearCommand}" />
    </VerticalStackLayout>
</ContentPage>
```

Mẫu đặt `SafeAreaEdges="All"` rõ ràng để tránh vùng thanh hệ thống và bàn phím theo cơ chế MAUI 10; đọc thêm giới hạn và kiểm thử bố cục trong `Fundamentals.md`. [S11]

### 6.2. `MainPage.xaml.cs` — nối View với ViewModel

```csharp
using Microsoft.Maui.Controls;
using MauiXamlDemo.ViewModels;

namespace MauiXamlDemo;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new GreetingViewModel();
    }
}
```

### 6.3. `ViewModels/GreetingViewModel.cs` — trạng thái và thao tác

```csharp
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace MauiXamlDemo.ViewModels;

public class GreetingViewModel : INotifyPropertyChanged
{
    private string name = string.Empty;

    public GreetingViewModel()
    {
        ClearCommand = new Command(
            () => Name = string.Empty,
            () => !string.IsNullOrEmpty(Name));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Command ClearCommand { get; }

    public string Name
    {
        get => name;
        set
        {
            var newName = value ?? string.Empty;
            if (name == newName)
                return;

            name = newName;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Greeting));
            ClearCommand.ChangeCanExecute();
        }
    }

    public string Greeting => string.IsNullOrWhiteSpace(Name)
        ? "Nhập tên để bắt đầu."
        : $"Chào {Name}!";

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

### 6.4. Tự lần theo luồng, không chỉ chép code

1. Code-behind tạo ViewModel và đặt `BindingContext`; các control con nhận context đó.
2. Gõ vào `Entry` → `TwoWay` cập nhật `Name`.
3. Setter thông báo cả `Name` và `Greeting`, vì lời chào phụ thuộc tên.
4. `ChangeCanExecute()` yêu cầu nút đánh giá lại điều kiện có tên để xóa.
5. Bấm nút → command xóa tên → binding cập nhật lại cả ô nhập và lời chào.

**Hành vi cần kiểm chứng khi chạy:** tên rỗng thì nút bị vô hiệu hóa; nhập tên thì lời chào đổi và nút được bật; bấm xóa thì UI trở về trạng thái đầu.

Đây là mẫu minh họa cho project MAUI, không phải chương trình C# chạy độc lập. Chưa build/chạy ứng dụng native trong phiên tổng hợp tài liệu này.

## 7. Hot Reload: tăng tốc sửa UI, không thay thế build

Nguồn ghi nhận XAML Hot Reload có trong Visual Studio và VS Code. Điều kiện cốt lõi: ứng dụng chạy với debugger được gắn, cấu hình tên **`Debug`**, XAML hợp lệ. [S7]

Quy trình đề xuất với mẫu trên: chạy debug → đổi `FontSize`, màu hoặc `Spacing` → xem kết quả → build/run lại sau khi chốt thay đổi.

**Nhớ giới hạn:**

- XAML Hot Reload không tự reload C#; handler mới hoặc `x:Name` mới dùng từ C# cần phía code được reload/build tương ứng.
- Thêm, xóa, đổi tên file hoặc thay NuGet package cần build và triển khai lại.
- Lỗi `XHR` phải được sửa trước khi thay đổi XAML được áp dụng.
- Full-page reload có thể mất trạng thái như focus/selection. [S7]

**Đúng phiên bản 10:** không thêm `EnableMauiIncrementalHotReload` hoặc coi `dotnet watch` với engine XAML incremental mới là luồng mặc định; phần đó trong [S7] thuộc MAUI 11. Với project bật SourceGen, vẫn kiểm tra hành vi Hot Reload thực tế và build lại khi thay cấu hình inflator; không suy ra khả năng reload chỉ từ việc build thành công. [S7], [S8]

**Khi không thấy cập nhật:** kiểm tra Debug/debugger → lưu file và sửa lỗi → xem log → thử build/run. Nguồn còn hướng dẫn kiểm tra encoding **UTF-8 with BOM của file `.cs`** khi Hot Reload lỗi im lặng. Đây là lưu ý chẩn đoán của trang, không phải yêu cầu đổi encoding toàn bộ repository. Các đường dẫn menu Visual Studio trong nguồn không nên áp nguyên sang VS Code. [S7]

## 8. Bảng gỡ lỗi có ROI cao

Đây là checklist thực hành rút ra từ các cơ chế đã học, không phải danh sách đầy đủ mọi nguyên nhân.

| Hiện tượng | Kiểm tra trước |
| --- | --- |
| XAML không nhận class của mình | `clr-namespace`, tên class, quyền truy cập và assembly reference. |
| Không khớp XAML/code-behind | `x:Class`, namespace, tên class `partial`, lời gọi `InitializeComponent()`. |
| Có UI nhưng binding rỗng | Object `BindingContext` thực tế và tên property trong `Path`. |
| Khai báo `x:DataType` nhưng không có dữ liệu | Đã tạo và gán ViewModel chưa? Khai báo kiểu không tạo object. |
| Dữ liệu đổi nhưng lời chào không đổi | Có phát `PropertyChanged` cho property phụ thuộc không? |
| UI nhập được nhưng ViewModel không đổi | Hướng binding có phù hợp không? Property nguồn có setter không? |
| Nút không đổi trạng thái bật/tắt | `CanExecute` và thời điểm gọi `ChangeCanExecute()`. |
| Thêm item hoặc sửa item mà danh sách không đổi | Phân biệt thông báo của collection và thông báo của từng item. |
| Template đọc nhầm property | Kiểm tra binding context và `x:DataType` của item template. |

## 9. Thực hành tiếp và phần học sâu sau

### Bài tập có đầu ra rõ ràng

- **Vòng 1:** thay tên project trong mẫu và sửa namespace cho đúng; đổi layout nhưng giữ nguyên binding.
- **Vòng 2:** thêm property `CharacterCount`; tự xác định lúc nào phải thông báo nó thay đổi.
- **Vòng 3:** đổi điều kiện nút xóa thành chỉ bật khi tên có ít nhất 3 ký tự; kiểm tra cả nhập và xóa.
- **Vòng 4:** đọc phần collection của [S4], thêm một danh sách nhỏ; phân biệt thêm item với đổi property của item.
- **Vòng 5:** thử Hot Reload với thay đổi UI, rồi kiểm tra lại bằng build/run.

### Chưa cần học hết ngay

| Chủ đề | Khi nào quay lại? |
| --- | --- |
| Converter phức tạp, nhiều `ConvertBack` | Khi binding cần chuyển đổi mà format đơn giản không đủ. |
| `x:Arguments`, `x:FactoryMethod`, modifier, namespace tùy biến | Khi việc tạo object hoặc tái sử dụng thư viện có yêu cầu cụ thể. |
| `OnPlatform` chi tiết | Khi khác biệt giữa các nền tảng đã được quan sát hoặc yêu cầu rõ. |
| Tổ chức MVVM quy mô lớn | Khi số màn hình, nghiệp vụ và phụ thuộc vượt khả năng của mẫu nhỏ. |
| Chẩn đoán Hot Reload nâng cao | Khi quy trình debug cơ bản không giải thích được lỗi. |

**Không bỏ qua vĩnh viễn:** một chủ đề ít gặp vẫn nên học sớm nếu nó là điều kiện bắt buộc của sản phẩm.

### Checklist kết thúc vòng học đầu

- [ ] Đọc được `xmlns`, `x:Class`, `x:Name`, `x:Key`, `x:DataType` mà không nhầm vai trò.
- [ ] Phân biệt attribute, property element, attached property và content property.
- [ ] Xác định được source, path, mode và cơ chế thông báo của một binding.
- [ ] Tự giải thích và sửa được mẫu nhập tên mà không thêm thao tác trực tiếp lên control từ ViewModel.
- [ ] Biết vì sao property phụ thuộc và trạng thái command cần được thông báo lại.
- [ ] Biết giới hạn Hot Reload và kiểm chứng lại bằng build/run.
- [ ] Phân biệt SourceGen, XamlC và compiled bindings; biết project đang dùng inflator nào.
- [ ] Không nhầm global/implicit namespaces preview với cấu hình mặc định của MAUI 10.

## 10. Nguồn gốc và phạm vi

| Mã | Tài liệu | Vai trò |
| --- | --- | --- |
| [S1] | XAML | Khái niệm, vai trò và giới hạn của markup. |
| [S2] | Get started with .NET MAUI XAML | Cấu trúc file, code-behind, khởi tạo UI và event. |
| [S3] | Essential .NET MAUI XAML syntax | Property element, attached/content property, khác biệt nền tảng. |
| [S4] | Data binding basics | Source/target, mode, template, collection và converter. |
| [S5] | Data binding and MVVM | ViewModel, thông báo thay đổi và commanding. |
| [S6] | XAML namespaces | Namespace và các cấu trúc `x:`. |
| [S7] | XAML Hot Reload | Điều kiện chạy, giới hạn và gỡ lỗi Hot Reload. |
| [S8] | XAML compilation | Runtime, XamlC, SourceGen và cấu hình inflator theo project/file. |
| [S9] | Compiled bindings | Kiểu binding, binding có Source và giới hạn trimming/AOT. |
| [S10] | What's new in .NET MAUI for .NET 10 | Đối chiếu control deprecated và thay đổi phiên bản. |
| [S11] | Safe area layout | Cấu hình SafeAreaEdges cho mẫu nhập liệu MAUI 10. |

**Lưu ý phiên bản:** phản hồi Microsoft Learn có thể chứa nhiều khối `moniker`, dù URL yêu cầu MAUI 10. Bản này đã đưa SourceGen và các tùy chọn namespace của MAUI 10 vào đúng phạm vi, ghi rõ preview/opt-in. Không lấy implicit namespaces mặc định, `x:Code` hoặc incremental Hot Reload riêng MAUI 11 làm hướng dẫn cho MAUI 10.

[S1]: https://learn.microsoft.com/en-us/dotnet/maui/xaml/?view=net-maui-10.0
[S2]: https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/get-started?view=net-maui-10.0
[S3]: https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/essential-syntax?view=net-maui-10.0
[S4]: https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/data-binding-basics?view=net-maui-10.0
[S5]: https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/mvvm?view=net-maui-10.0
[S6]: https://learn.microsoft.com/en-us/dotnet/maui/xaml/namespaces/?view=net-maui-10.0
[S7]: https://learn.microsoft.com/en-us/dotnet/maui/xaml/hot-reload?view=net-maui-10.0
[S8]: https://learn.microsoft.com/en-us/dotnet/maui/xaml/xamlc?view=net-maui-10.0
[S9]: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/compiled-bindings?view=net-maui-10.0
[S10]: https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0
[S11]: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/safe-area?view=net-maui-10.0
