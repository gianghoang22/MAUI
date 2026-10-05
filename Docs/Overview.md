# .NET MAUI 10 — Overview theo phương pháp học 80/20

> **Mục tiêu:** ưu tiên kiến thức có giá trị sử dụng cao để sớm đọc hiểu, xây dựng và sửa một ứng dụng MAUI; sau đó mở rộng theo nhu cầu sản phẩm.
>
> **Cập nhật:** 16/09/2026. **Phạm vi:** **.NET MAUI 10 trên .NET 10**. Đã đối chiếu lại ba nguồn Microsoft Learn ban đầu theo `view=net-maui-10.0`, bổ sung thay đổi của phiên bản 10 [S4]. Không coi MAUI 11 hoặc tính năng preview là mặc định của bộ tài liệu này.

## 1. Hiểu đúng cách học 80/20

**20% kiến thức quan trọng nhất → tạo ra khoảng 80% khả năng sử dụng thực tế.** Đây là nguyên tắc ưu tiên, không phải tỷ lệ được đo lường hay cam kết cho mọi dự án.

- **Học trước:** thứ thường xuyên dùng, giúp làm ra một luồng chức năng hoàn chỉnh hoặc gỡ được trở ngại lớn.
- **Học tới mức dùng được:** có thể tự làm và giải thích quyết định của mình, không chỉ xem hết video.
- **Học sâu đúng lúc:** quay lại phần chuyên sâu khi sản phẩm có nhu cầu tương ứng, không bỏ qua vĩnh viễn.

Thứ tự ưu tiên và bài thực hành dưới đây là **đề xuất của người tổng hợp**, dựa trên các nguồn ở cuối tài liệu; không phải lộ trình nguyên văn do Microsoft quy định.

## 2. Sáu điều cần nhớ trước tiên

1. **MAUI là framework xây dựng ứng dụng mobile và desktop native bằng C# và XAML**, không phải ngôn ngữ lập trình mới. Tên đầy đủ là *.NET Multi-platform App UI*. [S1]
2. **Một codebase có thể phục vụ Android, iOS, macOS và Windows.** Mục tiêu là chia sẻ nhiều nhất có thể phần giao diện, nghiệp vụ, tài nguyên và kiểm thử. [S1]
3. **Chia sẻ code không có nghĩa mọi nền tảng hoàn toàn giống nhau.** Vẫn có chỗ cho code, tài nguyên và lời gọi API riêng của từng nền tảng. [S1]
4. **Single project không có nghĩa một gói cài đặt chạy ở mọi nơi.** Project dùng multi-targeting; ứng dụng được biên dịch thành các gói native tương ứng. [S1]
5. **Nhóm kỹ năng cần ưu tiên:** dựng UI, hiển thị dữ liệu bằng binding, điều hướng, xử lý dữ liệu và sử dụng API thiết bị theo nhu cầu. [S1], [S3]
6. **Chốt môi trường build sớm, đặc biệt khi có iOS/macOS.** Build cho iOS và macOS cần máy Mac; phát triển iOS từ Windows cần Mac kết nối qua mạng. [S1], [S2]

MAUI là mã nguồn mở và phát triển từ Xamarin.Forms. Nếu đã biết Xamarin.Forms, có thể tận dụng kiến thức cũ, nhưng không nên mặc định mọi cách làm đều giữ nguyên. [S1]

## 3. Mô hình tư duy về kiến trúc

```text
Ứng dụng: giao diện + logic C#
               |
               v
       Controls và API của MAUI
               |
               v
 API native: Android / iOS / Mac Catalyst / WinUI 3

Khi cần: code ứng dụng có thể gọi trực tiếp API riêng của nền tảng.
```

- **Phần dùng chung:** thư viện nền tảng .NET (BCL) giúp chia sẻ logic; MAUI bổ sung lớp UI và API chung cho mobile/desktop. [S1]
- **Phần phụ thuộc nền tảng:** macOS dùng Mac Catalyst, Windows dùng WinUI 3; vẫn phải quan tâm môi trường chạy và build tương ứng. [S1]
- **Single project:** gom tài nguyên dùng chung, thông tin ứng dụng, điểm khởi chạy chung và việc chọn mục tiêu debug; vẫn hỗ trợ phần riêng khi cần. [S1]

**Ứng dụng thực tế:** ưu tiên viết phần dùng chung trước; chỉ tách riêng nền tảng khi yêu cầu chức năng hoặc hành vi thực tế buộc phải làm vậy.

