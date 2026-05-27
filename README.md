# Dự án TrelloMini - Công cụ Quản lý Dự án Tối giản

## 1. Giới thiệu

**TrelloMini** là một ứng dụng web quản lý dự án được xây dựng bằng ASP.NET Core Web API và giao diện Single Page Application (SPA) sử dụng HTML, CSS, và JavaScript thuần. Dự án mô phỏng các chức năng cốt lõi của các công cụ nổi tiếng như Trello và Jira, cho phép người dùng tạo dự án, quản lý công việc theo bảng Kanban, và cộng tác với các thành viên trong nhóm.

Đây là một dự án thể hiện khả năng xây dựng một ứng dụng full-stack hoàn chỉnh, từ việc thiết kế API backend, quản lý cơ sở dữ liệu, cho đến việc xây dựng một giao diện người dùng tương tác và linh hoạt.

## 2. Các chức năng chính

*   **Xác thực người dùng:**
    *   Đăng nhập an toàn và tiện lợi thông qua tài khoản **Google (OAuth 2.0)**.
*   **Quản lý Dự án:**
    *   Tạo dự án mới với tên, khóa (key), và mô tả.
    *   Xem danh sách các dự án mà người dùng là thành viên.
*   **Quản lý Thành viên (dành cho Chủ dự án):**
    *   Mời thành viên mới vào dự án qua email.
    *   Xóa thành viên khỏi dự án.
*   **Bảng Kanban:**
    *   Giao diện bảng trực quan với các cột trạng thái (Vd: To Do, In Progress, Done).
    *   **Kéo và thả (Drag & Drop)** để di chuyển công việc giữa các cột.
*   **Quản lý Công việc (Tasks):**
    *   Tạo, cập nhật, và lưu trữ (archive) công việc.
    *   Giao việc (assign) cho các thành viên.
    *   Thiết lập độ ưu tiên, ngày hết hạn, và nhãn màu.
    *   Tìm kiếm và lọc công việc.
*   **Chi tiết Công việc (Modal):**
    *   Xem và chỉnh sửa chi tiết công việc trong một cửa sổ modal.
    *   Thêm **checklist** và **bình luận** vào công việc.
*   **Giao diện người dùng:**
    *   Thiết kế responsive, hoạt động tốt trên nhiều thiết bị.
    *   Hỗ trợ 2 chế độ giao diện **Sáng (Light Mode)** và **Tối (Dark Mode)**.

## 3. Công nghệ sử dụng

*   **Backend:**
    *   C#, ASP.NET Core Web API (.NET)
    *   Entity Framework Core (Code-First)
    *   RESTful API
*   **Frontend:**
    *   HTML5, CSS3, JavaScript (Vanilla JS)
    *   Kiến trúc Single Page Application (SPA)
*   **Xác thực & Phân quyền:**
    *   ASP.NET Core Authentication với Google OAuth 2.0 (Cookie-based).
    *   Custom Authorization Policy (`ProjectOwner`).
*   **Cơ sở dữ liệu:**
    *   Microsoft SQL Server
*   **Bảo mật:**
    *   Chống tấn công CSRF/XSRF cho SPA bằng Antiforgery Tokens.

## 4. Hình ảnh minh họa dự án
#### Trang lưu trữ task đã hoàn thành
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/929ef6e7-7db1-4fa3-a606-791453afb61b" />

#### Quản lý task theo từng project
<img width="303" height="289" alt="image" src="https://github.com/user-attachments/assets/5b578545-826c-46bf-bdfd-6a192349b2cf" />

#### Thành tìm kiếm task theo nhiều lựa chọn lọc
<img width="1583" height="94" alt="image" src="https://github.com/user-attachments/assets/aff3eec8-82d0-418b-89ea-cc54a47098eb" />


### Trang Đăng nhập (Login Page)
<img width="1919" height="1039" alt="image" src="https://github.com/user-attachments/assets/5222dfc2-4651-49cb-a31d-e1047fa50bee" />

### Trang Settings
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/f946bba4-5944-4fcb-ae6b-df77da619aa3" />
<img width="1919" height="1074" alt="image" src="https://github.com/user-attachments/assets/138c63ed-83b3-44c3-a666-9582d6a6a58d" />


### Trang Quản lý Dự án (Projects Page)
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/1822d07a-c9ba-4dd0-996c-61fa26680cc6" />

### Bảng Kanban chính (Main Kanban Board)
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/f48ce96d-6a99-43e6-a102-17f525cf0d94" />

### Cửa sổ chi tiết công việc (Task Detail Modal)
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/163c8e4e-1087-4612-abf5-5f660dce0cd7" />

### Giao diện Sáng & Tối (Light & Dark Mode)
#### Dark mode
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/0f2a390f-d2b4-42d1-9961-704905e25540" />

#### Light mode
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/4900b9fb-5519-44d8-a0f3-606437f79376" />

## 5. Hướng dẫn cài đặt và chạy dự án

### Yêu cầu hệ thống:
*   .NET SDK (phiên bản tương ứng với dự án, ví dụ .NET 8).
*   Microsoft SQL Server.
*   Một IDE như Visual Studio 2022 hoặc VS Code.

### Các bước cài đặt:

1.  **Clone repository về máy:**
    ```bash
    git clone https://github.com/letruonghuy/TrelloMini
    cd TrelloMini
    ```

2.  **Cấu hình Google OAuth:**
    *   Truy cập Google Cloud Console và tạo một OAuth 2.0 Client ID.
    *   Trong phần "Authorized redirect URIs", thêm `https://localhost:<your_port>/signin-google`.
    *   Sử dụng .NET User Secrets để lưu trữ Client ID và Client Secret một cách an toàn:
    ```bash
    # Chạy các lệnh này trong thư mục dự án (TrelloMini)
    dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID"
    dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_CLIENT_SECRET"
    ```
    *Lưu ý: `Program.cs` đã được cấu hình để đọc các giá trị này. Không lưu trữ chúng trực tiếp trong `appsettings.json`.*

3.  **Cấu hình Connection String:**
    *   Mở file `appsettings.json`.
    *   Thay đổi giá trị của `DefaultConnection` để trỏ tới instance SQL Server của bạn.

    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Data Source=YOUR_SERVER_NAME;Initial Catalog=JiraMiniDb;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
    }
    ```

4.  **Cập nhật cơ sở dữ liệu:**
    *   Mở **Package Manager Console** trong Visual Studio hoặc dùng terminal.
    *   Chạy lệnh sau để tạo cơ sở dữ liệu và seed dữ liệu ban đầu:
    ```powershell
    Update-Database
    ```
    *(Dự án được cấu hình để tự động chạy migrations khi khởi động, nhưng chạy lệnh này thủ công để đảm bảo mọi thứ đã sẵn sàng).*

5.  **Chạy ứng dụng:**
    *   Nhấn `F5` trong Visual Studio hoặc sử dụng lệnh `dotnet run`.
    *   Ứng dụng sẽ tự động chuyển hướng đến trang `/login.html`.

## 6. Thông tin tác giả

*   **Họ và tên:** Lê Trương Trường Huy
*   **MSSV:** 2224802010230

---
