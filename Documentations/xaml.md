# .NET MAUI XAML — 80/20 Learning Roadmap

> Mục tiêu: nắm **20% XAML cốt lõi** để đọc, viết, debug và maintain phần lớn UI ứng dụng .NET MAUI 10.
>
> Phạm vi:
>
> - XAML fundamentals
> - Essential syntax
> - Properties / attributes
> - Layout
> - Data binding
> - MVVM
> - XAML namespaces
> - Hot Reload
>
> Không học hết XAML specification; ưu tiên phần dùng thường xuyên trong project thực tế.

# 1. XAML là gì?

**XAML (eXtensible Application Markup Language)** là ngôn ngữ markup khai báo UI và object graph.

Trong .NET MAUI: `XAML → .NET MAUI UI → Native platform`

```xml
<VerticalStackLayout>
    <Label Text="Hello MAUI" />
    <Button Text="Click me" />
</VerticalStackLayout>
```

XAML mô tả: `What the UI looks like` + `How objects are structured` + `How properties are configured` + `How UI connects to data`

Còn C# thường xử lý: `Behavior`; `Business logic`; `Events`; `Services`; `Application state`

# 2. Mental Model 80/20

Chỉ cần nhớ: `XAML`:
- `Element → {Object / Control}`
- `Attribute → {Property}`
- `Property Element → {Complex property value}`
- `Content → {Child object}`
- `Resource → {Shared object}`
- `Binding → {Connect UI ↔ data}`
- `Namespace → {Tell XAML where types come from}`

```xml
<Button
    Text="Save"
    BackgroundColor="Blue"
    Command="{Binding SaveCommand}" />
```

Có thể đọc như: `Button → {Text            → property; BackgroundColor → property; Command         → binding expression}`

# 3. XAML ↔ C# mapping

Đây là concept cốt lõi để đọc XAML.

XAML:

```xml
<Label
    Text="Hello"
    FontSize="24" />
```

Tư duy tương đương:

```csharp
var label = new Label();

label.Text = "Hello";
label.FontSize = 24;
```

XAML:

```xml
<Button Text="Save" />
```

Tương đương conceptually:

```csharp
var button = new Button
{
    Text = "Save"
};
```

Vì vậy:

> **XAML không phải magic UI language mà là cách khai báo để tạo/cấu hình objects.**

# 4. Element

Mỗi XML element thường là một object/control.

```xml
<Label />
```

→ `Label`

```xml
<Button />
```

→ `Button`

```xml
<Grid />
```

→ `Grid`

```xml
<VerticalStackLayout />
```

→ `VerticalStackLayout`

Nested elements tạo object hierarchy:

```xml
<VerticalStackLayout>

    <Label Text="Name" />

    <Entry />

    <Button Text="Save" />

</VerticalStackLayout>
```

`VerticalStackLayout → {Label; Entry; Button}`

# 5. Attribute = Property

Đây là pattern XAML phổ biến nhất.

```xml
<Label
    Text="Hello"
    FontSize="20"
    HorizontalOptions="Center" />
```

Mỗi attribute thường map vào property: `Text`; `FontSize`; `HorizontalOptions`

### 80/20 properties cần nhớ

## Layout

`Margin`; `Padding`; `WidthRequest`; `HeightRequest`; `MinimumWidthRequest`; `MinimumHeightRequest`; `MaximumWidthRequest`; `MaximumHeightRequest`; `HorizontalOptions`; `VerticalOptions`

## Visual

`BackgroundColor`; `Opacity`; `IsVisible`; `IsEnabled`

## Text

`Text`; `FontSize`; `FontAttributes`; `TextColor`; `HorizontalTextAlignment`; `VerticalTextAlignment`

## Input

`Text`; `Placeholder`; `IsPassword`; `Keyboard`

# 6. Content Property

Control có thể nhận child trực tiếp.

```xml
<ContentPage>
    <VerticalStackLayout>
        ...
    </VerticalStackLayout>
</ContentPage>
```

Không nhất thiết phải viết:

```xml
<ContentPage.Content>
    <VerticalStackLayout>
        ...
    </VerticalStackLayout>
</ContentPage.Content>
```

Bởi vì `ContentPage.Content` là content property.

`ContentPage → {Content → {VerticalStackLayout}}`

Nhờ đó XAML ngắn hơn C#.

