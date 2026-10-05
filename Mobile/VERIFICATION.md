# Kiểm chứng bàn giao — 05/10/2026

Môi trường: Windows, .NET SDK 10.0.401, MAUI Controls 10.0.20, Android SDK API 36, JDK 21.0.12. Emulator Android 16/API 36, x86_64.

| Hạng mục | Kết quả |
| --- | --- |
| Build Debug | Thành công, 0 warning, 0 error |
| Build Release, trimming và AOT mặc định | Thành công, 0 warning, 0 error |
| Unit/integration tests trên .NET | 14/14 qua |
| Smoke test UI Android Debug | 7/7 qua |
| CRUD và mở lại tiến trình trên Release | Qua; công việc và trạng thái được giữ |
| Hai theme sáng/tối | Đã xem ảnh của cả hai trang |
| 12 cặp màu chữ/nền | Đều đạt tối thiểu 4.5:1; cặp thấp nhất đã đo là 5.27:1 |
| Màn hình nhỏ, chữ lớn | 320dp rộng, font scale 2.0; bộ đếm, đổi tab, thêm việc và lỗi nhập vẫn thao tác được bằng cuộn |
| Đổi font scale khi app đang chạy | Đã kiểm tra Activity tạo lại và chuyển tab sau đó |
| Xoay ngang | Hai trang hiển thị, nội dung cuộn; đã xem ảnh |

Smoke test gồm: báo nhập rỗng; thêm và kiểm tra JSON thật; hoàn thành và kiểm tra JSON; force-stop/mở lại/làm lại; hủy xóa; xác nhận xóa; bộ đếm tăng, giữ qua đổi tab và đặt lại.

Các test .NET kiểm tra: Unicode, khoảng trắng, giới hạn tên, việc trùng tên nhưng khác ID, JSON hỏng không bị ghi đè, lỗi đọc/ghi không tạo trạng thái thành công giả, thao tác thêm liên tiếp bị khóa trong khi lưu.

APK nằm ở `artifacts/ViecNho-debug.apk` (khoảng 80 MiB) và `artifacts/ViecNho-release.apk` (khoảng 27 MiB); chứa `arm64-v8a` và `x86_64`. Đây là APK phục vụ học/thử, chưa cấu hình signing key phát hành lên cửa hàng.

Bằng chứng cục bộ (được gitignore):

- `artifacts/debug-build.log`, `artifacts/release-build.log`.
- `artifacts/android/results.json`: bảy kiểm tra smoke.
- `artifacts/android/01-initial.png`, `02-completed.png`, `03-learn.png`.
- `artifacts/android/dark-tasks.png`, `dark-learn.png`.
- `artifacts/android/small-large-text-validation.png`, `small-large-text-counter.png`.
- `artifacts/android/landscape-tasks.png`, `landscape-learn.png`.

Giới hạn kiểm chứng: chưa thử điện thoại ARM64 thật, Android API 24, tablet hoặc TalkBack bằng nghe/điều hướng thực tế. Đã dùng semantic labels/hints, native controls và screen reader announcements, nhưng không coi kiểm tra XML/ảnh là chứng nhận accessibility đầy đủ. Cấu hình màn hình/theme/cỡ chữ của emulator đã được trả về giá trị ban đầu sau kiểm tra.

Hai lỗi vòng đời tìm thấy khi thử máy đã được sửa: tạo trang trước resources của App; dùng lại trang/handler cũ sau Activity recreation. README giải thích cả hai để người học hiểu vì sao build thành công vẫn cần chạy trên Android.
