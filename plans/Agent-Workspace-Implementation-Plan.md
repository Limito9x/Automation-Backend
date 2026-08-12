# Agent & Workspace Implementation Plan

## Mục tiêu

Điều chỉnh architecture hiện tại để làm rõ ba khái niệm:

- **Project**: phạm vi nghiệp vụ lớn.
- **Workspace**: không gian quản lý Resource thuộc một Platform trong Project.
- **Agent**: machine/runtime có khả năng thao tác Resource và thực thi Pipeline.

MVP ưu tiên **Local Workspace + Local Agent**. Remote Workspace dùng R2 được chừa boundary để triển khai sau.

---

## Phase 1 — Resource / Workspace Foundation

### Workspace

Giữ `Resource` là entity đại diện cho tài nguyên thực tế, không đổi tên thành Workspace.

```text
Project
└── Workspace
     └── Resource
```

Workspace cần thể hiện:

- `ProjectId`
- `PlatformId`
- `Name`
- loại Workspace để chừa khả năng mở rộng Local/Remote

MVP chỉ triển khai **Local Workspace**.

### Resource

Resource tiếp tục thuộc Workspace.

Không để Resource phải biết chi tiết implementation của storage.

---

## Phase 2 — Agent Foundation

Tách Agent thành concept/module độc lập khỏi Resource.

### Agent

Agent đại diện cho một machine/runtime có thể thực thi công việc.

MVP cần:

- Identity của Agent
- Registration / pairing với Backend
- Liên kết Agent với Workspace cần thao tác
- Cấu hình Platform trên machine

Chưa triển khai hệ thống permission/access policy phức tạp. Trong MVP, Agent được xem là có quyền sử dụng các Workspace mà nó được cấu hình.

### Agent Platform Configuration

Agent cần biết các Platform nào được cài trên machine và cách gọi chúng.

```text
Agent
└── Platform Configurations
     ├── Blender
     │    ├── Version
     │    └── ExecutablePath
     ├── Daz
     │    └── ExecutablePath
     └── Unreal
          └── ExecutablePath
```

Pipeline không hardcode đường dẫn executable. Agent chịu trách nhiệm resolve Platform thành executable thực tế trên machine.

---

## Phase 3 — Agent Installation & Runtime

Xây dựng cách cài Agent trên máy local.

### Installation Flow

```text
Tauri Desktop
    │
    ├── Agent đã tồn tại
    │       └── sử dụng Agent
    │
    └── Agent chưa tồn tại
            ↓
       User xác nhận cài đặt
            ↓
       Install Agent
            ↓
       Register / Pair
            ↓
       Configure Workspace
            ↓
       Agent Ready
```

Tauri chỉ đóng vai trò Desktop UI / bootstrap installation.

Agent là chương trình runtime độc lập và có thể tiếp tục chạy khi Tauri đóng.

### Agent Runtime

Agent MVP gồm hai phần:

```text
Agent
├── API
└── Worker
```

**API** dùng cho registration và giao tiếp control-plane cơ bản với Backend.

**Worker** nhận Pipeline Job từ RabbitMQ và thực thi công việc trên machine.

Không cần xây dựng scheduler hoặc execution policy phức tạp ở phase này.

---

## Phase 4 — Pipeline Execution

Pipeline cần có execution target rõ ràng.

MVP:

```text
PipelineRun
└── AgentId
```

Flow:

```text
Backend
   ↓
RabbitMQ
   ↓
Agent Worker
   ↓
Resolve Platform Configuration
   ↓
Execute Platform / Script
   ↓
Local Workspace
```

Agent chịu trách nhiệm nhận job, resolve Platform configuration, thao tác filesystem của Local Workspace, chạy process cần thiết và trả kết quả thực thi.

---

## Phase 5 — Remote Workspace

Sau khi Local Workspace và Agent flow ổn định, mở rộng Workspace để hỗ trợ remote storage.

```text
Workspace
├── Local
└── Remote
```

Remote Workspace có thể sử dụng R2 thông qua Files module hiện tại.

R2 không cần trở thành entity riêng trong Resource domain.

```text
Workspace
   │
   └── Resource
          │
          └── Files infrastructure
                 └── R2
```

MVP không cần implement Remote Workspace.

---

## Phase 6 — Agent Scheduling & Policy

Chỉ triển khai khi nhu cầu thực tế xuất hiện.

Có thể mở rộng Agent với:

- capabilities
- visibility
- execution policy
- nhiều Agent cho một Workspace
- scheduler lựa chọn Agent phù hợp

Khi đó Pipeline Run có thể chuyển từ:

```text
PipelineRun
└── AgentId
```

sang:

```text
PipelineRun
└── ExecutionTarget
      └── Agent
```

Scheduler có thể lựa chọn dựa trên Platform, version, capability và trạng thái Agent.

---

# Architecture Target

```text
Project
  │
  └── Workspace
       │
       ├── Resource
       │
       └── Agents
              │
              ├── Agent A
              ├── Agent B
              └── Agent C


Agent
├── Identity
├── Platform Configuration
├── API
└── Worker


Pipeline
  │
  └── PipelineRun
        │
        └── Agent
```

## Nguyên tắc

1. **Resource != Workspace**
2. **Workspace != Storage**
3. **Agent != Storage**
4. Workspace là logical boundary của Resource.
5. Agent là execution endpoint của machine.
6. Local là flow ưu tiên của MVP.
7. R2 là hướng Remote Workspace về sau.
8. Tauri không phải Agent; Tauri chỉ hỗ trợ UI và bootstrap installation.
9. Pipeline Run phải có execution target rõ ràng.
10. Không triển khai permission, scheduler, capability matching phức tạp trong MVP.
