# .NET MAUI 10 Handlers — học theo phương pháp 80/20

> **Mục tiêu:** hiểu cầu nối giữa control MAUI và native view; chỉ can thiệp khi API có sẵn chưa đủ, và biết giới hạn phạm vi cùng cách quản lý lifecycle.
>
> **Cập nhật:** 16/09/2026. **Phạm vi:** **.NET MAUI 10 trên .NET 10**. Đã đối chiếu tài liệu handler [H1], tùy biến handler [H2] theo phiên bản 10 và thay đổi native implementation/handler mặc định [H3]. Không lấy handler riêng MAUI 11 làm mặc định.

## 1. Đúng tinh thần 80/20: hiểu trước, chưa cần tự viết handler ngay

**Ưu tiên kiến thức có ROI cao, không bỏ phần còn lại.** Phần lớn lúc dựng UI, hãy dùng property, binding, resource và những cơ chế đã học trong `XAML.md`/`Fundamentals.md`. Handler là công cụ khi cần vượt ra ngoài khả năng mà API control đang cung cấp. [H1], [H2]

Thứ tự lựa chọn đề xuất:

1. API/property của control đã đáp ứng chưa?
2. Nếu chỉ cần tái sử dụng trình bày/hành vi, resource, template hoặc behavior có đủ không?
3. Nếu cần thay đổi native view của control đã có, xem handler customization.
4. Chỉ xét custom handler khi phải ánh xạ một control/API mới sang native implementation.

Thứ tự học và bài tập dưới đây là đề xuất của người tổng hợp, không phải quy trình bắt buộc của Microsoft.

## 2. Mô hình tư duy cốt lõi

```text
Control MAUI, ví dụ Entry
        |
        v
Interface chung, ví dụ IEntry
        |
        v
EntryHandler + mapper
        |
        v
Native view của nền tảng
```

| Thuật ngữ | Cách hiểu để đọc code |
| --- | --- |
| Virtual view | Control đa nền tảng triển khai interface chung. |
| Handler | Tạo native view và ánh xạ API control sang API native. |
| `VirtualView` | Phía control đa nền tảng mà handler đang phục vụ. |
| `PlatformView` | Native view bên dưới; dùng để đọc/đặt property, gọi method hoặc gắn native event. |
| Mapper | Các ánh xạ từ thay đổi/yêu cầu phía MAUI sang thao tác phía native. |

Ví dụ trong nguồn: `Button` được ánh xạ sang `UIButton` trên iOS và `MaterialButton` trên Android. Với **MAUI 10**, ví dụ tùy biến `Entry` dùng `UITextField` trên iOS/Mac Catalyst, **`MauiAppCompatEditText` trên Android** và `TextBox` trên Windows. **Không cast `PlatformView` thành cùng một kiểu trên mọi hệ điều hành.** [H1], [H2]

### 2.1. Hai thay đổi cần nhớ khi học handler trên MAUI 10

- **`Entry`/`Editor` trên Android:** native view chuyển từ `AppCompatEditText` sang `Microsoft.Maui.Platform.MauiAppCompatEditText`, bổ sung hỗ trợ `SelectionChanged`. Khi đọc code cast native view hoặc đăng ký event, kiểm tra kiểu và namespace theo phiên bản 10; không chỉ đổi tên version trong URL. Mẫu `SetSelectAllOnFocus` ở mục 5 vẫn dùng được với native view này. [H2], [H3]
- **`CollectionView`/`CarouselView` trên iOS và Mac Catalyst:** handler tối ưu đã là mặc định. Không chép bước opt-in của tài liệu cũ như một yêu cầu bắt buộc. Nếu app có `ConfigureMauiHandlers`, kiểm tra đăng ký tùy biến có đang ghi đè handler mặc định không; thử lại cuộn, selection và cập nhật dữ liệu. Đây không phải thay đổi handler đồng loạt cho mọi nền tảng. [H3]

Khi mở [H2], một số ví dụ lifecycle có thể còn mang nhãn `moniker` không nhất quán với kiểu native bên trong. Đối chiếu bảng native view dành cho MAUI 10 và [H3], không sao chép nguyên một đoạn cast chỉ vì nó xuất hiện trên trang.

## 3. Mapper: phân biệt property với command

| Loại | Kích hoạt bởi | Điều cần nhớ |
| --- | --- | --- |
| Property mapper | Property của virtual view thay đổi. | Cập nhật native view tương ứng. |
| Command mapper | Một yêu cầu/thao tác được gửi tới native view, có thể kèm dữ liệu. | Ví dụ yêu cầu cuộn tới vị trí cụ thể. |

**Command trong command mapper không phải `ICommand` của MVVM.** `Button.Command` ở ViewModel và command mapper của handler giải quyết hai tầng khác nhau. [H1]

### Cách tùy biến mapping cần nhận diện

| API | Vai trò |
| --- | --- |
| `PrependToMapping` | Chèn tùy biến trước mapping MAUI. |
| `AppendToMapping` | Chèn tùy biến sau mapping MAUI. |
| `ModifyMapping` | Sửa mapping đã có; phải hiểu tác động lên logic mặc định. |

