# .NET MAUI 10 Platform Integration — học theo phương pháp 80/20

> **Mục tiêu:** chọn đúng API để dùng chức năng thiết bị, hiểu luồng Android App Links và phân biệt những thành phần cần thiết khi quản lý tài khoản/ký ứng dụng iOS.
>
> **Cập nhật:** 16/09/2026. **Phạm vi:** **.NET MAUI 10 trên .NET 10**. Đã đối chiếu lại ba nguồn Microsoft Learn ban đầu [P1]–[P3] theo phiên bản 10, bổ sung thay đổi platform/media picker [P6], [P7]. Giữ tài liệu Android [P4] và Apple [P5] làm nguồn đối chiếu. Không coi hướng dẫn giao diện Visual Studio là hướng dẫn giống hệt cho VS Code.

## 1. Đúng tinh thần 80/20 và phạm vi tài liệu

**Ưu tiên kiến thức có ROI cao trước, không bỏ phần còn lại.** Với phần platform, điều quan trọng là chọn đúng lớp API, hiểu điều kiện chạy và biết kiểm chứng trên nền tảng thật; không phải học thuộc mọi API thiết bị.

| Chủ đề | Cần đạt được trước | Chưa cần học hết |
| --- | --- | --- |
| Platform features | Tìm được API chung phù hợp và biết khi nào phải viết native code. | Tất cả sensor, media, communication API. |
| Android App Links | Theo được luồng domain → xác minh → intent → nội dung trong app. | Toàn bộ indexing, nhiều domain và mọi chính sách link của từng phiên bản Android. |
| Apple account management | Phân biệt account/team, API key, signing identity và provisioning profile. | Tự động hóa toàn bộ signing/distribution ngay từ đầu. |

Các bài tập, thứ tự ưu tiên và checklist là đề xuất của người tổng hợp. Đọc `Overview.md` để nối với yêu cầu máy phát triển; đọc `Fundamentals.md` để nối với lifecycle, DI và cấu trúc `Platforms`.

## 2. Chọn API theo nhu cầu, không theo tên nền tảng

MAUI cung cấp API chung cho nhiều chức năng vốn thuộc hệ điều hành. Khi API chung không đáp ứng, có thể dùng platform-specifics hoặc viết code gọi trực tiếp API native. [P1]

### Bản đồ tra cứu có ROI cao

| Nhu cầu | API/nhóm cần tìm | Cách phân biệt |
| --- | --- | --- |
| Thông tin app/thiết bị | `AppInfo`, `VersionTracking`, `DeviceInfo`, `DeviceDisplay` | Thông tin app không phải thông tin phần cứng/màn hình. |
| Mở web, app khác, xác thực qua browser | `Browser`, `Launcher`, `WebAuthenticator` | Mở trang, mở URI của app khác và nhận callback xác thực là ba việc khác nhau. |
| UI thread và quyền truy cập | `MainThread`, `Permissions` | Điều phối chạy trên UI khác với xin quyền hệ điều hành. |
| Kết nối mạng | `Connectivity` | Trạng thái mạng của thiết bị; không thay thế xử lý lỗi khi gọi dịch vụ. |
| Tệp và lưu trữ | `FilePicker`, `FileSystem`, `Preferences`, `SecureStorage` | Chọn tệp, thư mục app, tùy chọn và lưu key/value an toàn là những nhu cầu khác nhau. |
| Ảnh/video/chia sẻ | `MediaPicker`, `Screenshot`, `Share`, `Clipboard` | Chỉ học API cần cho luồng hiện tại. |
| Liên lạc và vị trí | `Email`, `PhoneDialer`, `Sms`, `Geolocation`, `Geocoding`, `Map` | Một số API mở ứng dụng hệ thống, không phải tự triển khai toàn bộ chức năng đó. |

Đây là bản đồ rút gọn từ [P1], không phải hướng dẫn chi tiết cho từng API. Phải mở trang API tương ứng để kiểm tra quyền, cấu hình nền tảng, khả năng hỗ trợ và lỗi có thể gặp.

### Quy trình đề xuất trước khi tích hợp