# 7. Property Element Syntax

Property có cấu trúc phức tạp: dùng property element.

```xml
<Label>
    <Label.Text>
        Hello
    </Label.Text>
</Label>
```

Trường hợp thực tế hữu ích hơn:

```xml
<Button>
    <Button.Command>
        <Binding Path="SaveCommand" />
    </Button.Command>
</Button>
```

Hoặc:

```xml
<ContentPage.Resources>
    <ResourceDictionary>
        ...
    </ResourceDictionary>
</ContentPage.Resources>
```

`Element.Property`

`Grid.RowDefinitions`; `Grid.ColumnDefinitions`; `ContentPage.Resources`; `Button.Command`

# 8. Collections trong XAML

Một số property là collection.

```xml
<Grid.RowDefinitions>
    <RowDefinition Height="Auto" />
    <RowDefinition Height="*" />
</Grid.RowDefinitions>
```

`Grid → {RowDefinitions → {RowDefinition; RowDefinition}}`

Hoặc:

```xml
<HorizontalStackLayout>
    <Label />
    <Button />
    <Image />
</HorizontalStackLayout>
```

Child elements được thêm vào collection children.

# 9. Attached Properties

Control có thể nhận property do object khác định nghĩa.

Ví dụ với Grid:

```xml
<Button
    Grid.Row="1"
    Grid.Column="2" />
```

`Grid.Row` là attached property của Grid, không phải property của `Button`.

`Button → {Grid.Row = 1}`

Các attached properties rất quan trọng: `Grid.Row`; `Grid.Column`; `Grid.RowSpan`; `Grid.ColumnSpan`; `AbsoluteLayout.LayoutBounds`; `AbsoluteLayout.LayoutFlags`

### Đừng nhầm

```xml
<Button
    WidthRequest="100"
    Grid.Row="1" />
```

```text
WidthRequest → Button property
Grid.Row     → Grid attached property
```

# 10. Markup Extensions

Markup extension tạo/resolve giá trị đặc biệt trong XAML.

Pattern:

```xml
Property="{Something ...}"
```

```xml
Text="{Binding Name}"
```

```xml
TextColor="{StaticResource PrimaryColor}"
```

Một số loại cần biết: `Binding`; `StaticResource`; `DynamicResource`; `x:Static`; `x:Reference`; `x:Type`

80/20:

> Trước tiên đọc/dùng markup extension có sẵn; chưa cần tự viết.

# 11. Resources

Resources dùng để chia sẻ object/value trong XAML.

```xml
<ContentPage.Resources>
    <ResourceDictionary>

        <Color x:Key="PrimaryColor">
            #512BD4
        </Color>

    </ResourceDictionary>
</ContentPage.Resources>
```

Sử dụng:

```xml
<Button
    BackgroundColor="{StaticResource PrimaryColor}" />
```

```text
ResourceDictionary
       ├── key → object
       └── lookup
             ↓
       StaticResource
```

# 12. StaticResource vs DynamicResource

## StaticResource

```xml
BackgroundColor="{StaticResource PrimaryColor}"
```

Resolve resource trong quá trình XAML loading/resource lookup.

Dùng phổ biến cho: `Colors`; `Styles`; `Converters`; `Templates`

## DynamicResource

```xml
BackgroundColor="{DynamicResource PrimaryColor}"
```

Dùng để property phản ứng khi resource đổi.

`Theme change → Resource changes → UI updates`

80/20:

- StaticResource → default choice.
- DynamicResource → cần runtime resource changes.

# 13. XAML Namespaces

Phần quan trọng, dễ nhầm.

Ở đầu file:

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
```

### Default namespace

```xml
xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
```

Cho phép:

```xml
<Label />
<Button />
<Grid />
```

### XAML language namespace

```xml
xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
```

Cho phép:

```xml
x:Class
x:Name
x:Key
x:Type
x:Reference
x:Static
```

# 14. `x:Class`

```xml
<ContentPage
    x:Class="MyApp.MainPage">
```

Liên kết XAML với C# code-behind:

```csharp
namespace MyApp;

