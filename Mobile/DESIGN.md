# Thiết kế Việc nhỏ

Ứng dụng Android học MAUI, hai tab: Việc nhỏ và Khám phá. Mục tiêu là đọc dễ, thao tác rõ và nhìn được luồng dữ liệu.

Tham khảo `ui-ux-pro-max`: kết quả Flat Design phù hợp native app. Các đề xuất landing page/video và font web không phù hợp; không áp dụng. MAUI chưa có stack riêng trong bộ dữ liệu skill. Hướng dẫn native typography đã được đối chiếu qua truy vấn `dynamic type` (domain web/app-interface).

- Nền sáng dịu, màu chính teal đậm, card phẳng; theme tối tương ứng.
- Font hệ thống Android, hỗ trợ tiếng Việt và font scale. Body 16, phụ 14, heading 20–36.
- Spacing 8/16/24, bo góc 12/16; nội dung rộng tối đa 640 DIP.
- Nút native cao tối thiểu 48 DIP, nhãn bằng chữ, có trạng thái pressed/disabled/focused.
- List có header cuộn cùng nội dung, không nhét CollectionView trong ScrollView.
- Hoàn thành có chữ trạng thái + gạch ngang, không chỉ đổi màu.
- Lỗi có nội dung cụ thể, giữ dữ liệu nhập; có loading và retry khi chưa tải được.
- Xóa có hộp thoại xác nhận/hủy; không cần gesture ẩn.
- Không có hoạt ảnh trang trí; sử dụng safe area và font scaling của MAUI.

Nguồn token thực tế: `Resources/Styles/Colors.xaml` và `Styles.xaml`. Trạng thái kiểm thử thực tế được ghi trong `VERIFICATION.md`; không coi checklist thiết kế là bằng chứng đã kiểm tra mọi thiết bị.
