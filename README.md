# WebApp
Một dự án học tập với mục tiêu là xây dựng một webiste bán đồ ăn nhanh sử dụng ASP.Net

# Instructions
Truy cập tài khoản google cá nhân. Chọn mục bảo mật -> thực hiện bảo mật 2 lớp (nếu chưa)
Sau đó trên thanh tìm kiến nhập `Mật khẩu ứng dụng` -> tạo một mật khẩu ứng dụng -> lưu lại mật khẩu

Sau khi pull về thì thực hiện các bước sau:
  1) Sau khi mở project, click chuột phải vào project name `FastFood.MVC`
  2) Chọn `Open in terminal`
  3) Thực hiện các lệnh sau
       - `dotnet ef database update`
  4) Click chuột phải vào project name `FastFood.MVC`
  5) Chọn `Manage User Secrets`
  6) Nếu có gì trước đó thì xóa đi, thêm vào mấy dòng sau:
     ```
     "AdminCredentials": { 
        "Email": "admin@example.com", // Account dành cho admin, có thể thay đổi được
        "Password": "admin@123"
      },
     "EmailSettings: {
       "Email": "điền email vào đây",
       "Password": "điền mật khẩu ứng dụng đã tạo bên trên vào đây"
     }
     ```
  7) ~
