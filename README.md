# WebApp

# Instructions
Đăng kí tài khoản SendGrid để dùng dịch vụ gửi mail (dùng để confirm email hoặc rest password).
  Lưu ý: Nhớ tạo API Key (lưu lại) và thực hiện Single Sender Verification
Sau đó vào EmailSender.cs để nhập email đã đăng kí của mình (thay vào chỗ 'hoaiktl.com.vn@gmail.com')

Sau khi pull về thì thực hiện các bước sau:
  1) Sau khi mở project, click chuột phải vào project name "FastFood.MVC"
  2) Chọn "Open in terminal"
  3) Thực hiện các lệnh sau
       - dotnet user-secrets set SendGridKey <API Key> // API Key tạo trên SendGrid
       - dotnet ef database update
  4) Click chuột phải vào project name "FastFood.MVC"
  5) Chọn "Manage User Secrets"
  6) Thêm vào mấy dòng sau:
     '''
     "AdminCredentials": { 
        "Email": "admin@example.com", // Account dành cho admin, có thể thay đổi được
        "Password": "admin@123"
      }
     '''
  7) ~