1. Mô tả một hành vi cụ thể: chọn ảnh, mở bản đồ, lưu tùy chọn hay nhận link?
2. Tìm API MAUI có sẵn trước khi tự viết native code.
3. Đọc phần setup của API trên từng target; không giả định một cấu hình áp dụng cho mọi nền tảng.
4. Thiết kế các trường hợp không có quyền, người dùng hủy, thiếu khả năng hỗ trợ hoặc mất mạng.
5. Thử trên thiết bị/nền tảng mục tiêu; cân nhắc đặt việc gọi API sau một service khi giúp dễ kiểm thử và thay thế.

**Ranh giới dữ liệu:** dùng `Preferences` cho tùy chọn thông thường, xét `SecureStorage` cho key/value nhạy cảm phù hợp; đừng coi một nơi lưu key/value bất kỳ là nơi an toàn để cất secret. [P1]

### 2.1. Các API platform bổ sung cần biết trong MAUI 10

| Nhu cầu | Điểm mới để tra cứu | Không được suy ra |
| --- | --- | --- |
| Chọn nhiều ảnh/video | `MediaPicker.PickPhotosAsync` và `PickVideosAsync` trả về `List<FileResult>`. | Kết quả không còn là một file duy nhất; hủy chọn trả danh sách rỗng. |
| Giới hạn/xử lý ảnh | `SelectionLimit`, `MaximumWidth`, `MaximumHeight`, `CompressionQuality`, `RotateImage`, `PreserveMetaData`. | Mọi picker đều thực thi giới hạn như nhau hoặc metadata luôn phù hợp để chia sẻ. |
| Kiểm tra dịch vụ vị trí | `Geolocation.IsEnabled`. | Dịch vụ đang bật không có nghĩa app đã được cấp quyền hoặc lấy vị trí chắc chắn thành công. |
| Kiểm tra rung/phản hồi xúc giác | `Vibration.IsSupported`, `HapticFeedback.IsSupported`. | Mọi thiết bị/nền tảng đều hỗ trợ chức năng đó. |

Các API mới được đối chiếu từ [P6]; chữ ký, hành vi hủy và giới hạn media picker từ [P7]. Bắt đầu bằng API sản phẩm cần, không gọi mọi tính năng chỉ để thử.

**Ví dụ gọi chọn ảnh trên MAUI 10:** đặt trong phương thức `async` được gọi từ UI thread, với namespace `Microsoft.Maui.Media`; đây chỉ là bước lấy kết quả, không phải toàn bộ luồng lưu/upload.

```csharp
var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
{
    SelectionLimit = 3,
    MaximumWidth = 1280,
    MaximumHeight = 1280,
    CompressionQuality = 85,
    RotateImage = true,
    PreserveMetaData = false
});
```

Sau lời gọi, kiểm tra `photos.Count == 0` để kết thúc luồng khi người dùng hủy. Duyệt từng `FileResult`, mở bằng `OpenReadAsync()` và dispose stream sau khi dùng; không mặc định `FullPath` là đường dẫn có thể đọc trực tiếp trên mọi nền tảng. Nếu chỉ cần một ảnh, dùng `SelectionLimit = 1` rồi kiểm tra kết quả trước khi lấy phần tử đầu. [P7]

**Giới hạn phải tự kiểm tra:** Windows không hỗ trợ `SelectionLimit`; một số picker Android có thể không thực thi nó. Kiểm tra số lượng kết quả trong app trước khi xử lý/upload, thay vì chỉ tin vào tùy chọn UI. Vẫn cần setup quyền/manifest/`Info.plist`, gọi picker trên UI thread và xử lý lỗi theo nền tảng. `PreserveMetaData=false` trong mẫu là lựa chọn cho việc xử lý ảnh, không phải cam kết đã làm sạch mọi dữ liệu nhạy cảm. [P7]

Các mục `SaveToGallery` và khôi phục tác vụ media picker Android trong [P7] được đánh dấu riêng MAUI 11; không đưa vào code MAUI 10 chỉ vì cùng trang có hiển thị chúng.

## 3. Android App Links: hiểu toàn bộ đường đi trước khi sửa code

### 3.1. Ba khái niệm không hoàn toàn giống nhau

| Khái niệm | Cách hiểu |
| --- | --- |
| Deep link | URI đưa tới nội dung cụ thể trong app; có thể dùng custom scheme. |
| Web link | Deep link dùng HTTP/HTTPS; không tự chứng minh app sở hữu domain. |
| Android App Link | Web link có cấu hình xác minh quan hệ website–app, thay vì chỉ đăng ký nhận một URI. |