Chưa cần học sâu Mono/CLR, JIT/AOT hoặc tùy biến handler để bắt đầu dựng màn hình và xử lý dữ liệu. Quay lại khi gặp vấn đề build, hiệu năng hoặc UI native đặc thù.

## 4. Nền tảng và môi trường — kiểm tra trước khi đầu tư học

### 4.1. Hệ điều hành mà ứng dụng nhắm tới

Các mức dưới đây lấy từ **nhánh áp dụng cho MAUI 10 của Supported platforms**, không lấy yêu cầu riêng MAUI 11 hoặc minimum target của một project làm mức chung của framework. [S2]

| Nền tảng | Mức được tài liệu nêu | Điểm cần nhớ |
| --- | --- | --- |
| Android | Android 5.0, API 21 trở lên | Đây là mức hệ điều hành của thiết bị, không phải phiên bản Android SDK phải cài để build. |
| iOS | iOS 12.2 trở lên | Cần máy Mac cho việc build. |
| macOS | macOS 12 trở lên | MAUI sử dụng Mac Catalyst. |
| Windows | Windows 11 hoặc Windows 10 phiên bản 1809 trở lên | MAUI sử dụng WinUI 3. |

**Nếu dùng .NET MAUI Blazor**, nguồn còn nêu yêu cầu Android 7.0/API 24 trở lên, iOS 16.4 trở lên, macOS 12 trở lên và WebView của nền tảng được cập nhật. Không lấy mức tối thiểu của MAUI thông thường để mặc định cho MAUI Blazor. [S2]

Tài liệu cũng đề cập **Tizen do Samsung cung cấp hỗ trợ**. Chỉ đưa vào lộ trình ban đầu nếu sản phẩm thực sự nhắm tới Tizen. [S2]

### 4.2. Máy phát triển và công cụ

Theo tài liệu, khi sử dụng **VS Code cùng extension .NET MAUI**: [S2]

| Máy phát triển | Những đích build được tài liệu liệt kê | Điều kiện đáng chú ý |
| --- | --- | --- |
| Windows | Android, iOS, Windows | Phát triển iOS cần Mac kết nối qua mạng. |
| macOS | Android, iOS, macOS | Build cho các nền tảng Apple cần Mac. |

**Với bối cảnh bạn đang dùng VS Code trên Windows:**

- Nếu sản phẩm chưa chốt nền tảng, có thể bắt đầu bằng ứng dụng nhỏ trên Windows, rồi kiểm chứng phần dùng chung trên Android.
- Nếu sản phẩm ưu tiên iOS, xác nhận có Mac và đường build iOS ngay từ đầu; đừng để việc này thành trở ngại cuối dự án.
- Xác nhận công cụ phát triển MAUI đã được cài; sử dụng Codex trong VS Code không thay thế bước chuẩn bị môi trường MAUI.

**Đừng nhầm ba lớp yêu cầu:** hệ điều hành chạy ứng dụng, hệ điều hành máy phát triển và phiên bản bộ công cụ build. Với Xcode, Android SDK/JDK và Windows App SDK, nguồn yêu cầu tra ma trận [Release versions][R7] tương ứng với phiên bản MAUI cụ thể. Bản tổng hợp này không tự chọn phiên bản công cụ thay bạn. [S2]

### 4.3. Đối chiếu với project đang học

Project `../Project/MauiApp1/MauiApp1/MauiApp1.csproj` hiện đã nhắm **.NET 10**, không cần hạ/nâng target chỉ vì bộ ghi chú trước đây chọn sai phiên bản:

```xml
<TargetFrameworks>net10.0-android;net10.0-windows10.0.19041.0</TargetFrameworks>
<MauiXamlInflator>SourceGen</MauiXamlInflator>
```

- Project đặt minimum Android là API 24; đó là lựa chọn của project, không mâu thuẫn với mức hỗ trợ chung ở mục 4.1.
- `Microsoft.Maui.Controls` lấy version từ `$(MauiVersion)`; đừng hiểu chuỗi này là số phiên bản cố định. Đối chiếu SDK/workload và package thực tế khi gặp khác biệt API. MAUI được phân phối qua workload và NuGet. [S4]
- `MauiXamlInflator=SourceGen` đã bật trong project này; đọc phần Source Generation trong `XAML.md`, không coi nó là mặc định của mọi project MAUI 10.

