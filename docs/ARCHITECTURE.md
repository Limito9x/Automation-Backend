# Triết Lý Kiến Trúc Của Dự Án (Architecture Philosophy)

Tài liệu này đúc kết những tư tưởng, định hướng thiết kế và các bài học kinh nghiệm (bao gồm cả những thất bại từ mô hình Hybrid VSA trước đó) để định hình nên bộ khung kiến trúc hiện tại của dự án: **Modular Monolith + Vertical Slice Architecture**.

---

## 1. Tại Sao Lại Là Modular Monolith?

Thay vì phân tách mã nguồn thành vô số các microservices đắt đỏ hoặc nhồi nhét tất cả vào một khối Monolith hỗn độn, dự án chọn **Modular Monolith** vì những lý do cốt lõi sau:

- **Sự tách biệt của Microservices nhưng chi phí của Monolith:** Các module được phân tách ranh giới rõ ràng (chỉ giao tiếp qua Contracts hoặc Memory Bus), dễ dàng scale hoặc tách thành microservice thực sự sau này nếu cần thiết, nhưng hiện tại vẫn được triển khai dưới dạng một tiến trình duy nhất để tiết kiệm chi phí hạ tầng.
- **Không cồng kềnh, không "Nghi thức" (Ceremony):** Clean Architecture truyền thống thường ép buộc việc chia nhỏ một logic đơn giản thành hàng chục project con (Domain, Application, Infrastructure, Presentation, v.v.), dẫn đến một Solution chứa hàng trăm project vô nghĩa. Với Modular Monolith kết hợp VSA, mỗi module chỉ là **một project duy nhất** chứa toàn bộ các layer bên trong nó, vô cùng tinh gọn.
- **Thừa kế để tái sử dụng:** Mọi module đều được hưởng lợi từ `SharedKernel`. Các cấu hình cốt lõi (như Middleware, định dạng response, exception handling) được cài đặt một lần và dùng chung, giảm thiểu tối đa việc thiết lập lại (boilerplate) ở từng module.
- **Tôn trọng tính đặc thù:** Dù dùng chung `SharedKernel`, mỗi module vẫn có quyền tự quyết định cấu trúc sâu bên trong nếu cần. Ví dụ: Module `Identity` có thể tích hợp thẳng ASP.NET Core Identity để xử lý bảo mật mà không bị gò bó bởi quy tắc chung.

---

## 2. Tại Sao Lại Là Vertical Slice Architecture (VSA)?

Chúng ta từ bỏ hoàn toàn kiến trúc phân tầng truyền thống (Layered Architecture: *Controller -> Service -> Repository*) để chuyển sang **Kiến trúc theo chiều dọc (Vertical Slice)**.

- **Tối đa hoá sự cô lập (High Cohesion, Low Coupling):** Mỗi tính năng (Feature/Use Case) là một "lát cắt" độc lập từ giao diện API xuống tận Database. Khi bạn sửa đổi tính năng `CreateInvoice`, bạn chỉ đụng vào đúng thư mục `CreateInvoice` mà không sợ làm "vỡ" tính năng `UpdateInvoice` hay bất kì tính năng nào khác.
- **Tập trung vào Use Case:** Code được tổ chức theo những gì hệ thống *Làm* thay vì hệ thống *Là gì*. Nhìn vào thư mục `Features`, bất cứ ai cũng hiểu ngay module này làm được những chức năng gì.
- **Bỏ qua các tầng trung gian thừa thãi:** Không cần tạo ra những interface hay service rỗng tuếch chỉ để "tuân thủ quy tắc truyền dữ liệu qua các tầng". Handler của VSA gọi thẳng vào DbContext nếu điều đó giải quyết được bài toán một cách hiệu quả và trực tiếp nhất.

## 3. Tổng Kết
Kiến trúc này được sinh ra để **thực dụng hóa quá trình phát triển**. Chúng ta tối ưu cho việc đọc hiểu code (Readability), tối giản hóa các thủ tục không cần thiết (Ceremony), nhưng vẫn giữ được bộ khung vững chắc để hệ thống có thể mở rộng (Scale) mạnh mẽ trong tương lai.
