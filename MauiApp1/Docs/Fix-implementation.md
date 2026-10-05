# Triển khai Fix.md

Phạm vi lấy từ `Fix.md`, bao gồm dòng bổ sung về layout, tỉ lệ và responsive. Giữ nguyên file yêu cầu gốc.

## Thay đổi

| Yêu cầu | Cách triển khai |
| --- | --- |
| Kết quả rỗng bị chồng chữ | Heading ở hàng Auto riêng. Empty state và danh sách dùng IsEmpty/HasResults loại trừ nhau, không trộn Header với EmptyView native. |
| Lật flashcard hai mặt | Một mặt duy nhất mỗi lần, có nhãn Anh/Việt. Nhấn thẻ hoặc Lật để đổi qua lại; lưu mặt đang xem. |
| Thao tác nhớ/chưa nhớ | Một lần bấm lưu đánh giá, cập nhật sao và tự sang câu tiếp; không cần thêm lần bấm Next. |
| Sao và tiếp tục | Sao nghĩa là đã thuộc. Có bộ lọc Chưa thuộc / Đã thuộc / Tất cả. Đã nhớ tự gắn sao; Chưa nhớ bỏ sao. Nút Tiếp tục phiên ở bộ từ giữ đúng snapshot; Tiếp tục học sau kết quả tạo lượt mới theo bộ lọc. |
| Đổi cách học ngay trong phiên | Nút Tùy chỉnh mở mode, chiều học, bộ lọc; xác nhận trước khi thay phiên đang dở. |
| Bỏ xuất/chia sẻ bộ từ | Đã bỏ nút và command xuất khỏi Deck. Import và file mẫu vẫn còn. |
| Lớp của tôi đặt đúng vị trí | ListHeading nằm phía trên danh sách bên phải trên desktop, sau form trên Android. |
| Back và bỏ hướng dẫn phím dài | Back nằm trong nội dung các trang con; thanh navigation trên cùng được ẩn. Không còn đoạn hướng dẫn phím trên màn học. Phím tắt vẫn hoạt động. |
| Theme (cập nhật sau Fix.md) | Trắng chủ đạo, xanh rất nhạt, màu đặc và viền mảnh. Bỏ gradient, kính trong và shadow. Tăng khoảng cách ngoài trang/giữa vùng/trong thẻ; giữ animation lật 240 ms. |
| Responsive và tỉ lệ | CollectionWorkspace chia khoảng 30:70 từ 900 DIP; form clamp 280–400 DIP. Hẹp hơn chuyển hai vùng cuộn trên/dưới. AdaptiveColumns chia 1–3 cột cho form, tùy chỉnh và đáp án theo chiều rộng thực tế. |

## Quy tắc để không mất dữ liệu

- `IsStarred` mặc định false và `LearningSession.Filter` mặc định All khi đọc dữ liệu cũ. Giữ schema 1, không reset file hiện có.
- Sửa thẻ không tự bỏ sao. Xóa thẻ/bộ/lớp vẫn theo xác nhận và cascade hiện có.
- Điểm, sao và index của một lượt đánh giá flashcard được ghi cùng nhau; checkpoint cũ không được lùi tiến độ.
- Trắc nghiệm và viết không tự gắn sao. Bộ lọc áp dụng cho câu hỏi; trắc nghiệm có thể lấy đáp án nhiễu từ toàn bộ bộ từ.
- Lượt đang dở giữ nội dung snapshot. Thay đổi sao không tự loại câu khỏi snapshot giữa phiên; bộ lọc có hiệu lực khi bắt đầu lượt mới.
- Dữ liệu trong `studymate.json` không bị chỉnh sửa. Không chạy đồng thời bản cũ và mới để ghi cùng file.

## Ranh giới giao diện

Yêu cầu Liquid Glass ban đầu đã được thay bằng theme trắng/xanh nhạt theo yêu cầu mới. Không còn gradient/translucency/shadow trên các bề mặt. Nội dung chữ giữ tương phản trên cả theme sáng/tối. Animation lật tôn trọng cài đặt tắt animation của Windows; trên Android dùng hệ thống animation MAUI.

## Kiểm chứng

- Harness logic trong `obj/VocabSmoke`: 66 kiểm tra, gồm bộ 300 từ/100 từ đã thuộc, không lặp từ vừa đánh dấu, ôn sao chiều ngược, resume mặt thẻ và tương thích JSON cũ.
- UI harness trong `obj/FixMdUiRegression.ps1`: kiểm tra màn rỗng, bốn chế độ, lật hai lần, tiếp tục, sao, đổi tùy chỉnh trong trang và kích thước 390/640/950/1440 pixel.
- Build UI kiểm thử dùng repository và preference container riêng, không ghi dữ liệu của bản app người dùng đang mở.
- UI Automation kiểm tra bounds, focus và thao tác điều khiển; không thay thế test gõ IME, cảm giác animation, DPI khác hoặc thiết bị Android thật.