Để kiểm tra môi trường mà không tự nâng cấp hay cài thêm workload:

```console
dotnet --info
dotnet --list-sdks
dotnet workload list
```

Các lệnh trên là hướng dẫn tự kiểm tra, không thay thế việc đối chiếu ma trận công cụ theo bản MAUI 10 cụ thể. Đợt cập nhật này chỉ sửa docs, không đổi project, SDK hoặc workload.

### 4.4. Những điểm MAUI 10 cần học khác tài liệu cũ

| Khi gặp trong bài học cũ | Cách đọc theo MAUI 10 | Đọc tiếp trong bộ docs |
| --- | --- | --- |
| Chỉ giải thích XAML runtime/XamlC | Bổ sung XAML Source Generation; phân biệt với compiled bindings. | `XAML.md` |
| Giả định safe area giống trước | Học `SafeAreaEdges`, thử thanh hệ thống và bàn phím. | `Fundamentals.md` |
| Handler danh sách phải opt-in trên iOS/Mac Catalyst | Handler tối ưu của `CollectionView`/`CarouselView` đã là mặc định. | `Handler.md` |
| Native input Android là `AppCompatEditText` | `Entry`/`Editor` dùng `MauiAppCompatEditText`. | `Handler.md` |
| Chọn media theo một kết quả duy nhất | Học `PickPhotosAsync`/`PickVideosAsync`, danh sách rỗng khi hủy và giới hạn nền tảng. | `Platform.md` |

Các thay đổi phiên bản được đối chiếu từ [S4]; chi tiết và nguồn chuyên biệt nằm trong từng file. **Không phải học lại từ đầu:** binding, MVVM, DI, Shell và single project vẫn là tuyến học chính.

## 5. Kiến thức có ROI cao — học theo đầu ra

Thứ tự này ưu tiên khả năng hoàn thành chức năng thực tế. Có thể đổi thứ tự nếu sản phẩm có rủi ro nền tảng cần kiểm chứng sớm.

| Ưu tiên | Học gì trước? | Vì sao đáng đầu tư? | Đầu ra chứng minh đã hiểu |
| --- | --- | --- | --- |
| 1 | Single project, chọn target, build và debug | Chưa chạy được ứng dụng thì khó kiểm chứng kiến thức tiếp theo. [S1] | Tự chạy project và xác định đang chạy trên nền tảng nào. |
| 2 | XAML, controls và layout cơ bản | Là nền tảng để dựng màn hình và tương tác. [S1] | Có màn hình với văn bản, dữ liệu nhập, nút thao tác và danh sách. |
| 3 | Data binding; MVVM ở mức cơ bản | MAUI hỗ trợ binding; MVVM là chủ đề trong tài liệu kiến trúc được giới thiệu. [S1], [S3] | Tổ chức dữ liệu và logic màn hình, tránh dồn mọi thứ vào xử lý UI. |
| 4 | Điều hướng và luồng nhiều màn hình | Một chức năng thường cần đi từ danh sách tới chi tiết rồi quay lại. [S1], [S3] | Hoàn thành luồng danh sách → chi tiết → quay lại. |
| 5 | Gọi REST, xử lý dữ liệu và lưu cục bộ | Learn path và workshop có nội dung này, giúp vượt qua mức UI mẫu. [S3] | Lấy dữ liệu, hiển thị và lưu một lựa chọn hoặc dữ liệu nhỏ. |
| 6 | Một API thiết bị mà sản phẩm cần | MAUI cung cấp API chung cho nhiều khả năng native. [S1] | Tích hợp chức năng như chọn file hoặc theo dõi trạng thái kết nối. |
| 7 | Hot Reload trong vòng lặp phát triển | Quan sát thay đổi XAML/C# khi ứng dụng đang chạy, giảm gián đoạn lúc chỉnh sửa. [S1] | Dùng Hot Reload cho thay đổi được hỗ trợ; vẫn kiểm tra lại bằng build/run. |

**Tự kiểm tra:** thử sửa một yêu cầu nhỏ mà không làm lại toàn bộ theo video. Nếu chưa giải thích được dữ liệu đến từ đâu, UI cập nhật thế nào hoặc phần nào là đặc thù nền tảng, quay lại đúng chỗ còn thiếu.

## 6. Dùng tài nguyên học tập theo thứ tự có chủ đích

