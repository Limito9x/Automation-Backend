# Kế Hoạch Triển Khai Lớn (Master Plan) Cho Ngày Mai

Kế hoạch tập trung vào tiêu chí **"Đánh nhanh thắng nhanh"**, hiện thực hóa sức mạnh thực tế của Agent: Tự động chạy ngầm Worker, tạo Local Workspace, tự động quét/đồng bộ Resource cục bộ và chạy Resource Inspector.

---

## 🎯 Mục Tiêu Trọng Tâm

### 1. Tự Động Kích Hoạt Worker & Heartbeat (`Automation-Worker`)
- **Tự động khởi chạy Daemon**: `run_worker.bat` / Python Daemon tự đọc `agent_config.json`, tự chạy ngầm tiến trình Worker.
- **Tự động Heartbeat (Báo sống)**: Định kỳ 30 giây một lần, Worker gửi Heartbeat báo `LastSeenAt` về Backend để Agent trên Web Dashboard luôn sáng đèn xanh **Online**.

### 2. Quản Lý Local Workspace (`Automation.Resource`)
- **Chỉ hỗ trợ Local Workspace**: Triển khai API/UI Tạo Workspace.
- **Chặn Remote**: Trong Handler `CreateWorkspaceHandler`, nếu `Kind == WorkspaceKind.Remote` -> Ném ngay `NotImplementedException("Remote Workspaces are currently not supported.")`.
- **Ghi nhận RootPath**: Cho phép chỉ định thư mục cục bộ (vd: `D:/GameAssets/ProjectA`).

### 3. Đồng Bộ Resource Cục Bộ (Local Resource Sync Engine)
- **Local File Scanner (Worker)**: Worker đọc `RootPath` của Local Workspace, tự động quét toàn bộ cây thư mục/file bên trong.
- **Tính toán Hash SHA256**: Tránh upload trùng lặp.
- **Đẩy dữ liệu về Backend**: Worker gọi `POST /api/resources/sync-local` để khởi tạo danh sách `ResourceItem` & `ResourceVersion` trên Web Dashboard.

### 4. Dựng Resource Inspector (Worker -> Backend)
- **Inspector Engine (Python Worker)**:
  - Tự động soi chi tiết file vừa quét: Kích thước (Dimensions) ảnh, dung lượng file, loại MIME, phần mở rộng (Extension).
  - So khớp Extension của file với danh sách Extension cho phép của Platform.
- **Báo cáo kết quả**: Đẩy kết quả Inspection về module `Automation.Inspection` / `Automation.Resource`.

---

## 🛠️ Các Bước Thực Hiện Chi Tiết (Ngày Mai)

### Giai Đoạn 1: Worker Auto-Launcher & Heartbeat Loop
- [ ] [Automation-Worker] Viết `app/daemon.py` tự đọc `agent_config.json`, khởi tạo Heartbeat Loop (mỗi 30s).
- [ ] [Automation.Agent] Thêm Endpoint `POST /api/agents/heartbeat` cập nhật `LastSeenAt`.
- [ ] [Automation-Worker] Cập nhật `run_worker.bat` 1-Click tự kích hoạt nếu chưa có config, hoặc chạy daemon nếu đã có config.

### Giai Đoạn 2: Local Workspace Creation & Guard
- [ ] [Automation.Resource] Cập nhật `CreateWorkspaceHandler.cs`: Thêm logic kiểm tra `if (request.Kind == WorkspaceKind.Remote) throw new NotImplementedException(...);`.
- [ ] [Automation-Frontend] Thêm UI Dialog/Form tạo Workspace với lựa chọn Local (chọn đường dẫn `RootPath`).

### Giai Đoạn 3: Local Resource Sync Pipeline
- [ ] [Automation-Worker] Viết `scripts/file_scanner.py` quét file trong `RootPath`, tính SHA256.
- [ ] [Automation-Worker] Tích hợp gọi API `/api/resources/sync-local` kèm header `X-Agent-Secret`.
- [ ] [Automation-Frontend] Hiển thị danh sách Resource vừa được Agent đồng bộ lên giao diện Web.

### Giai Đoạn 4: Resource Inspector Engine
- [ ] [Automation-Worker] Viết `scripts/inspector.py` đọc metadata của file (Images/Text/Binaries).
- [ ] [Automation.Resource/Inspection] Lưu kết quả Inspector và hiển thị cờ Status (Valid/Invalid Extension) trên Web.

---

## 📋 Verification Plan (Xác Nhận Kết Quả)

1. **Test Auto-Worker**: Chạy `run_worker.bat` -> Màn hình Web `/agents` lập tức sáng đèn **Online**.
2. **Test Local Workspace**: Tạo Local Workspace chỉ định thư mục `D:/TestAssets` -> Nếu chọn Remote báo lỗi rõ ràng.
3. **Test Auto-Sync & Inspector**: Thả file ảnh/texture vào `D:/TestAssets` -> Worker tự phát hiện, tự soi metadata và đẩy hiển thị ngay lập tức lên Web Dashboard!