Tài liệu Microsoft tập trung Android App Links. `AutoVerify = true` yêu cầu hệ điều hành xác minh, **không phải bằng chứng rằng việc xác minh đã thành công**. [P2], [P4]

```text
Người dùng mở HTTPS link
          |
          v
Android xét intent filter + trạng thái domain association
          |
          v
Activity nhận intent
          |
          v
App đọc và kiểm tra URI
          |
          v
Ánh xạ sang route/nội dung được cho phép
```

### 3.2. Website và app phải khớp nhau

File website phải nằm ở vị trí dạng:

```text
https://app.example.com/.well-known/assetlinks.json
```

Nó mô tả package Android và SHA-256 của certificate ký app. Dùng fingerprint của certificate ứng với **bản app thực sự đang được cài**, không mặc định bản debug và bản phát hành dùng cùng certificate. Nguồn cho phép khai báo nhiều fingerprint khi cần. [P2]

**JSON minh họa, chưa dùng để triển khai:** thay package/domain theo app của bạn và thay chuỗi placeholder bằng fingerprint hợp lệ. Fingerprint không phải private key; không đưa keystore hoặc private key vào file website.

```json
[
  {
    "relation": [
      "delegate_permission/common.handle_all_urls"
    ],
    "target": {
      "namespace": "android_app",
      "package_name": "com.example.taskapp",
      "sha256_cert_fingerprints": [
        "REPLACE_WITH_SHA256_OF_INSTALLED_APP_CERT"
      ]
    }
  }
]
```

Điều kiện phục vụ file: HTTPS, content type `application/json`, không redirect; mỗi host được hỗ trợ cần có file liên kết tương ứng. Kiểm tra cả nội dung JSON và việc thiết bị có tải được file, không chỉ thấy file tồn tại trên máy phát triển. [P2], [P4]

### 3.3. Intent filter phải khớp scheme, host và path mong muốn

Đoạn sau là **attribute để bổ sung lên `MainActivity` hiện có**, không phải một class/app hoàn chỉnh. Cần namespace `Android.App` và `Android.Content`; giữ nguyên cấu hình `[Activity(...)]` của project, không tạo thêm một `MainActivity` thứ hai.

```csharp
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "https",
    DataHost = "app.example.com",
    DataPathPrefix = "/tasks/",
    AutoVerify = true)]
```

Trong ví dụ, URI như `https://app.example.com/tasks/42` thuộc phạm vi filter. Kiểm tra cấu hình `Exported` của activity khi cần nhận intent từ ngoài. Domain/path/package trong mẫu chỉ là minh họa, phải đổi nhất quán với app và website thật. [P2]

### 3.4. Nhận được link chưa có nghĩa xử lý đúng

Nguồn MAUI 10 vẫn minh họa đăng ký `OnCreate` và `OnNewIntent` qua `ConfigureLifecycleEvents`, đọc `Intent.Action`/`Intent.Data`, rồi chuyển URI bằng `SendOnAppLinkRequestReceived`. `App` override `OnAppLinkRequestReceived` để xử lý tiếp. [P2]

**Checklist xử lý an toàn đề xuất:**

- Chỉ nhận scheme/host/path mà ứng dụng hỗ trợ; kiểm tra URI hợp lệ và kiểu/giới hạn của ID, query parameters.
- Ánh xạ vào route nội bộ được cho phép, không đưa nguyên URI bên ngoài vào điều hướng một cách mù quáng.
- Kiểm tra cả khi app chưa chạy và khi app đang chạy rồi nhận link mới.
- Chỉ điều hướng khi UI/window đã sẵn sàng và thực hiện ở ngữ cảnh UI phù hợp; nếu chưa sẵn sàng, cần cơ chế giữ yêu cầu để xử lý sau.
- Link không chứng minh người mở được quyền xem dữ liệu. Nội dung riêng tư vẫn cần xác thực và phân quyền.

[P2] cảnh báo app links là một đầu vào có thể bị lợi dụng; đoạn mẫu nhận URI không nên được coi là bộ xử lý bảo mật hoàn chỉnh. Việc điều hướng trực tiếp tới nội dung không có nghĩa bỏ kiểm tra quyền truy cập.

### 3.5. Kiểm thử: trạng thái verified mới chứng minh domain verification