Trang *Learning resources* là bản đồ tài nguyên, không phải nội dung đầy đủ của từng khóa. Mô tả dưới đây dựa trên trang đó; chưa xem trực tiếp các khóa, video hoặc mã nguồn liên kết. [S3]

| Tài nguyên được giới thiệu | Dùng vào lúc nào? | Mục tiêu sử dụng |
| --- | --- | --- |
| [.NET MAUI for beginners][R2] | Khi chưa hình dung cách bắt đầu | Xem chuỗi video ngắn để làm quen quy trình tạo ứng dụng đầu tiên. |
| [Build mobile and desktop apps with .NET MAUI][R1] | Làm tuyến học chính | Học nền tảng, sau đó tới lưu dữ liệu cục bộ và gọi REST web services. |
| [.NET MAUI workshop][R3] | Khi đã dựng được UI cơ bản | Thực hành danh sách khỉ: lấy JSON từ REST, hiển thị dữ liệu, vị trí/bản đồ và theme. |
| [.NET MAUI samples][R5] | Khi cần ví dụ cho vấn đề cụ thể | Đọc sample có mục tiêu; thử thay đổi nó để hiểu, không chỉ sao chép. |
| [Enterprise application patterns using .NET MAUI][R4] | Đọc chọn lọc khi tổ chức ứng dụng | Ưu tiên MVVM, điều hướng; bổ sung DI, cấu hình và giảm phụ thuộc giữa các thành phần khi cần. |
| [.NET MAUI podcast][R6] | Sau khi đã có nền tảng | Bổ sung góc nhìn và cập nhật kiến thức; không dùng thay thực hành cốt lõi. |

**Nguyên tắc:** một tuyến học chính + một bài thực hành xuyên suốt. Không cần học đồng thời mọi tài nguyên để cảm thấy mình đang tiến bộ.

## 7. Bài thực hành 80/20: danh sách và chi tiết

Đây là **bài tập đề xuất**, không phải tutorial nguyên văn trong ba tài liệu. Dùng miền dữ liệu gần sản phẩm của bạn: sản phẩm, công việc, khách hàng hoặc dữ liệu mẫu của workshop.

| Chặng | Việc làm | Tiêu chí hoàn thành |
| --- | --- | --- |
| A — Chạy được | Tạo project, chọn một nền tảng và chạy/debug. | Biết cách tái chạy ứng dụng và quan sát lỗi. |
| B — UI và dữ liệu | Hiển thị dữ liệu mẫu, dùng binding, mở màn hình chi tiết. | Tự thay dữ liệu và sửa bố cục mà không phụ thuộc hoàn toàn vào hướng dẫn. |
| C — Dữ liệu thực tế | Thay dữ liệu mẫu bằng REST; thêm trạng thái đang tải, rỗng, lỗi và thử lại. | Luồng chính không chỉ hoạt động khi mọi thứ thành công. |
| D — Giá trị native | Lưu một tùy chọn và tích hợp một API thiết bị có ích. | Giải thích được phần dùng chung và điều phải kiểm tra theo nền tảng. |
| E — Đa nền tảng | Chạy cùng luồng trên nền tảng thứ hai nếu sản phẩm cần; kiểm tra lại sau build. | Ghi nhận khác biệt UI/hành vi; chỉ tách code riêng ở chỗ có lý do. |

**Đích đến vòng đầu:** chưa phải sản phẩm production hoàn chỉnh, mà là một luồng đủ để kiểm chứng UI, dữ liệu, điều hướng, môi trường build và khả năng chia sẻ code.

## 8. Phần học sâu sau — và lúc nào phải quay lại

| Chủ đề | Có thể chưa đào sâu khi bắt đầu | Quay lại khi… |
| --- | --- | --- |
| Tùy biến handler, gọi trực tiếp API native | Luồng cơ bản chưa cần UI hoặc hành vi đặc thù. [S1] | Control/API chung không đáp ứng yêu cầu thực tế. |
| Graphics, canvas và xử lý hình vẽ | App ban đầu chỉ cần control thông thường. [S1] | Sản phẩm cần giao diện vẽ tùy biến. |
| Nhiều API cảm biến, xác thực, lưu trữ an toàn | Chỉ học API cần cho chức năng đang làm. [S1] | Tính năng yêu cầu; dữ liệu nhạy cảm phải xét bảo mật ngay khi thiết kế. |
| Kiến trúc enterprise đầy đủ | Chưa cần toàn bộ pattern cho một bài tập nhỏ. [S3] | Phụ thuộc và nhu cầu mở rộng đòi hỏi cách tổ chức rõ hơn. |
| Runtime, JIT/AOT và chi tiết biên dịch | Chỉ cần hiểu mỗi nền tảng có cơ chế build/chạy riêng. [S1] | Gặp vấn đề hiệu năng, tương thích hoặc build/release cần điều tra. |
| MAUI Blazor hoặc Tizen | Không đưa vào tuyến học chính nếu sản phẩm không dùng. [S2] | Phạm vi sản phẩm thực sự yêu cầu. |