public partial class MainPage : ContentPage
{
}
```

`MainPage.xaml — x:Class → MainPage.xaml.cs`

Đây là lý do thường có cặp: `MainPage.xaml`; `MainPage.xaml.cs`

# 15. `x:Name`

Cho phép đặt tên cho object.

```xml
<Entry x:Name="NameEntry" />
```

Sau đó code-behind có thể tham chiếu:

```csharp
var name = NameEntry.Text;
```

Nhưng trong architecture MVVM:

> Không lạm dụng `x:Name` cho ViewModel điều khiển View trực tiếp.

Ưu tiên: `Binding`; `Command`; `MVVM`

thay vì: `View reference`

# 16. `x:Key`

Đặt key cho resource.

```xml
<Color
    x:Key="PrimaryColor">
    #512BD4
</Color>
```

Dùng:

```xml
BackgroundColor="{StaticResource PrimaryColor}"
```

`x:Key → ResourceDictionary → StaticResource`

# 17. `x:Type`

Tham chiếu đến CLR type.

```xml
<DataTemplate
    x:DataType="models:Product">
```

Trong MAUI hiện đại, `x:DataType` rất quan trọng cho compiled bindings.

# 18. `x:DataType` — cực kỳ quan trọng

```xml
<ContentPage
    x:DataType="viewmodels:MainViewModel">
```

Binding:

```xml
<Label Text="{Binding UserName}" />
```

Nếu ViewModel:

```csharp
public string UserName { get; set; }
```

XAML biết type của binding source.

Lợi ích:

- Without x:DataType → runtime binding.
- With x:DataType → compiled binding → better performance, better compile-time checking.

### CollectionView

```xml
<CollectionView
    ItemsSource="{Binding Products}">

    <CollectionView.ItemTemplate>

        <DataTemplate
            x:DataType="models:Product">

            <Label
                Text="{Binding Name}" />

        </DataTemplate>

    </CollectionView.ItemTemplate>

</CollectionView>
```

Nên nắm chắc pattern này.

# 19. Data Binding — 20% kiến thức quan trọng nhất

Binding là cầu nối: `View — Binding → ViewModel`

```xml
<Label
    Text="{Binding UserName}" />
```

Nếu BindingContext là:

```csharp
public class MainViewModel
{
    public string UserName { get; set; }
        = "Giang";
}
```

UI hiển thị: `Giang`

# 20. BindingContext

Binding tìm source qua `BindingContext`, không tự biết nguồn data.

```csharp
BindingContext = new MainViewModel();
```

Sau đó:

```xml
<Label Text="{Binding UserName}" />
```

`Page.BindingContext → MainViewModel.UserName → Label`

Nếu Binding không hoạt động:

> **Kiểm tra BindingContext trước.**

# 21. OneWay Binding

Default scenario: `ViewModel → View`

```xml
<Label Text="{Binding UserName}" />
```

Khi ViewModel thay đổi: `UserName → PropertyChanged → Label updates`

Đây là pattern cực kỳ phổ biến.

# 22. TwoWay Binding

Hai chiều: `ViewModel ↔ View`

```xml
<Entry
    Text="{Binding UserName, Mode=TwoWay}" />
```

User nhập: `Entry → Binding → UserName`

ViewModel thay đổi: `UserName → Binding → Entry`

Thường dùng cho: `Entry`; `Switch`; `Picker`; `CheckBox`

# 23. Binding Mode

80/20: `OneWay`; `TwoWay`; `OneTime`; `OneWayToSource`

Không cần học thuộc mọi trường hợp ngay.

Hãy nhớ:

- OneWay: ViewModel → View.
- TwoWay: ViewModel ↔ View.

# 24. Property Change Notification

Binding cần biết khi data thay đổi.

Sai:

```csharp
public string Name { get; set; }
```

Property đổi không có notification thì UI có thể không update.

Thường dùng:

```csharp
ObservableObject
```

với CommunityToolkit.Mvvm:

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;
}
```

Toolkit generate property + notification.

`Property changes → PropertyChanged → Binding → UI updates`

# 25. Commands

Dùng MVVM thì không gắn business logic trực tiếp vào Button click.

Thay vì:

```xml
<Button Clicked="OnSaveClicked" />
```

có thể dùng:

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
    // Save logic
}
```

`Button → Command → ViewModel → Service`

Đây là pattern cốt lõi của MAUI.

# 26. `CommandParameter`

Có thể truyền data:

```xml
<Button
    Text="Delete"
    Command="{Binding DeleteCommand}"
    CommandParameter="{Binding .}" />