Key có ý nghĩa: nếu muốn phản ứng theo một property, dùng đúng key mà mapper của MAUI sử dụng, chẳng hạn `nameof(IEntry.IsPassword)`. Một key tùy ý không tự trở thành bộ theo dõi thay đổi của property. [H2]

**Không cần học thuộc bảng mọi handler.** Tra đúng control trong [H1] khi làm việc; một số control/page còn dùng implementation tương thích kiểu renderer, nên đừng suy ra tên hoặc API chỉ từ quy tắc đặt tên.

## 4. Hai bẫy có ROI cao nhất: global scope và lifecycle

### 4.1. Mapper customization có phạm vi toàn ứng dụng

Sửa mapper không chỉ ảnh hưởng control nơi đoạn code được gọi. Tùy biến có thể tác động tới mọi control cùng loại trong app. Muốn giới hạn, một cách trong nguồn là tạo subclass để đánh dấu nhóm control, rồi kiểm tra kiểu trong mapping callback. [H1], [H2]

**Khuyến nghị thực hành:** tập trung việc đăng ký mapping ở một nơi khởi tạo, tránh append lại mỗi lần mở page. Với thay đổi chỉ dành cho một nhóm control, luôn có control đối chứng không thuộc nhóm đó.

### 4.2. Handler lifecycle không phải page lifecycle hay app lifecycle

| Sự kiện | Ý nghĩa | Thao tác cần cân nhắc |
| --- | --- | --- |
| `HandlerChanging` | Handler sắp được thêm/thay/gỡ; kiểm tra `OldHandler` và `NewHandler`. | Nếu có `OldHandler`, tháo native event và dọn phần gắn với native view cũ. |
| `HandlerChanged` | Handler/native view đã được tạo và các property ban đầu đã được áp dụng. | Lúc phù hợp để truy cập native view và đăng ký native event cần thiết. |

Ngoài event còn có `OnHandlerChanging` và `OnHandlerChanged` để override. Khi thay handler, đừng tiếp tục giữ và sử dụng native view cũ như thể nó vẫn còn gắn với control. [H1]

Nếu đã đăng ký native event, phải thiết kế đường unsubscribe tương ứng; không chỉ viết phần attach. Trong tài liệu tùy biến, việc này được thực hiện qua `HandlerChanged` và `HandlerChanging`. [H2]

## 5. Ví dụ ngắn: chỉ một nhóm Entry chọn toàn bộ chữ khi nhận focus

**Ví dụ minh họa do người tổng hợp điều chỉnh từ [H2].** Hành vi native bên dưới chỉ được thêm trên **Android**. Trên nền tảng khác, đoạn đăng ký không được biên dịch và control giữ hành vi mặc định.

### 5.1. Tạo subclass để xác định phạm vi

Đặt class trong project MAUI, ví dụ `Controls/SelectAllEntry.cs`:

```csharp
using Microsoft.Maui.Controls;

namespace HandlerDemo.Controls;

public class SelectAllEntry : Entry
{
}
```

### 5.2. Đăng ký mapping một lần lúc khởi tạo

Đặt đoạn sau trong `MauiProgram.CreateMauiApp()`, trước `builder.Build()`, không đặt lặp lại ở constructor của từng page. Đổi namespace đồng bộ nếu project không tên `HandlerDemo`.

```csharp
#if ANDROID
Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(
    "HandlerDemo.SelectAllOnFocus",
    (handler, view) =>
    {
        if (view is HandlerDemo.Controls.SelectAllEntry)
        {
            handler.PlatformView.SetSelectAllOnFocus(true);
        }
    });
#endif
```

Key trong mẫu là key tùy biến, không đại diện cho property `Text`. Mục tiêu là cấu hình hành vi native khi áp dụng tùy biến, không phải chạy callback mỗi lần người dùng gõ.

### 5.3. So sánh control thường với control được đánh dấu

Đặt layout sau vào nội dung một page đã có:

```xaml
<VerticalStackLayout
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:controls="clr-namespace:HandlerDemo.Controls"
    Spacing="12">
    <Entry Text="Entry bình thường" />
    <controls:SelectAllEntry Text="Chọn toàn bộ khi nhận focus" />
</VerticalStackLayout>
```

**Cần kiểm chứng:**

- Trên Android, khi chuyển focus vào `SelectAllEntry`, toàn bộ text được chọn.
- `Entry` thường không bị áp dụng tùy biến này.
- Mở/đóng page nhiều lần không làm đăng ký mapping lặp lại.
- Nền tảng khác vẫn build và giữ hành vi mặc định.

Mẫu này không gắn native event nên không có event để unsubscribe. Nếu chuyển sang cách dùng native event trên iOS/Windows, phải bổ sung quản lý lifecycle, không chỉ sao chép phần đăng ký.

Các đoạn trên không phải project độc lập. Chưa build/chạy ứng dụng MAUI trong phiên tổng hợp này.

## 6. Tổ chức code theo nền tảng

Hai cách được [H2] trình bày:

