# Pipeline Execution & Agent Boundary Refactor Plan

## 1. Mục tiêu

Refactor pipeline theo các nguyên tắc:

- Tách rõ **Control Flow** và **Data Flow**.
- Pipeline có **Start Node** làm entry point.
- Không còn runtime scan input thiếu rồi popup yêu cầu người dùng bổ sung.
- External/runtime inputs phải được khai báo tại Start Node.
- `PipelineNode` chỉ là graph instance; `inputs`/`outputs` đến từ Node Definition/Tool definition.
- Execution state được lưu tập trung, không truyền toàn bộ context qua từng message.
- Redis là **shared execution state store**, nhưng Worker **không truy cập Redis trực tiếp**.
- Backend sở hữu Redis và execution state.
- Agent là boundary của machine, filesystem, process và worker.
- Worker chỉ execute node, nhận input và trả output.
- MVP: một pipeline chạy trên một Agent.

## 2. Kiến trúc đích

```text
                         BACKEND
              ┌──────────────────────────┐
              │ Pipeline Executor         │
              │ Dependency Resolver       │
              │ Authorization             │
              │ Redis / Execution State   │
              └────────────┬─────────────┘
                           │
                         gRPC
                           │
                    ┌──────▼──────┐
                    │    AGENT    │
                    │ Worker Mgmt │
                    │ Filesystem  │
                    │ Processes   │
                    └──────┬──────┘
                           │
                    ┌──────▼──────┐
                    │   WORKER    │
                    │ Blender/Py  │
                    │ UE/...      │
                    └─────────────┘
```

### Boundary

**Backend**
- Pipeline definition/execution.
- Dependency resolution.
- Execution state.
- Redis.
- Authorization.
- Scheduling/dispatch.

**Agent**
- Machine identity.
- WorkspaceAgent/root path.
- Filesystem.
- Process lifecycle.
- Worker lifecycle.
- gRPC connection.

**Worker**
- Execute node.
- Nhận input.
- Chạy tool/script.
- Trả output/status/error.
- Không biết Redis/database/authorization implementation.

---

## 3. Pipeline Graph

Pipeline có hai loại dependency:

```text
Control Flow
A ─────exec────> B ─────exec────> C

Data Flow
A.output ─────data────> C.input
```

### Control Flow

Quyết định node nào được phép chạy tiếp:

```text
Start
  ↓
Import Diffeomorphic
  ↓
Generate UV
  ↓
Texture Bake
  ↓
Save Blend
  ↓
Sync Local Change
```

Không cần lưu `exec input` và `exec output` thành hai field riêng trên `PipelineNode`. Chúng là pins thuộc definition và edges thuộc graph.

### Data Flow

Quyết định node đang chạy lấy value nào:

```text
Get Inspection
      ↓
Get Tag Value
      ↓
targetObjects ─────> Generate UV
```

Pure/Data node có thể không có execution flow. Action node có execution flow. Contract của node quyết định pin, không phải UI tự suy đoán.

---

## 4. Start Node

Thay thế hoàn toàn cơ chế:

```text
run → thiếu input → scan → popup → user bổ sung
```

bằng:

```text
             Start
          /    |           input input input
          \    |    /
             ↓
          Pipeline
```

Ví dụ:

```text
Start
 ├── sourceResourceVersion
 ├── targetObjects
 ├── outputWorkspace
 └── textureResolution
```

Nếu value chỉ xác định được runtime:

```text
Start
   ↓
Get Inspection
   ↓
Get Tag Value
   ↓
Generate UV
```

Nếu cần external/runtime parameter thì Start Node phải khai báo parameter đó.

---

## 5. Literal / Static Value

Không nên để Action Node tự chứa nhiều field value đặc biệt:

```text
Generate UV
    ratio = 0.5
```

Nên biểu diễn:

```text
[Static Value: 0.5]
          │
          ▼
    [Generate UV]
```

