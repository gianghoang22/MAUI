> **Bản lưu hướng cũ.** Project đã chuyển sang VocabMate. Xem `VocabMate-roadmap.md` và `VocabMate-architecture.md`; đường dẫn code StudyMate bên dưới không còn đại diện cho ứng dụng hiện tại.

# StudyMate — kế hoạch hai checkpoint

## Nguyên tắc

Một sản phẩm xuyên suốt. Hoàn thiện luồng sử dụng trước, sau đó thêm từng tính năng để giải thích được kiến thức MAUI tương ứng. Không cần backend trong hai checkpoint này.

Roadmap học ban đầu tham chiếu .NET MAUI 9; project thực tế đang target .NET 10. Không hạ framework chỉ để khớp tham số `view` của tài liệu. Đối chiếu tài liệu với framework thực tế khi làm bài.

## Checkpoint 1 — nền tảng ứng dụng

### Đã triển khai trong MVP, cần tự chạy demo nghiệm thu

- [x] 5 màn hình và navigation Shell qua ID môn/nhiệm vụ.
- [x] XAML layout, binding hai chiều, `INotifyPropertyChanged`, command, `x:DataType`.
- [x] MVVM và constructor injection cho repository, ViewModel và trang.
- [x] CRUD môn học/nhiệm vụ, validation tên trống/trùng và xác nhận xóa.
- [x] Tìm kiếm, lọc trạng thái, tính tiến độ, quy tắc quá hạn theo ngày.
- [x] Lưu local, bảo vệ file chính khi đọc lỗi và chặn thao tác ghi đồng thời trong app.
- [x] Resource dictionary, style sáng/tối, data template cho nhiệm vụ.
- [x] Data trigger cho trạng thái quá hạn/hoàn thành.
- [x] `ProgressCard` với bindable properties và control template.
- [x] Semantic heading và nhãn mô tả cho các thao tác chính; chưa đánh giá accessibility đầy đủ.

### Bài học tiếp theo trước khi chốt checkpoint 1

- [ ] **Behaviors:** tách kiểm tra tiêu đề thành behavior hiển thị lỗi ngay khi nhập; repository vẫn giữ validation bảo vệ dữ liệu.
- [ ] **Localization:** dùng resource `.resx` Việt/Anh, chuyển ngôn ngữ và cập nhật UI; không chỉ đổi riêng một màn hình.
- [ ] **Lifecycle:** lưu/khôi phục bản nháp riêng với dữ liệu đã xác nhận; kiểm tra background, resume và app bị hệ điều hành dừng.
- [ ] **Handler:** `StudyEntry` riêng, một tùy biến native trên Android; chứng minh `Entry` thường không bị ảnh hưởng.
- [ ] **Accessibility:** đọc màn hình, cỡ chữ lớn, tương phản sáng/tối, thứ tự focus và điều hướng bàn phím Windows.
- [ ] **Single project:** giải thích target framework, tài nguyên, platform bootstrap và cấu hình build Android/Windows hiện có.
- [ ] **XAML Hot Reload:** demo một thay đổi UI, giải thích giới hạn so với thay đổi C# và cấu hình native.
- [ ] Chạy checklist README trên Windows và Android; ghi rõ thiết bị/emulator, OS, kết quả và lỗi còn lại.
- [ ] Tạo video demo ngắn và tự giải thích luồng từ nhấn Lưu đến cập nhật Dashboard.

### Thứ tự đọc code để tự học

1. `Models/StudyData.cs`: quan hệ một môn có nhiều nhiệm vụ; không lưu tiến độ trùng lặp trong model.
2. `ViewModels/ViewModelBase.cs`: tại sao UI biết property thay đổi, cách chặn double click.
3. `ViewModels/SubjectsViewModel.cs` + `Views/SubjectsPage.xaml`: binding/command/DI trong luồng nhỏ nhất.
4. `Services/JsonStudyRepository.cs`: validation, ghi dữ liệu, xử lý lỗi và trade-off JSON.
5. `AppShell.xaml.cs` + `Views/TaskDetailPage.xaml.cs`: route, ID, lifetime và `IQueryAttributable`.
6. `ViewModels/TaskDetailViewModel.cs`: form sửa trên bản nháp trong RAM, chỉ persist khi nhấn Lưu.
7. `Themes/StudyTheme.xaml` + `Controls/ProgressCard.xaml`: resources, trigger, template, bindable properties.

Các đường dẫn ở mục này tính từ thư mục `MauiApp1/`.

## Checkpoint 2 — tích hợp thiết bị

Chưa triển khai. Chỉ bắt đầu khi luồng dữ liệu checkpoint 1 ổn định.

- [ ] **MediaPicker:** chụp/chọn ảnh tài liệu cho nhiệm vụ.
- [ ] **FilePicker:** đính kèm PDF hoặc tài liệu; sao chép file vào thư mục app thay vì chỉ giữ đường dẫn tạm bên ngoài.
- [ ] **Share:** chia sẻ ghi chú hoặc bản báo cáo local do app tạo.
- [ ] **Permissions:** từ chối quyền, hủy picker, quyền bị thu hồi sau lần sử dụng trước.
- [ ] **Quản lý attachment:** file không tồn tại, dung lượng lớn, tên trùng, dọn file khi xóa nhiệm vụ.
- [ ] **Android:** cấu hình quyền cần thiết, kiểm thử thiết bị thực/emulator và deep link nếu rubric yêu cầu.
- [ ] **iOS:** chỉ thêm target khi có môi trường build/test phù hợp; cấu hình usage descriptions, signing và kiểm thử quyền; không ghi nhận pass nếu chỉ build Windows/Android.
- [ ] **Platform fallback:** thiết bị không có camera hoặc không có ứng dụng xử lý file vẫn nhận thông báo rõ ràng.

## Không làm trong scope ban đầu

Login, cloud sync, AI, chat, push notification, gamification, báo cáo phức tạp và dịch vụ backend. Chỉ thêm nếu có yêu cầu checkpoint cụ thể.
