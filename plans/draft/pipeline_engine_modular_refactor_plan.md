# Kế Hoạch Tái Cấu Trúc Toàn Diện Pipeline Engine (Compiler & Runtime Modularization)

> **Mục tiêu:** Xóa bỏ toàn bộ "nợ kỹ thuật" (Technical Debt), giải tán các "God Class" (`DagPlanner` > 500 dòng, `PipelineExecutionEngine` > 500 dòng), tái cấu trúc Engine theo mô hình **Compiler / Virtual Machine** tiêu chuẩn. Đảm bảo code sạch, mỗi file không quá 100-150 dòng, dễ mở rộng tính năng mới (`Branch/If`, `While`, `ParallelForEach`) mà không lo phá vỡ hệ thống cũ.

---

## 1. Phân Tích Hiện Trạng & Vấn Đề (Pain Points)

Hiện tại, Engine đang gặp 4 vấn đề kiến trúc nghiêm trọng:

1. **God Classes (Trách nhiệm quá tải):**
   - `DagPlanner.cs`: Đang ôm đồm 5 trách nhiệm khác nhau (Parse DAG, Detect Cycle, Topo Sort, Group Subgraphs, Validate Pins).
   - `PipelineExecutionEngine.cs`: Đang ôm đồm 6 trách nhiệm (State Management, ForEach Iteration, Scope Context, Tool Execution, Agent Batch Packaging, Wolverine/RabbitMQ Dispatching).
2. **Tight Coupling (Ràng buộc chặt):**
   - Các logic kiểm tra kiểu dữ liệu (`JsonElement`, `IDictionary`, `Dictionary<string, string>`) nằm rải rác bên trong các nhánh `if-else` của Engine.
   - Logic tìm node con trong vòng lặp bị phụ thuộc vào dây Exec thay vì phân tích độc lập theo cấu trúc ngữ nghĩa đồ thị.
3. **Khó bảo trì & Dễ phát sinh lỗi dây chuyền:**
   - Mỗi lần sửa một lỗi nhỏ (như khoảng trắng trong tên pin) lại có nguy cơ ảnh hưởng đến luồng phân giải của các node khác.
   - Việc debug đòi hỏi phải lần theo các khối code dài hàng trăm dòng.

---

## 2. Kiến Trúc Mục Tiêu (Target Architecture)

Tách biệt Engine thành 2 tầng rõ rệt: **Compiler Tầng Trên (Build & Analysis)** và **Runtime Tầng Dưới (Execution & Memory)**.

```
Automation.Pipeline.Engine/
│
├── Compiler/                         # [TẦNG 1: BIÊN DỊCH & PHÂN TÍCH ĐỒ THỊ]
│   ├── IGraphCompiler.cs             # Interface biên dịch chung
│   ├── GraphCompiler.cs              # Điều phối quá trình compile DAG -> ExecutionPlan
│   ├── Analyzers/
│   │   ├── CycleDetector.cs          # Chuyên trách phát hiện chu trình (Tarjan/Kahn)
│   │   ├── ScopeAnalyzer.cs          # Chuyên trách phân tích Scope & Subgraphs (ForEach, Branch)
│   │   └── TopologicalSorter.cs      # Chuyên sắp xếp thứ tự thực thi Action & Pure nodes
│   └── Validators/
│       └── PreflightValidator.cs     # Kiểm tra tính toàn vẹn các chân Pin trước khi chạy
│
├── Runtime/                          # [TẦNG 2: MÁY ẢO & ĐIỀU PHỐI THỰC THI]
│   ├── IPipelineRuntime.cs           # Interface điều phối chính
│   ├── PipelineRuntime.cs            # Vòng lặp thực thi các Execution Instructions
│   │
│   ├── Scope/                        # Quản lý bộ nhớ Stack Frame & Biến
│   │   ├── ExecutionScope.cs         # Cấu trúc Scope phân cấp (Parent -> Child)
│   │   └── ScopeMemoryManager.cs     # Tra cứu & giải mã biến theo phạm vi
│   │
│   ├── Resolvers/                    # Phân giải dữ liệu đầu vào cho Node
│   │   ├── PinDataResolver.cs        # Nạp dữ liệu từ upstream node, config, runtime inputs
│   │   ├── DynamicPinResolver.cs     # Xử lý các chân pin động (FormatString, Template)
│   │   └── AssetResolver.cs          # Tải và bóc tách metadata file Asset
│   │
│   └── Executors/                    # Các bộ thực thi chuyên biệt (Decoupled Runners)
│       ├── IDagNodeExecutor.cs       # Interface executor chung
│       ├── DotNetToolExecutor.cs     # Chạy các Pure/Action C# Tools (.NET)
│       ├── FlowControlExecutor.cs    # Chạy các Scope điều khiển (ForEach, Yield Collection)
│       └── AgentStageDispatcher.cs   # Đóng gói và gửi task sang Agent qua RabbitMQ
│
└── Models/                           # [MÔ HÌNH DỮ LIỆU TINH GỌN]
    ├── CompiledExecutionPlan.cs      # Kết quả đầu ra của Compiler
    ├── ExecutionInstruction.cs       # Đơn vị lệnh thực thi
    └── RuntimeState.cs               # Trạng thái bộ nhớ thời gian thực
```