UI có thể render literal inline cạnh pin để UX tiện dụng, nhưng về execution model nó vẫn là một data provider.

---

## 6. Node Definition và PipelineNode

### Node Definition

Là contract của node:

```text
Generate UV

Inputs:
    targetObjects : Object[] [required]
    resolution    : Int      [required]

Outputs:
    uvName        : String
```

Nó chứa:
- Input pins.
- Output pins.
- Control pins.
- Data pins.
- Primitive types.
- Cardinality.
- Kind/execution metadata.
- Reference tới implementation/script nếu cần.

### PipelineNode

Chỉ đại diện instance trong graph:

```text
PipelineNode
    id
    kind
    refId
    position
    metadata/runtime configuration
```

Không duplicate input/output definition.

Khi load graph:

```text
PipelineNode
      ↓
kind + refId
      ↓
Node Definition / Tool
      ↓
Resolve Pins
      ↓
UI Graph / Validation / Execution
```

Một resolver chung có thể phục vụ:
1. UI render.
2. Graph validation.
3. Execution input/output resolution.
4. Compile execution dependency map.

---

## 7. Dependency Resolver

Resolver giải:

```text
Node A output X
       ↓
Node B input Y
```

thành:

```text
B.Y = A.X
```

Nó cần tạo được execution dependency map và phân biệt:
- Control dependency.
- Data dependency.
- Required input.
- Cardinality/type compatibility.

---

## 8. Execution State và Redis

Execution state thuộc về một `PipelineExecution`.

Conceptual state:

```text
PipelineExecution:{executionId}

Node A
    status = completed
    outputs = ...

Node B
    status = running

Node C
    status = pending
```

Redis có thể lưu các key kiểu:

```text
pipeline:execution:{executionId}:node:{nodeId}:output
pipeline:execution:{executionId}:node:{nodeId}:status
```

Redis chứa:
- Primitive output.
- JSON structured output.
- Metadata.
- Resource/ResourceVersion references.

**Không lưu binary asset lớn trực tiếp trong Redis.**

---

## 9. Redis Boundary

Nguyên tắc:

> Redis là shared execution state của hệ thống, không phải shared database cho Worker.

Đúng:

```text
Backend → Redis
```

Sai:

```text
Worker → Redis
```

Nếu Worker cần state:

```text
Worker
   ↓
Agent
   ↓ gRPC
Backend
   ↓
Execution State Resolver
   ↓
Redis
   ↓
Backend
   ↓ gRPC
Agent
   ↓
Worker
```

Worker chỉ biết abstraction kiểu:

```text
GetExecutionValue(...)
```

chứ không biết Redis tồn tại.

### Lý do

Worker có thể chạy user-defined script. Không nên cấp Redis credential cho process có khả năng chạy code của người dùng:

```text
User Script
   ↓
Worker
   ↓
Redis
   ↓
execution/workspace ngoài scope
```

Thay bằng:

```text
User Script
   ↓
Worker
   ↓
Agent
   ↓
Backend authorization
   ↓
Execution State
```

---

## 10. Agent ↔ Backend

Agent chủ động giữ gRPC connection tới Backend.

Backend registry:

```text
AgentId → connection
```

Backend cần gọi Agent:

```text
AgentRegistry.Get(agentId)
       ↓
GrpcConnection
       ↓
Send command
```

Điều này tránh phụ thuộc vào inbound connection từ Backend tới máy user và phù hợp với NAT/firewall/dynamic IP.

Agent restart phải reconnect và đăng ký lại.

---

## 11. Worker Execution

Luồng cơ bản:

```text
Backend
   ↓
Execution Engine
   ↓
resolve inputs
   ↓
Agent
   ↓
Worker
   ↓
execute
   ↓
Output / Status / Error
   ↓
Agent
   ↓
Backend
   ↓
Execution State
```

Sau khi Node A hoàn thành:

```text
Node A completed
      ↓
persist output
      ↓
resolve Node B inputs
      ↓
dispatch Node B
```