**Lưu ý đối chiếu nguồn:** [P2] nhắc Google Play, Search Console và app indexing. Không lấy các bước đó làm điều kiện để bắt đầu kiểm thử cục bộ: tài liệu Android [P4] mô tả việc cài app trên thiết bị và kiểm tra xác minh bằng `adb`. Cũng không áp quy tắc “mọi host phải hợp lệ” cho mọi phiên bản: Android ghi rõ điều kiện này cho Android 11 trở xuống. Phải xét phiên bản Android đang test.

Ví dụ sau dành cho **thiết bị thử nghiệm Android 12+** dùng cơ chế domain verification tương ứng, có kết nối mạng. Nếu app target thấp hơn Android 12, đọc phần bật cơ chế xác minh mới trong [P4] trước. Thay package và URI bằng giá trị thật.

Lệnh đầu **reset trạng thái App Links của package trên thiết bị thử nghiệm**, sau đó yêu cầu xác minh lại:

```console
adb shell pm set-app-links --package com.example.taskapp 0 all
adb shell pm verify-app-links --re-verify com.example.taskapp
```

Chờ verification chạy bất đồng bộ, có thể vài phút, rồi kiểm tra:

```console
adb shell pm get-app-links com.example.taskapp
adb shell am start -W -a android.intent.action.VIEW -c android.intent.category.BROWSABLE -d "https://app.example.com/tasks/42"
```

Domain cần có trạng thái `verified`. `none` có thể là chưa hoàn tất; app mở được do người dùng chọn mặc định hoặc do force-approve không chứng minh website association đã được xác minh. Các lệnh trên là hướng dẫn, **không được chạy trong phiên soạn tài liệu này**. [P4]

| Trường hợp kiểm thử | Kết quả cần xác nhận |
| --- | --- |
| App chưa chạy, link hợp lệ | Mở đúng nội dung sau khi app sẵn sàng. |
| App đang chạy, nhận link mới | Xử lý URI mới, không giữ nhầm nội dung của lần trước. |
| Sai host, scheme, ID hoặc query | Bị từ chối/xử lý lỗi an toàn. |
| Cài bản app ký bằng certificate khác | Association được kiểm tra lại với fingerprint phù hợp. |
| Đổi `assetlinks.json` | Kiểm tra re-verification/cache, không chỉ tải lại trang web trên máy tính. |

## 4. Apple account management: phân biệt credential và mục đích

### 4.1. Trước hết, nguồn đang hướng dẫn công cụ nào?

[P3] hướng dẫn **Apple account management trong Visual Studio**, với đường dẫn menu `Tools > Options > Xamarin > Apple Accounts`. Đây **không phải menu của VS Code**. Vì bạn đang dùng VS Code, nên học mô hình tài khoản/signing trước; khi cấu hình thực tế phải theo luồng công cụ/build đang sử dụng, không đi tìm nguyên menu này trong VS Code.

Về tên chương trình và phạm vi, Apple phân biệt:

- **Apple Account/đăng ký developer miễn phí:** có thể dùng Personal Team trong Xcode để thử app trên thiết bị cá nhân, với giới hạn và yêu cầu reprovisioning.
- **Apple Developer Program:** phục vụ thêm capabilities, tài nguyên phát triển và phân phối qua các công cụ như App Store Connect.
- **Apple Developer Enterprise Program:** dành cho tổ chức đủ điều kiện cần phân phối nội bộ trực tiếp tới nhân viên; không phải một “App Store riêng” dùng cho mọi mục đích. [P5]

Không suy từ workflow Visual Studio trong [P3] rằng mọi hình thức thử app trên iPhone đều bắt buộc cùng một loại membership/API key. Ngược lại, khả năng Personal Team trong Xcode cũng không chứng minh mọi IDE hỗ trợ cùng workflow đó.

### 4.2. Bốn thứ dễ nhầm

| Thành phần | Vai trò | Không nên nhầm với |
| --- | --- | --- |
| Account và Team | Xác định danh tính, tổ chức và quyền được cấp. | Một certificate hoặc một file profile. |
| App Store Connect/Enterprise API key | Cho công cụ truy cập chức năng quản lý theo quyền của key. | Chứng chỉ trực tiếp ký mã ứng dụng. |
| Signing identity | Certificate cùng private key tương ứng để ký. | Chỉ có file certificate là đã đủ. |
| Provisioning profile | Thành phần cấu hình cần khớp với app và luồng ký/phân phối đang dùng. | API key dùng để quản lý tài khoản. |

