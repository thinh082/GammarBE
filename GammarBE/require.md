### QUY ƯỚC CODE VỚI TUI
1. Luôn trả lời bằng tiếng việt kể cả plan làm ra
2. Các hàm điều dùng async await
3. Kiểu trả về dạng dynamic càng tốt
4. Kiểu trả về nên có key - value là code và message với code là theo quy ước của response code và message tùy vào kết quả trả ra
5. Dùng LINQ ENTITY với các biến chỉ đọc thì dùng asknotracking() và nhớ dùng await trong các LINQ nhé
6. Dùng try catch để bắt lỗi trong các trường hợp thực thi nhiều tác vụ
7. Dùng begin transaction và commit transaction trong các trường hợp thực thi nhiều tác vụ
8. Dùng HTTPGET và Post là chính 
9. CommonServices làm hàm dùng chung
10. Interface cho các service nằm trong class Services luôn không tạo class riêng, và interface chỉ có các phương thức cần thiết khi gọi tới ctr ở controller
11. Tên file để các từ tiếng anh cơ bản nhưng cmt vẫn là tiếng Việt ví dụ AuthServices/AuthController.
12. Không tự ý sửa các file model entities , nếu thấy plan có yêu cầu sửa thì báo lại cho tôi
13. Nên dùng transaction khi thực hiện nhiều tác vụ liên quan đến database
14. Luôn validate thật kỹ các trường hợp có thể xảy ra, ví dụ điển hình là Email không được trùng trong DB, hoặc khi update thì phải kiểm tra xem có tồn tại không