Không truyền toàn bộ `all_step_outputs` qua từng message.

---

## 12. Không batch cứng theo platform/worker

Không nên:

```text
Blender A → B → C
        gom thành payload

UE D → E
    gom thành payload
```

vì crossing boundary giữa Blender/UE sẽ làm context khó quản lý.

Nên:

```text
A
 ↓
state
 ↓
B
 ↓
state
 ↓
C
 ↓
state
 ↓
D
```

Mỗi node có execution boundary rõ ràng.

Nếu sau này cần batching để tối ưu thì đó là optimization của executor, không phải thay đổi graph contract.

---

## 13. Large Data và Execution State

Hai loại state phải tách.

### Execution state

```text
Redis
 ├── primitive
 ├── JSON
 ├── metadata
 └── references
```

### Asset

```text
WorkspaceAgent
      ↓
Filesystem / Storage
      ↓
.daz / .blend / .fbx / texture...
```

Node output lớn nên trả reference:

```json
{
  "resourceVersionId": "...",
  "workspaceAgentId": "...",
  "relativePath": "Eva/Eva.blend"
}
```

thay vì nhét binary vào execution message/Redis.

---

## 14. Tool / Node Definition

### Pure/Data Tool

Ví dụ:
- Get Inspection.
- Get Tag Value.
- String Append.

Có thể chỉ cung cấp data.

### Action

Ví dụ:
- Import Diffeomorphic.
- Generate UV.
- Texture Bake.
- Save Blend.
- Sync Local Change.

Có execution/control flow.

Không nên dùng `kind` để ép mọi tool phải giống nhau. Contract của definition quyết định input/output/control pins.

---

## 15. Pipeline Compile / Validation

Trước execution:

```text
Pipeline Definition
       ↓
Resolve Node Definitions
       ↓
Resolve Pins
       ↓
Validate Graph
       ↓
Validate Start Inputs
       ↓
Build Execution Plan
       ↓
Run
```

Phải phát hiện trước:
- Thiếu Start input.
- Required input chưa có source.
- Sai primitive type.
- Sai cardinality.
- Data edge không hợp lệ.
- Control edge không hợp lệ.
- Node reference không tồn tại.
- Circular dependency nếu không hỗ trợ.
- Node không có execution contract hợp lệ.

Không đợi runtime mới popup.

---

## 16. Execution Algorithm MVP

```text
1. Load Pipeline
2. Resolve definitions
3. Validate graph
4. Create PipelineExecution
5. Initialize Start Node
6. Find ready nodes
7. Execute node
8. Persist outputs/status
9. Mark node completed
10. Re-evaluate dependent nodes
11. Execute next ready nodes
12. Continue until finished
13. Mark execution completed/failed
```

Một node chỉ ready khi:

```text
control_dependencies_completed
AND
required_inputs_resolved
```

---

# 17. Refactor Order

## Phase 1 — Contract

- [ ] Chốt Control Flow.
- [ ] Chốt Data Flow.
- [ ] Chốt Start Node.
- [ ] Chốt Primitive Type.
- [ ] Chốt Cardinality.
- [ ] Chốt Input/Output/Control Pin.
- [ ] Chốt Action vs Pure/Data.
- [ ] Bỏ runtime missing-input popup.

## Phase 2 — Node Definition

- [ ] Đưa pin definition về Node Definition/Tool.
- [ ] Xóa duplicated input/output khỏi `PipelineNode`.
- [ ] Chuẩn hóa `kind + refId`.
- [ ] Hỗ trợ Static Value Node.
- [ ] Xem xét variadic/dynamic pins.

## Phase 3 — Graph Resolver

- [ ] Resolve `PipelineNode → Definition`.
- [ ] Resolve Data Edges.
- [ ] Resolve Control Edges.
- [ ] Validate types/cardinality.
- [ ] Build dependency map.
- [ ] Dùng chung resolver cho UI + validation + execution.