Các phân biệt về API key và signing identity dựa trên workflow và trạng thái mà [P3] mô tả; phạm vi account/team được đối chiếu với [P5].

### 4.3. Workflow trong nguồn Microsoft

1. Kiểm tra team/membership, quyền và thỏa thuận Apple còn đang chờ được xem xét/chấp nhận.
2. Với chức năng Apple Accounts của Visual Studio trong nguồn, tạo API key phù hợp và cung cấp **Issuer ID**, **Key ID**, cùng file **private key**.
3. Thêm account vào công cụ, chọn đúng team rồi xem certificates/profiles.
4. Chỉ tạo signing certificate khi có nhu cầu và đủ quyền; tải profile phù hợp với quy trình build. [P3]

**Quyền của key:** nguồn ghi Visual Studio hỗ trợ **Team Keys**, không phải **Individual Keys** trong workflow này. Key có quyền Admin mới thực hiện được những tác vụ như tạo Bundle ID và provisioning profile mới; không suy ra mọi tác vụ đều nên dùng quyền Admin. Nếu tổ chức quản lý signing tập trung, phối hợp với người phụ trách thay vì tự tạo thêm credential. [P3]

### 4.4. Đọc trạng thái signing trước khi tạo certificate mới

| Trạng thái trong nguồn | Ý nghĩa/việc cần kiểm tra |
| --- | --- |
| `Valid` | Có certificate và private key phù hợp, chưa hết hạn. |
| `Not in Keychain` | Cần tìm signing identity đúng từ môi trường đang giữ private key; portal không cung cấp lại private key đó. |
| `Private key is missing` | Có certificate nhưng thiếu private key tương ứng; API private key không thay thế nó. |
| `Expired` | Certificate đã hết hạn; xử lý theo quy trình signing của đội, không xóa hàng loạt để thử. |

Tải provisioning profile không tự khôi phục private key đang thiếu. Việc tạo certificate mới cũng không thay thế kiểm tra đúng team, quyền và cấu hình build. [P3]

**Khuyến nghị bảo vệ credential:** không commit private key/signing material vào repo, không gửi chúng vào chat, không đặt trong log hoặc `assetlinks.json`. Chỉ lưu/chuyển qua quy trình được tổ chức cho phép. Tài liệu này không tạo key, không đăng nhập tài khoản và không thay đổi cấu hình signing.

## 5. Bảng gỡ lỗi có ROI cao

Đây là checklist đề xuất dựa trên nguồn, không bao quát mọi lỗi công cụ/nền tảng.

| Hiện tượng | Kiểm tra trước |
| --- | --- |
| API chạy trên một target nhưng không chạy ở target khác | Tài liệu setup/quyền/khả năng hỗ trợ của đúng API, không chỉ code C# chung. |
| Picker trả nhiều file hơn giới hạn hoặc không có file | Kiểm tra danh sách rỗng khi hủy và tự kiểm soát số lượng trên Windows/Android. |
| Connectivity báo có mạng nhưng gọi API thất bại | Endpoint, lỗi request và trạng thái dịch vụ; đừng coi mạng thiết bị là bằng chứng backend hoạt động. |
| Link mở browser thay vì app | Trạng thái verified, intent filter, host/path và lựa chọn mặc định của người dùng. |
| App Links chạy ở debug nhưng lỗi ở bản khác | Package và fingerprint certificate của bản đã cài. |
| Website có JSON nhưng verification vẫn lỗi | Đúng host/đường dẫn, HTTPS, MIME type, redirect, nội dung và độ trễ xác minh/cache. |
| Link chỉ chạy khi app khởi động mới | Kiểm tra cả luồng `OnCreate` và `OnNewIntent`. |
| Nhận URI nhưng không điều hướng | Kiểm tra URI, UI/window readiness và route nội bộ. |
| Không tìm thấy menu Apple Accounts | Đang dùng VS Code hay Visual Studio? Không trộn hướng dẫn giao diện. |
| Có certificate nhưng không ký được | Private key, team, thời hạn và profile; đừng chỉ nhìn tên certificate. |
| Không tạo được Bundle ID/profile | Quyền account/API key và quy trình quản trị team. |