---

## 3. Chi Tiết Trách Nhiệm Từng Thành Phần

### 3.1. Compiler Layer (`Compiler/`)
| Component | Trách nhiệm duy nhất |
| :--- | :--- |
| **`GraphCompiler`** | Nhận `Pipeline` Entity $\rightarrow$ gọi tuần tự các Analyzers $\rightarrow$ trả về `CompiledExecutionPlan`. |
| **`ScopeAnalyzer`** | Nhận diện node `FlowControl`, tự động gom các Action & Pure nodes có liên kết dữ liệu/exec vào `BodySubgraph` độc lập. |
| **`CycleDetector`** | Kiểm tra vòng lặp vô tận trên các cạnh Exec và Data (loại trừ các chân Feedback như `Yield`). |
| **`PreflightValidator`** | Soát lỗi các pin `IsRequired` chưa được cắm dây và không có giá trị inline. |

### 3.2. Runtime Layer (`Runtime/`)
| Component | Trách nhiệm duy nhất |
| :--- | :--- |
| **`PipelineRuntime`** | Nhận `CompiledExecutionPlan` và `RuntimeState` $\rightarrow$ duyệt qua từng instruction và ủy quyền cho Executor tương ứng. |
| **`FlowControlExecutor`** | Quản lý vòng lặp `ForEach`: lặp qua Collection, khởi tạo `IterationContext`, kích hoạt `BodySubgraph`, gom `Yield` vào `ResultMap`. |
| **`DotNetToolExecutor`** | Nạp inputs qua `PinDataResolver`, thực thi `ITool.ExecuteAsync` và ghi outputs vào State. |
| **`AgentStageDispatcher`** | Gom các node Agent liên tiếp cùng Executor (Blender/Unreal), đóng gói `StageTaskMessage` và bắn qua MessageBus. |
| **`PinDataResolver`** | Chuẩn hóa dữ liệu đa hình (`JsonElement`, `IDictionary`, `String`, `EntityRef`), hỗ trợ so khớp không phân biệt khoảng trắng/gạch dưới. |

---

## 4. Lộ Trình Triển Khai Từng Bước (Phased Execution Plan)

### Phase 1: Chuẩn bị Models & Interfaces (Zero Breaking Changes)
- Định nghĩa `CompiledExecutionPlan`, `ExecutionInstruction`, `IDagNodeExecutor`.
- Tạo các interface `IGraphCompiler`, `IPinDataResolver`, `IScopeMemoryManager`.

### Phase 2: Tách Tầng Compiler
- Chuyển logic từ `DagPlanner.cs` sang:
  - `Analyzers/ScopeAnalyzer.cs` (~100 dòng).
  - `Analyzers/CycleDetector.cs` (~80 dòng).
  - `Analyzers/TopologicalSorter.cs` (~90 dòng).
  - `Validators/PreflightValidator.cs` (~70 dòng).
- `DagPlanner.cs` trở thành facade mỏng gọi `GraphCompiler`.

### Phase 3: Tách Tầng Resolvers
- Chuyển logic từ `InputResolver.cs` sang:
  - `Resolvers/PinDataResolver.cs` (Xử lý mapping dây nối & fuzzy name matching).
  - `Resolvers/DynamicPinResolver.cs` (Xử lý dynamic input pins).
  - `Resolvers/AssetResolver.cs` (Xử lý resolve file asset).

### Phase 4: Tách Tầng Executors & Runtime
- Tách `PipelineExecutionEngine.cs` thành:
  - `Executors/DotNetToolExecutor.cs` (~90 dòng).
  - `Executors/FlowControlExecutor.cs` (~120 dòng).
  - `Executors/AgentStageDispatcher.cs` (~100 dòng).
  - `Runtime/PipelineRuntime.cs` (~100 dòng - chỉ điều phối vòng lặp chính).

### Phase 5: Kiểm Thử Độc Lập (Unit Tests) & Xác Nhận E2E
- Viết Unit Test độc lập cho `ScopeAnalyzer` (kiểm tra gom Subgraph).
- Viết Unit Test cho `PinDataResolver` (kiểm tra match tên pin có dấu cách, kiểu dữ liệu JSON).
- Chạy thử nghiệm toàn bộ Pipeline E2E (Daz Import $\rightarrow$ Save Scene $\rightarrow$ ForEach Modular FBX Export).

---

## 5. Kết Quả Kỳ Vọng Sau Khi Refactor

1. **Codebase sạch và trong sáng:** Không còn bất kỳ file nào vượt quá 150 dòng code.
2. **Dễ kiểm thử (Testability):** Có thể viết Unit Test cho từng analyzer/resolver mà không cần dựng DbContext hay MessageBus.
3. **Mở rộng dễ dàng:** Khi cần thêm node `Branch (If/Else)`, chỉ cần viết thêm `BranchExecutor` kế thừa `IDagNodeExecutor` mà không sửa vào core engine.
4. **Độ tin cậy cao:** Loại bỏ hoàn toàn các lỗi ngầm về parsing kiểu dữ liệu, matching khoảng trắng hay treo chu trình.