```

Trong CollectionView: `Current item → CommandParameter → DeleteCommand(item)`

ViewModel:

```csharp
[RelayCommand]
private async Task DeleteAsync(Product product)
{
}
```

# 27. Binding trong CollectionView

Đây là scenario phải thành thạo.

Model:

```csharp
public class Product
{
    public string Name { get; set; } = "";
}
```

ViewModel:

```csharp
public ObservableCollection<Product> Products { get; }
```

XAML:

```xml
<CollectionView
    ItemsSource="{Binding Products}">

    <CollectionView.ItemTemplate>

        <DataTemplate
            x:DataType="models:Product">

            <Label
                Text="{Binding Name}" />

        </DataTemplate>

    </CollectionView.ItemTemplate>

</CollectionView>
```

`MainViewModel → {Products → {Product; Product; Product}}`

`CollectionView → {ItemTemplate → {BindingContext = Product}}`

**Điểm dễ sai:**

Trong `DataTemplate`, BindingContext thường chuyển từ: `MainViewModel`

sang: `Product`

# 28. RelativeSource / x:Reference

BindingContext không phải object cần lấy data: dùng kỹ thuật khác.

```xml
Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}},
                  Path=BindingContext.DeleteCommand}"
```

Đây là pattern nâng cao hơn.

80/20:

> Chưa cần thuộc syntax; cần hiểu **"BindingContext hiện tại không phải source cần dùng."**

Sau đó học: `RelativeSource`; `x:Reference`; `Source`

# 29. Events vs Commands

### Event

```xml
<Button
    Clicked="OnClicked" />
```

C#:

```csharp
private void OnClicked(
    object sender,
    EventArgs e)
{
}
```

### Command

```xml
<Button
    Command="{Binding SaveCommand}" />
```

MVVM: `View → Command → ViewModel`

80/20 recommendation:

- UI-only event → Event có thể phù hợp.
- Application action / business behavior → Command.

# 30. Compiled Binding

Ưu tiên compiled binding khi học MAUI XAML hiện đại.

```xml
<ContentPage
    x:DataType="viewmodels:MainViewModel">
```

Sau đó:

```xml
<Label
    Text="{Binding UserName}" />
```

Benefits: `Compile-time checking`; `Better performance`; `Better tooling`; `Fewer runtime binding errors`

Đặc biệt hữu ích trong: `CollectionView`; `DataTemplate`; `Complex UI`; `Large applications`

# 31. XAML Hot Reload

Hot Reload hiển thị thay đổi XAML nhanh hơn, không cần restart app mỗi lần.

Workflow: `Run app → Modify XAML → Save → Hot Reload → UI updates`

Rất hữu ích để chỉnh: `Spacing`; `Margin`; `Padding`; `Colors`; `Fonts`; `Layout`; `Visibility`; `Styles`

# 32. Hot Reload Mental Model

Hot Reload không có nghĩa: `Mọi thay đổi đều được apply.`

Nó phụ thuộc vào: `IDE`; `Debugger`; `Target platform`; `Type of change`; `Project configuration`

Nếu Hot Reload không hoạt động: `1. App đang Debug?`; `2. Correct project/target?`; `3. Hot Reload enabled?`; `4. XAML có compile?`; `5. Có syntax error?`; `6. Restart app có fix không?`

# 33. XAML Debugging 80/20

Khi UI sai, debug theo thứ tự: `1. XAML compile? → 2. Element tồn tại? → 3. Layout đúng? → 4. Property đúng? → 5. Resource tồn tại? → 6. BindingContext đúng? → 7. Binding path đúng? → 8. Property notification? → 9. Platform-specific?`

# 34. Nếu UI không hiển thị

### Case 1 — control không xuất hiện

Check: `IsVisible`; `WidthRequest`; `HeightRequest`; `Opacity`; `Parent layout`; `Grid row/column`

### Case 2 — text không đúng

Check: `Text`; `Binding`; `BindingContext`; `Property name`; `x:DataType`

### Case 3 — style không áp dụng

Check: `ResourceDictionary`; `x:Key`; `StaticResource`; `DynamicResource`; `Resource scope`

# 35. Binding Debugging

Nếu:

```xml
<Label Text="{Binding UserName}" />
```

không hiển thị:

### Step 1

BindingContext:

```csharp
BindingContext = viewModel;
```

### Step 2

Property:

```csharp
public string UserName { get; set; }
```

### Step 3

Notification: `ObservableObject?`; `INotifyPropertyChanged?`

### Step 4

Binding path:

```xml
Text="{Binding UserName}"
```

### Step 5

`x:DataType`:

```xml
x:DataType="viewmodels:MainViewModel"
```

# 36. XAML Namespace Debugging

Nếu compiler báo: `The type 'Something' was not found`

kiểm tra:

```xml
xmlns:models="clr-namespace:MyApp.Models"
```

Sau đó:

```xml
x:DataType="models:Product"
```

`xmlns alias → CLR namespace → Type`

```xml
xmlns:vm="clr-namespace:MyApp.ViewModels"
```

```xml
x:DataType="vm:MainViewModel"
```

# 37. Custom Namespace

Có thể map C# namespace:

```xml
xmlns:models="clr-namespace:MyApp.Models"
```

Sau đó:

```xml
<models:ProductView />
```

Nếu type nằm ở assembly khác:

```xml
xmlns:controls="clr-namespace:MyControls;assembly=MyControls"
```

80/20:

> Cần đọc được namespace mapping; chưa cần viết custom XAML markup extension.

# 38. Layout 80/20

Nếu chỉ học kỹ một layout:

> **Grid**

```xml
<Grid
    RowDefinitions="Auto,*"
    ColumnDefinitions="Auto,*">

    <Label
        Grid.Row="0"
        Grid.Column="0"
        Text="Name" />

    <Entry
        Grid.Row="0"
        Grid.Column="1" />