## 6. Lộ trình thực hành theo đầu ra

| Chặng | Việc làm | Bằng chứng hoàn thành |
| --- | --- | --- |
| A — Một API thiết bị | Chọn đúng một nhu cầu của sản phẩm, đọc trang API tương ứng. | Chạy được và xử lý trường hợp hủy/từ chối/lỗi phù hợp. |
| B — App Link tối thiểu | Một host, một nhóm path, một package và certificate rõ ràng. | Domain verified và link mở đúng nội dung. |
| C — Tình huống thực tế | Test app đang/chưa chạy, URI sai và nội dung có phân quyền. | Không điều hướng nhầm hoặc bỏ qua kiểm tra quyền. |
| D — Chuẩn bị iOS | Liệt kê account/team, kiểu phân phối, công cụ, người quản lý signing. | Biết cần credential nào và ai được phép cung cấp, không tạo key tùy tiện. |

Không cần triển khai App Links hay quy trình Apple nếu sản phẩm chưa cần. Tuy nhiên, nếu đó là điều kiện bắt buộc, kiểm chứng sớm thay vì để cuối dự án.

### Học sâu sau, không bỏ qua

- Các API sensor/media riêng: khi có chức năng cụ thể cần dùng.
- Native code/platform-specifics: khi API chung không đáp ứng.
- Nhiều domain/path, App Links trên nhiều phiên bản Android: khi phạm vi hỗ trợ mở rộng.
- Browser authentication: khi sản phẩm có yêu cầu xác thực và callback; không coi App Links tự thay thế toàn bộ luồng đó.
- Signing automation và CI/CD: khi đã xác định rõ identity, profile, quyền và nơi lưu secret.

### Checklist vòng đầu

- [ ] Biết tìm API đúng thay vì mặc định phải viết native code.
- [ ] Phân biệt deep link, web link và verified Android App Link.
- [ ] Giải thích được quan hệ host, package và certificate fingerprint.
- [ ] Kiểm chứng cả domain verification lẫn xử lý URI bên trong app.
- [ ] Không coi link là bằng chứng người dùng có quyền xem nội dung.
- [ ] Phân biệt API key, certificate/private key và provisioning profile.
- [ ] Không áp nguyên hướng dẫn Visual Studio vào VS Code.

## 7. Nguồn, đối chiếu và giới hạn

| Mã | Nguồn | Vai trò |
| --- | --- | --- |
| [P1] | Platform features — Microsoft Learn | Link gốc: bản đồ các API tích hợp nền tảng. |
| [P2] | Android app links — Microsoft Learn | Link gốc: asset links, intent filter và luồng nhận URI trong MAUI. |
| [P3] | Apple account management — Microsoft Learn | Link gốc: quản lý account, keys, certificates/profiles bằng Visual Studio. |
| [P4] | Verify App Links — Android Developers | Bổ sung: kiểm thử ADB, domain state, khác biệt phiên bản và verification/cache. |
| [P5] | Developer account overview — Apple Developer | Bổ sung: account, Personal Team và phạm vi các developer programs. |
| [P6] | What's new in .NET MAUI for .NET 10 — Microsoft Learn | Thay đổi API platform của MAUI 10. |
| [P7] | Media picker — Microsoft Learn | Multi-select, xử lý ảnh, giới hạn và setup theo nền tảng. |

Nội dung Microsoft Learn đã được đối chiếu theo nhánh MAUI 10; không khẳng định đã đọc toàn bộ trang con trong danh mục API. Hướng dẫn Android/Apple là phần đối chiếu ngoài bộ link gốc và còn phụ thuộc OS/công cụ thực tế, không tự đổi phiên bản theo MAUI. Ví dụ cần thay placeholder và tích hợp vào project thật; chưa triển khai website, chạy `adb`, build app hoặc tạo credential trong phiên này.

[P1]: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/?view=net-maui-10.0
[P2]: https://learn.microsoft.com/en-us/dotnet/maui/android/app-links?view=net-maui-10.0
[P3]: https://learn.microsoft.com/en-us/dotnet/maui/ios/apple-account-management?view=net-maui-10.0
[P4]: https://developer.android.com/training/app-links/verify-applinks
[P5]: https://developer.apple.com/help/account/basics/about-your-developer-account/
[P6]: https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0
[P7]: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device-media/picker?view=net-maui-10.0