## Phase 4 — Start

- [ ] Tạo Start Node.
- [ ] Khai báo Start Inputs.
- [ ] Pipeline execution nhận Start Input payload.
- [ ] Validate trước khi run.
- [ ] Xóa flow popup thiếu input.

## Phase 5 — Execution State

- [ ] Tạo `PipelineExecution`.
- [ ] Tạo Node Execution state.
- [ ] Tạo output reference.
- [ ] Tạo Redis state abstraction.
- [ ] Persist status/output.
- [ ] Không phụ thuộc Dictionary RAM cho execution state.

## Phase 6 — Backend ↔ Agent

- [ ] Chuẩn hóa gRPC connection.
- [ ] `AgentRegistry: AgentId → connection`.
- [ ] Reconnect khi Agent restart.
- [ ] Dispatch command qua connection.
- [ ] Agent report result/status.

## Phase 7 — Agent ↔ Worker

- [ ] Worker manager.
- [ ] Worker execution contract.
- [ ] Input/output message.
- [ ] Status/error message.
- [ ] Worker không có Redis credential.

## Phase 8 — Worker State Access

- [ ] Backend contract `GetExecutionValue`.
- [ ] Agent bridge request.
- [ ] Authorization theo Execution/Node.
- [ ] Worker chỉ gọi abstraction cần thiết.
- [ ] Không cho Worker trực tiếp connect Redis.

## Phase 9 — Asset Boundary

- [ ] Redis chỉ chứa execution state/reference.
- [ ] Binary asset nằm tại WorkspaceAgent/filesystem/storage.
- [ ] Chuẩn hóa ResourceVersion location.
- [ ] Output lớn trả reference.

## Phase 10 — End-to-end MVP

Test pipeline:

```text
Start
  ↓
Get Inspection
  ↓
Get Tag Value
  ↓
Import Diffeomorphic
  ↓
Generate UV
  ↓
Texture Bake
  ↓
Save Blend
  ↓
Sync Local Change
```

Kiểm tra:
- [ ] Start input.
- [ ] Pure/data node.
- [ ] Action node.
- [ ] Data dependency.
- [ ] Control dependency.
- [ ] Redis output state.
- [ ] Backend → Agent dispatch.
- [ ] Agent → Worker execution.
- [ ] Worker output.
- [ ] Backend state update.
- [ ] Workspace sync.

---

# 18. Các nguyên tắc cần giữ sau refactor

1. **Pin là contract** — definition quyết định pin.
2. **Start là boundary của external input.**
3. **Control Flow quyết định execution.**
4. **Data Flow quyết định value dependency.**
5. **Redis là execution state, không phải asset storage.**
6. **Backend sở hữu Redis.**
7. **Agent sở hữu machine/filesystem/process/worker.**
8. **Worker chỉ execute.**
9. **Không truyền toàn bộ context qua từng message.**
10. **Không runtime popup để sửa graph.**
11. **MVP chạy một Pipeline trên một Agent.**
12. **Multi-agent execution để sau, không làm phức tạp MVP.**

## 19. Kết quả mong muốn

```text
                 START
                   │
             external inputs
                   │
                   ▼
          ┌─────────────────┐
          │    Pipeline     │
          │                 │
          │ Control Flow    │
          │ Data Flow       │
          └────────┬────────┘
                   │
             Execution Engine
                   │
             ┌─────▼─────┐
             │   Redis   │
             │   State   │
             └─────┬─────┘
                   │
                  gRPC
                   │
                 Agent
                   │
                 Worker
                   │
              Execute Node
                   │
                 Output
                   │
             Backend State
```

Mục tiêu cuối cùng là giữ boundary rõ ràng: **Pipeline mô tả graph → Executor điều phối → Backend sở hữu state → Agent sở hữu machine → Worker thực thi node → Redis lưu execution state → Workspace/Storage giữ asset**.