</Grid>
```

Các ký hiệu cần hiểu: `Auto`; `*`; `2*`; `100`

### `Auto`

Kích thước dựa trên content.

### `*`

Chiếm phần không gian còn lại.

### `2*`

Nhận 2 phần so với `*`.

`*,2*`

→ tỷ lệ: `1 : 2`

# 39. Grid vs StackLayout

### StackLayout

Dùng khi UI tuyến tính: Dọc: `A → B → C`.

hoặc: Ngang: `A B C`.

### Grid

Dùng khi cần: `Rows`; `Columns`; `Alignment`; `Forms`; `Dashboard`; `Complex layout`

80/20:

- Simple linear UI → Vertical/HorizontalStackLayout.
- Complex UI → Grid.

# 40. Margin vs Padding

Rất dễ nhầm.

- Margin → khoảng cách bên ngoài element.
- Padding → khoảng cách bên trong element.

```text
       Margin
    ┌─────────────┐
    │   ┌─────┐   │
    │   │Text │   │
    │   └─────┘   │
    └─────────────┘
       Padding
```

```text
Margin  = outside
Padding = inside
```

# 41. `HorizontalOptions` / `VerticalOptions`

Kiểm soát alignment/sizing behavior.

Các giá trị thường gặp: `Start`; `Center`; `End`; `Fill`

Có thể gặp: `StartAndExpand`; `CenterAndExpand`; `...`

Với modern MAUI layout, đừng lạm dụng `*AndExpand`.

Ưu tiên: `Grid`; `Auto`; `*`; `explicit sizing khi thực sự cần`

# 42. Styles

Thay vì:

```xml
<Button
    BackgroundColor="Blue"
    TextColor="White"
    FontSize="16"
    CornerRadius="10" />
```

lặp lại nhiều lần, tạo Style:

```xml
<Style
    TargetType="Button"
    x:Key="PrimaryButton">

    <Setter
        Property="BackgroundColor"
        Value="Blue" />

    <Setter
        Property="TextColor"
        Value="White" />

</Style>
```

Sử dụng:

```xml
<Button
    Style="{StaticResource PrimaryButton}"
    Text="Save" />
```

`Style → Setters → Reusable visual configuration`

# 43. Implicit Style

Không cần key:

```xml
<Style TargetType="Button">
    ...
</Style>
```

Áp dụng cho Button trong scope.

Cách xây design system đơn giản: `Resources → {Colors; Typography; Button styles; Entry styles; Card styles}`

# 44. Resource Scope

Resources có scope.

`Application.Resources → {Page.Resources → {Control.Resources}}`

`Narrow scope ← Control ← Page ← Application → Broad scope`

Khi resource không tìm thấy:

> Kiểm tra resource dictionary và scope trước.

# 45. Binding + Resources

Có thể kết hợp:

```xml
<Button
    Text="{Binding SaveText}"
    BackgroundColor="{StaticResource PrimaryColor}"
    Command="{Binding SaveCommand}" />