**Không trì hoãn mọi thứ mang nhãn chuyên sâu.** Nếu sản phẩm bắt buộc có tính năng native khó hoặc nền tảng cụ thể, thử tính khả thi của nó sớm hơn thứ tự thông thường.

## 9. Checklist trước khi chuyển sang phần chuyên sâu

- [ ] Giải thích được MAUI giải quyết bài toán gì và phần nào có thể chia sẻ.
- [ ] Phân biệt được target OS, máy phát triển và bộ công cụ build.
- [ ] Tự chạy/debug project và hoàn thành luồng danh sách → chi tiết.
- [ ] Biết dữ liệu liên hệ với UI thế nào, không chỉ chỉnh giao diện tĩnh.
- [ ] Gọi REST và xử lý ít nhất tình huống đang tải, không có dữ liệu và lỗi.
- [ ] Kiểm chứng nền tảng quan trọng của sản phẩm, không suy ra từ việc chỉ chạy được trên máy mình.
- [ ] Biết tài nguyên nào cần mở tiếp để giải quyết vấn đề đang gặp.

Ba trang tổng quan **chưa đủ làm hướng dẫn triển khai production**: cần tài liệu riêng cho cài đặt chi tiết, quyền hệ thống, bảo mật, kiểm thử, ký gói và phát hành. Hoàn thành checklist học tập không có nghĩa đã xử lý xong các yêu cầu đó.

## 10. Nguồn và lưu ý phiên bản

| Mã | Tài liệu gốc | Vai trò trong bản tổng hợp |
| --- | --- | --- |
| [S1] | What is .NET MAUI? | Khái niệm, kiến trúc, khả năng, single project, API thiết bị và Hot Reload. |
| [S2] | Supported platforms for .NET MAUI apps | Hệ điều hành, điều kiện build, VS Code, MAUI Blazor và Tizen. |
| [S3] | Learning resources for .NET MAUI | Tài nguyên để tiếp tục học và nội dung được trang giới thiệu. |
| [S4] | What's new in .NET MAUI for .NET 10 | Đối chiếu thay đổi phiên bản, workload/NuGet và các mục cần học bổ sung. |

**Cách đọc nguồn:** bản cập nhật này đối chiếu trực tuyến nguồn Microsoft Learn, không chỉ dùng lại các file HTML đã cung cấp trước đây. Thông báo đăng nhập và thành phần điều hướng của trang không được coi là nội dung bài học.

**Lưu ý phiên bản:** Microsoft Learn có thể trả về nhiều khối `moniker` dù URL đã chọn MAUI 10. Chỉ dùng nội dung chung hoặc khối có `net-maui-10.0`; không suy ra phiên bản từ thẻ canonical. Các tài nguyên học mở rộng không có bộ chọn phiên bản vẫn cần kiểm tra target/API trước khi chép mẫu. Đây là tài liệu học MAUI 10, không phải tuyên bố về bản mới nhất hoặc vòng đời hỗ trợ.

[S1]: https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui?view=net-maui-10.0
[S2]: https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms?view=net-maui-10.0
[S3]: https://learn.microsoft.com/en-us/dotnet/maui/get-started/resources?view=net-maui-10.0
[S4]: https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0
[R1]: https://learn.microsoft.com/en-us/training/paths/build-apps-with-dotnet-maui
[R2]: https://www.youtube.com/playlist?list=PLdo4fOcmZ0oUBAdL2NwBpDs32zwGqb9DY
[R3]: https://github.com/dotnet-presentations/dotnet-maui-workshop
[R4]: https://learn.microsoft.com/en-us/dotnet/architecture/maui/
[R5]: https://learn.microsoft.com/en-us/samples/browse/?expanded=dotnet&products=dotnet-maui
[R6]: https://www.dotnetmauipodcast.com
[R7]: https://github.com/dotnet/maui/wiki/Release-Versions
