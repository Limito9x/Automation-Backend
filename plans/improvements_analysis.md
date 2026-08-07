# Automation Studio — Những Điểm Khó Khăn & Hướng Cải Tiến

Dựa trên tài liệu thiết kế và kiến trúc hiện tại, dưới đây là các điểm cần cải tiến (bottlenecks/challenges) và hướng giải quyết tương ứng.

## 1. Quan hệ Polymorphic không có FK cứng (`TagLink`, `AssetLink`)
**Vấn đề:** 
`EntityType` + `EntityId` là pattern linh hoạt nhưng database không thể enforce referential integrity. Điều này dễ dẫn đến dữ liệu rác (orphan records) khi xoá entity mà quên dọn dẹp các link liên quan. Việc query eager-loading cross-module cũng phức tạp và không tận dụng được EF Core Navigation Property.
**Hướng cải tiến:**
- Dùng **Domain Events** (Wolverine): Khi module nghiệp vụ xoá entity, publish event → module Tag/Files lắng nghe và cleanup `TagLink`/`AssetLink` tương ứng.
- Đánh **index kết hợp** `(EntityType, EntityId)` trên `TagLink` và `AssetLink` để tối ưu tốc độ truy vấn.
- Tạo các **Contract interface** (VD: `ITaggableEntity`) để chuẩn hóa các type được phép liên kết.

## 2. Module `Content` với Dynamic Schema (JSONB `FieldsConfig` + `Values`)
**Vấn đề:**
- EF Core không hỗ trợ native JSONB validation. Dữ liệu lưu vào `Content.Values` có thể sai lệch so với schema động trong `ContentType.FieldsConfig`.
- Khi `FieldsConfig` bị chỉnh sửa, dữ liệu cũ có nguy cơ bị lỗi (schema drift).
**Hướng cải tiến:**
- Xây dựng một **`FieldValidator` service** chuyên biệt trong module Content để validate từng trường (field) theo type và constraints tại runtime trước khi lưu.
- Sử dụng **GIN index** trên PostgreSQL cho cột `Values` để tối ưu các query JSONB.
- Áp dụng **Soft Migration** cho schema: Khi sửa đổi `FieldsConfig`, cấm việc xóa trường trực tiếp (chỉ cho phép thêm, đổi tên hoặc đánh dấu deprecated).

## 3. Gộp khái niệm `Storage` vào trong module `Resource`
**Vấn đề:**
- Ban đầu `Storage` nằm riêng thành module độc lập, khiến module `Resource` phải FK vào cả `Storage` và module `Files` (Asset).
- Việc chia quá nhỏ đối với `Storage` (chỉ lưu config Local/R2) làm hệ thống thêm rườm rà không cần thiết.
**Hướng cải tiến:**
- Gộp luôn thực thể `Storage` vào module `Resource`.
- Rõ ràng hóa thiết kế: `Resource.AssetId` chỉ nên là con trỏ (shortcut) đến version mới nhất để query cho nhanh, còn lịch sử thay đổi file nằm ở `ResourceVersion.AssetId`.
- Như vậy `Resource` module giờ sẽ quản lý cả cấu hình nơi lưu trữ (Storage) lẫn dữ liệu phiên bản (Resource/ResourceVersion).

## 4. Module `Inspection` — Orchestration & Xử lý bất đồng bộ
**Vấn đề:**
- Quá trình chạy Inspection có thể mất nhiều thời gian do gọi Worker bên ngoài, không thể chạy đồng bộ chặn request của user.
- Logic phân tích kết quả JSONB linh động theo `RelevantFieldPath` phức tạp.
**Hướng cải tiến:**
- Tách làm 2 phase: Tạm thời implement manual trigger + chạy tác vụ phân tích (parse json) qua các background jobs (Wolverine/Hangfire).
- Đẩy logic parse kết quả vào một **`InspectionService`** nội bộ chuyên nhận `ResourceVersion` và parse cấu trúc JSON động thành các dòng `InspectionItem`.

## 5. Module `Pipeline` — DAG execution & Phức tạp đa cấu trúc
**Vấn đề:**
- Pipeline bản chất là một **DAG** (Directed Acyclic Graph), cần validate tránh vòng lặp (cycle) khi nối Node.
- Execution tracking realtime cần sự phối hợp phức tạp từ Worker trả kết quả về thông qua RabbitMQ.
**Hướng cải tiến:**
- Khi user save `PipelineEdge`, chạy thuật toán **Topological Sort** để kiểm tra tính hợp lệ của graph. Reject ngay nếu phát hiện vòng lặp.
- Thiết kế **`ToolRegistry`** map `HandlerKey` với các `IResolverTool` implementation để thực thi linh hoạt bằng C# Reflection/DI tại runtime thay vì hardcode.
- Quản lý trạng thái NodeExecution thông qua Polling ở phase đầu, sau đó nâng cấp lên SignalR nếu cần.

## 6. Module `Tag` — Cross-module tagging inconsistency
**Vấn đề:**
`TagCategory.AppliesTo` đang quy định cứng, dễ dẫn tới thiếu nhất quán khi áp dụng tag cho nhiều thực thể khác (không chỉ `Content` mà còn `InspectionItem`).
**Hướng cải tiến:**
- Coi trường `AppliesTo` chỉ là gợi ý UI/UX để lọc danh sách Tag. Backend vẫn cho phép TagLink được gắn tự do theo Polymorphic pattern.