```

Một control có thể đồng thời có: `Normal property` + `Resource` + `Binding` + `Attached property`

```xml
<Button
    Grid.Row="1"
    Style="{StaticResource PrimaryButton}"
    Text="{Binding SaveText}"
    Command="{Binding SaveCommand}" />
```

Cần đọc thành thạo kiểu XAML thực tế này.

# 46. XAML + MVVM Architecture

Một màn hình MAUI chuẩn: `MainPage.xaml — Binding → MainViewModel — calls → Services → API / Database / Device`

XAML nên tập trung vào: `Layout`; `Presentation`; `Binding`; `Commands`; `Resources`

ViewModel: `State`; `Commands`; `Presentation logic`

Service: `Business/data/platform operations`

# 47. Anti-patterns cần tránh

## 1. Code-behind quá nhiều

Không nên:

```csharp
private async void SaveClicked(...)
{
    // 200 lines business logic
}
```

Nếu logic có thể nằm ở ViewModel/Service: `Move it.`

## 2. `x:Name` cho mọi thứ

Không nên:

```xml
<Entry x:Name="Entry1" />
<Entry x:Name="Entry2" />
<Label x:Name="Label1" />
<Button x:Name="Button1" />
```

Nếu chỉ để truyền data: `Binding`

thường phù hợp hơn.

## 3. Event cho mọi action

Thay vì: `Clicked`; `TextChanged`; `Toggled`; `Selected`

đều xử lý trong code-behind, cân nhắc: `Binding`; `Command`; `EventToCommandBehavior`

## 4. Hard-code style khắp nơi

Không nên:

```xml
<Button BackgroundColor="..." />
<Button BackgroundColor="..." />
<Button BackgroundColor="..." />
```

Tạo resource/style.

# 48. 80/20 Learning Order

## Level 1 — XAML Syntax

Học: `Element`; `Attribute`; `Property`; `Content`; `Property Element`; `Collection`; `Attached Property`

Checkpoint:

> Nhìn XAML và giải thích được object tree + properties.

## Level 2 — Layout

Học: `Grid`; `VerticalStackLayout`; `HorizontalStackLayout`; `Margin`; `Padding`; `Auto`; `*`; `Row`; `Column`

Checkpoint:

> Tự dựng được một màn hình form/dashboard.

## Level 3 — Resources

Học: `ResourceDictionary`; `x:Key`; `StaticResource`; `DynamicResource`; `Style`; `Setter`; `Implicit Style`

Checkpoint:

> Tạo được style/resource dùng lại giữa nhiều page.

## Level 4 — Binding

Học: `BindingContext`; `Binding`; `OneWay`; `TwoWay`; `PropertyChanged`; `ObservableObject`

Checkpoint:

> UI tự cập nhật khi ViewModel thay đổi.

## Level 5 — MVVM

Học: `View`; `ViewModel`; `Command`; `RelayCommand`; `ObservableCollection`; `x:DataType`; `Compiled Binding`

Checkpoint:

> Tạo được CRUD screen không cần business logic trong code-behind.

## Level 6 — Namespace

Học: `xmlns`; `xmlns:x`; `clr-namespace`; `x:Class`; `x:Name`; `x:Key`; `x:Type`; `x:DataType`

Checkpoint:

> Tự xử lý được lỗi type/namespace trong XAML.

## Level 7 — Hot Reload + Debugging

Học: `Hot Reload`; `XAML compile errors`; `Binding errors`; `Layout debugging`; `Resource lookup`

Checkpoint:

> Chỉnh UI nhanh, tự tìm nguyên nhân XAML không hoạt động như mong muốn.

# 49. 20% XAML Syntax cần thuộc

Cheat sheet ngắn:

```xml
<!-- Namespace -->
xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"

<!-- Class -->
x:Class="MyApp.MainPage"

<!-- Name -->
x:Name="MyButton"

<!-- Resource key -->
x:Key="PrimaryColor"

<!-- Binding -->
Text="{Binding Name}"

