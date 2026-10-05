# 1. Page Kết quả:
- vẫn bị ghi đè title và label text "Hoàn thành mỗi lượt học....", phải khi hiển thị kết quả mới ko bị ghi đè nữa.

# 2. Chế độ học flashcard
- Tao muốn nó lật qua lật lại 2 mặt 1 mặt là tiếng anh 1 mặt tiếng việt á
- Cho thêm 1 cái tiếp tục học nữa, chứ mỗi lần bấm bắt đầu học lại học lại từ đầu, thêm phần đánh dấu sao nữa vd trong bộ 300 từ mà thuộc được 100 rồi, thì có thể đánh dấu cho 100 từ đó, và lúc học thì ko bị lặp lại, hoặc có thể ôn dễ hơn.
- Bỏ nút xuất/chia sẻ file excel đi.
- Khi bắt đầu học cho phép custom chế độ học, chiều học luôn chứ ko cần phải back ra phần "Bộ từ vựng" để đổi


# 3. Page Thư viện
- Phần title "Lớp của tôi" đang nằm lệch tao muốn nó nằm bên phải trên của các lớp luôn chứ ko nằm ở bên trái phần nav

- Thêm các nút back nữa, hiện tại nó đang nằm ở trên thanh app khá là khó chịu. Bỏ text "Bàn phím: tab để di chuyển...."

# 4. Về UXUI
- Phần học UX UI khá là khó chịu, đặc biệt các thao tác như lật thẻ, đã nhớ/chưa nhớ - tham khảo app/web Quizlet để làm lại phần này

- Về mặt UXUI - tao muốn hướng tới Liquid Glass, với tone màu nhẹ nhàng, ko gây mỏi mắt như Baby blue, các UX tao muốn mượt mà hơn có các animation để chuyển động nhìn xịn hơn.

- Sửa lại layout và tỉ lệ các màn hình cần có responsive.

# 5. Ở home page
- Mục lớp của tôi, các lớp vẫn chưa có spacing - nhìn như đang dính vào nhau, cần thêm gap vào đó, cả trong phần bộ từ của lớp nữa. các từ vựng trong bộ từ cũng vậy. Ở page kết quả nữa, cũng chưa có spacing.

- Chức năng sửa và xóa các lớp khi di chuột vào và bấm nút 3 chấm ở góc trên bên phải các thẻ lớp. Trong bộ từ ở bản android nếu ko có từ vựng thì sẽ có 1 khoảng trống khá khó chịu phải scroll xuống mới thấy button delete.

- Nút tải lại, ko cần thiết lắm, trên bản android, nút đó cũng đang bị dính với các card lớp.

- Ko gõ được text ở bản android khi xài keyboard bên ngoài à, phải xài show on screen keyboard có sẵn của emulator.

- Ở trên android emulator cũng phải làm scale font chữ, resposive cho các resolution luôn. Có vẻ như chưa hỗ trợ scale đúng

- Phần tạo lớp cũng vậy, nếu ko nhập gì, chỉ bấm tạo thì hiện lỗi 

# 6. UXUI include all platform
- Ở HomePage:
+ Phần action ... ở mục "Từ mới hôm nay. Tự tin ngày mai", tao muốn bỏ đi và thay bằng button "Tiếp tục học" ở ngay dưới phần card "Bộ từ - thẻ".
- Các phần card như lớp, bộ từ ko cần phải bấm vô action mới mở được, chuyển thành 1 click là mở được, action chỉ để "Xóa, sửa" thôi ko còn mở trong dialog nữa.
- Phần flashcard:
+ Ở trên windows, tao muốn có hiệu ứng lật qua lật lại của flashcard
+ Các action như nghe tiếng anh (đổi thành nghe phát âm) rồi đánh dấu đã thuộc, lật thẻ, đã nhớ cũng để ở ngoài luôn, rồi bỏ phần dialog action đi 
- Trong phần bộ từ vựng:
+ mục đánh dấu sao (đã thuộc) cho phép đánh dấu ở ngoài ko cần trong dialog action
+ các tính năng thêm từ, import excel, bắt đầu học,... đều để ở ngoài luôn ko cần để trong dialog