- **Conditional compilation:** dùng `#if ANDROID`, `#if IOS`, `#if MACCATALYST`, `#if WINDOWS` khi phần native còn nhỏ.
- **Partial classes/methods:** giữ phần khai báo chung ngoài `Platforms`, đặt implementation riêng trong thư mục nền tảng khi logic lớn hơn.

Không để reference native Android bị biên dịch vào target Windows/iOS. Với partial methods, chú ý quy tắc chữ ký và việc implementation có bắt buộc hay không; không suy ra rằng mọi partial method đều có thể bỏ trống trên mọi target.

## 7. Bảng gỡ lỗi thực tế

Đây là checklist đề xuất dựa trên các cơ chế trong nguồn, không phải danh sách đầy đủ mọi nguyên nhân.

| Hiện tượng | Kiểm tra trước |
| --- | --- |
| Sửa một ô nhập nhưng cả app thay đổi | Mapper có tính global; đã giới hạn bằng kiểu/control mục tiêu chưa? |
| Native view chưa có hoặc đã thay | Thời điểm truy cập và các sự kiện handler, không chỉ thời điểm tạo page. |
| Callback không chạy khi property đổi | Key có đúng mapping của property không, hay chỉ là key tùy ý? |
| Hành vi/event bị chạy nhiều lần | Mapping hoặc event có đang được đăng ký lặp lại? |
| Page đã bỏ nhưng native event còn hoạt động | Có tháo event khỏi `OldHandler.PlatformView` và bỏ tham chiếu cũ không? |
| Android chạy nhưng target khác lỗi compile | Reference/type native đã nằm đúng điều kiện build hoặc thư mục nền tảng chưa? |
| Mẫu input Android cũ không khớp native type/event | Đối chiếu `MauiAppCompatEditText` và `Microsoft.Maui.Platform` cho MAUI 10. |
| Danh sách đổi hành vi sau nâng cấp trên iOS/Mac Catalyst | Handler tối ưu mặc định và mọi đăng ký ghi đè trong `ConfigureMauiHandlers`. |
| Tùy biến phá hành vi có sẵn | Đang append/prepend hay sửa mapping gốc? Phần mặc định nào đã bị thay? |

## 8. Thực hành và phần học sâu sau

### Vòng học đầu

1. Chọn một control đang dùng trong sản phẩm và tra interface/handler của nó.
2. Vẽ đường đi: property MAUI → mapper → native view.
3. Làm mẫu Android ở mục 5 với một control đối chứng.
4. Thử thay handler/mở lại màn hình và quan sát lifecycle bằng log.
5. Nếu cần thêm native event, chứng minh cả attach lẫn detach hoạt động đúng.

### Khi nào quay lại học sâu?

| Chủ đề | Dấu hiệu cần học |
| --- | --- |
| Custom handler hoàn chỉnh | Control mới cần API chung và native implementation riêng. |
| Command mapper tùy biến | Cần gửi thao tác kèm dữ liệu, không chỉ đồng bộ property. |
| Native API chi tiết | API chung và tùy biến đơn giản chưa đạt hành vi sản phẩm. |
| Tổ chức partial/multi-targeting lớn | Nhiều platform implementation làm code điều kiện khó quản lý. |
| Tối ưu/đo hiệu năng handler | Đã đo và xác định vấn đề nằm ở tầng native/mapping. |

**80/20 không phải bỏ các mục này:** học chúng khi nhu cầu thật sự xuất hiện, thay vì tự xây handler cho mọi control ngay từ đầu.

### Checklist hoàn thành vòng đầu

- [ ] Phân biệt được virtual view, native view, `VirtualView` và `PlatformView`.
- [ ] Không nhầm command mapper với `ICommand` trong ViewModel.
- [ ] Hiểu mapper global và chứng minh control ngoài phạm vi không bị đổi.
- [ ] Chọn key phù hợp với mục đích tùy biến.
- [ ] Có kế hoạch attach/detach cho native event.
- [ ] Kiểm tra target khác, không chỉ target đã viết native code.

## 9. Nguồn và giới hạn

| Mã | Nguồn | Cách sử dụng |
| --- | --- | --- |
| [H1] | .NET MAUI handlers | Link ban đầu: kiến trúc, mappers, lifecycle và bảng handler. |
| [H2] | Customize .NET MAUI controls with handlers | Nguồn bổ sung được H1 dẫn tới: phạm vi tùy biến và ví dụ thực hành. |
| [H3] | What's new in .NET MAUI for .NET 10 | Native input Android và handler danh sách mặc định trên iOS/Mac Catalyst. |

Đã chọn phần áp dụng MAUI 10, bao gồm native input Android và handler danh sách mặc định đúng phiên bản. Không lấy Android Shell handler riêng MAUI 11 làm hướng dẫn cho MAUI 10. Chưa đọc toàn bộ chuỗi hướng dẫn tạo custom handler; bảng học sâu chỉ chỉ ra lúc nên mở tiếp. Ví dụ cần build và kiểm thử native trên các target thực tế khi tích hợp.

[H1]: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/?view=net-maui-10.0
[H2]: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/customize?view=net-maui-10.0
[H3]: https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0