<!-- Resource -->
BackgroundColor="{StaticResource PrimaryColor}"

<!-- Attached property -->
Grid.Row="1"

<!-- Compiled binding -->
x:DataType="vm:MainViewModel"

<!-- Command -->
Command="{Binding SaveCommand}"

<!-- Command parameter -->
CommandParameter="{Binding .}"
```

# 50. 20% XAML Controls cần thuộc

`ContentPage`; `Grid`; `VerticalStackLayout`; `HorizontalStackLayout`; `ScrollView`; `CollectionView`

`Label`; `Button`; `Entry`; `Editor`; `Image`

`Picker`; `DatePicker`; `TimePicker`; `CheckBox`; `Switch`

`Border`; `Frame (legacy scenarios)`; `ActivityIndicator`; `ProgressBar`; `WebView`

Không cần học mọi control trước.

# 51. 20% Binding Patterns cần thuộc

### Simple property

```xml
Text="{Binding Name}"
```

### Two-way

```xml
Text="{Binding Name, Mode=TwoWay}"
```

### Command

```xml
Command="{Binding SaveCommand}"
```

### Command parameter

```xml
CommandParameter="{Binding .}"
```

### Resource

```xml
BackgroundColor="{StaticResource PrimaryColor}"
```

### Compiled binding

```xml
x:DataType="vm:MainViewModel"
```

### Collection

```xml
ItemsSource="{Binding Items}"
```

### Template

```xml
<DataTemplate x:DataType="models:Item">
```

# 52. Mini Project — XAML Checkpoint

## Project: MAUI Expense Screen

Tạo một màn hình:

```text
┌──────────────────────────────┐
│ Expenses                     │
│ [ Search...              ]   │
│ ┌──────────────────────────┐ │
│ │ Food              $12.50 │ │
│ │ Sep 30                   │ │
│ └──────────────────────────┘ │
│ ┌──────────────────────────┐ │
│ │ Transport          $8.00 │ │
│ │ Sep 29                   │ │
│ └──────────────────────────┘ │
│              [ + Add ]       │
└──────────────────────────────┘
```

Phải sử dụng: `Grid`; `CollectionView`; `DataTemplate`; `x:DataType`; `Binding`; `Command`; `Resource`; `Style`

# 53. Broken Lab — Binding

Cố tình tạo:

```xml
<Label
    Text="{Binding UserNam}" />
```

Trong ViewModel:

```csharp
public string UserName { get; set; }
```

Nhiệm vụ: `1. Observe symptom`; `2. Find binding error`; `3. Identify typo`; `4. Fix`; `5. Verify`

Root cause: `Binding path mismatch`

# 54. Broken Lab — BindingContext

XAML:

```xml
<Label
    Text="{Binding UserName}" />
```

Nhưng không set:

```csharp
BindingContext
```

Nhiệm vụ: `1. Observe blank UI`; `2. Check BindingContext`; `3. Set ViewModel`; `4. Verify`

Root cause: `Binding has no correct source`

# 55. Broken Lab — Resource

XAML:

```xml
<Button
    BackgroundColor="{StaticResource PrimaryColour}" />
```

Resource:

```xml
x:Key="PrimaryColor"
```

Nhiệm vụ: `Find resource lookup failure`

Root cause: `Key mismatch`

# 56. Broken Lab — Layout

Cố tình:

```xml
<Grid>
    <Label
        Grid.Row="5"
        Text="Hello" />
</Grid>
```

Nhưng Grid chỉ có:

```xml
<RowDefinitions>
    <RowDefinition Height="Auto" />
</RowDefinitions>
```

Nhiệm vụ: `Identify invalid layout assumptions`; `Fix row definition`; `Verify`

# 57. Broken Lab — x:DataType

Model:

```csharp
public class Expense
{
    public string Title { get; set; }
}
```

XAML:

```xml
<DataTemplate
    x:DataType="models:Expenses">
```

Nhiệm vụ: `Identify type mismatch`; `Fix namespace/type`; `Build`; `Verify`

Root cause: `Compiled binding type mismatch`

# 58. Support Engineer XAML Troubleshooting Tree

`XAML issue`:
- `Build fails? → {Syntax / Namespace / Type}`
- `UI missing? → {Layout / Visibility / Size}`
- `UI visible but wrong value? → {Binding / Resource}`
- `Binding doesn't update? → {PropertyChanged / ObservableObject}`
- `Command doesn't execute? → {BindingContext / Command / CanExecute}`
- `Style doesn't apply? → {Resource scope / Key / TargetType}`
- `Only fails on one platform? → {Platform-specific behavior / Handler}`

# 59. Những thứ chưa cần học ngay

Theo 80/20, chưa cần đào sâu: `❌ Custom MarkupExtension`; `❌ Custom XAML parser`; `❌ Complex ControlTemplate`; `❌ Complex DataTemplateSelector`; `❌ Advanced RelativeSource`; `❌ Advanced triggers`; `❌ VisualStateManager internals`; `❌ Custom Handler implementation`; `❌ XAML compiler internals`; `❌ XML schema internals`

Khi project yêu cầu thì học.

# 60. Definition of Done

**Nắm XAML nền tảng** khi làm được toàn bộ:

- [ ] Đọc được XAML object tree
- [ ] Hiểu element/property
- [ ] Hiểu property element
- [ ] Hiểu attached property
- [ ] Dùng Grid thành thạo
- [ ] Dùng StackLayout đúng tình huống
- [ ] Hiểu Margin vs Padding
- [ ] Tạo ResourceDictionary
- [ ] Dùng StaticResource
- [ ] Dùng Style
- [ ] Hiểu xmlns
- [ ] Dùng x:Class
- [ ] Dùng x:Key
- [ ] Dùng x:DataType
- [ ] Hiểu BindingContext
- [ ] Dùng OneWay binding
- [ ] Dùng TwoWay binding
- [ ] Dùng Command
- [ ] Dùng CommandParameter
- [ ] Dùng ObservableObject
- [ ] Dùng ObservableCollection
- [ ] Dùng CollectionView + DataTemplate
- [ ] Hiểu compiled binding
- [ ] Dùng XAML Hot Reload
- [ ] Debug binding failure
- [ ] Debug resource failure
- [ ] Debug layout failure
- [ ] Debug namespace failure

# 61. 80/20 Final Mental Model

Sơ đồ tổng hợp XAML: `XAML`:
- Syntax: Element, Property, Content, Attached.
- Layout: Grid, Stack, Margin, Padding.
- Namespace: xmlns, x:Class, x:Key, x:Type.

Các nhánh → Resources → Style / Color → Binding:
- BindingContext → ViewModel.
- Command → ViewModel.

Hai nhánh → MVVM → Services → API / DB / Device.

# 62. Thứ tự học thực tế

Nếu mỗi ngày chỉ có 1–2 giờ:

## Day 1

`XAML structure`; `Element`; `Attribute`; `Property`; `x:Class`; `xmlns`

## Day 2

`Grid`; `StackLayout`; `Margin`; `Padding`; `Sizing`

## Day 3

`Resources`; `StaticResource`; `DynamicResource`; `Styles`

## Day 4

`BindingContext`; `Binding`; `OneWay`; `TwoWay`

## Day 5

`ObservableObject`; `ObservableCollection`; `RelayCommand`

## Day 6

`CollectionView`; `DataTemplate`; `x:DataType`; `Compiled Binding`

## Day 7

`Hot Reload`; `Binding debugging`; `Resource debugging`; `Layout debugging`

Sau 7 ngày: `Mini Expense UI → MVVM → CollectionView → Commands → SQLite/API`

# 63. Official Microsoft Learn

Nguồn nền tảng cho roadmap:

- [XAML — .NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/xaml/?view=net-maui-10.0)
- [Get started with .NET MAUI XAML](https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/get-started?view=net-maui-10.0)
- [Essential .NET MAUI XAML syntax](https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/essential-syntax?view=net-maui-10.0)
- [Data binding basics](https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/data-binding-basics?view=net-maui-10.0)
- [Data binding and MVVM](https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/mvvm?view=net-maui-10.0)
- [XAML namespaces](https://learn.microsoft.com/en-us/dotnet/maui/xaml/namespaces/?view=net-maui-10.0)
- [XAML Hot Reload for .NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/xaml/hot-reload?view=net-maui-10.0)

> **Lưu ý:** Các URL được chuẩn hóa sang `view=net-maui-10.0` để roadmap này bám vào .NET MAUI 10 thay vì MAUI 9.
