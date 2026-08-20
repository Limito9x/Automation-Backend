# Plan Refactor: Pipeline Execution — Agent Selection (MVP)

## 1. Bối cảnh & Vấn đề

Kiến trúc hiện tại (`pipeline_consumer.py` + `StageTaskMessage`) publish task vào **1 queue RabbitMQ chung** (`stage_tasks`), không phân biệt Agent nào sẽ nhận và xử lý. Đồng thời, module `Workspace` đã có sẵn khái niệm `Agent`/`WorkspaceAgent`/`ResourceVersionLocation` nhưng **hoàn toàn tách rời** khỏi luồng thực thi Pipeline — không có cầu nối nào giữa 2 module này.

**Hệ quả nếu không sửa:** nếu có từ 2 Agent (2 máy) trở lên cùng lắng nghe queue chung, RabbitMQ có thể giao việc cho Agent **không có file cần thiết trên đĩa** → Worker crash vì không tìm thấy file.

**Ràng buộc MVP đã thống nhất** (đơn giản hóa có chủ đích):

- 1 `PipelineExecution` chạy trọn vẹn trên **đúng 1 Agent** — không có việc chia nhỏ Node ra nhiều Agent khác nhau trong cùng 1 lần chạy.
- 1 Pipeline có thể chạm tới **nhiều Workspace khác nhau** (VD: đọc từ Workspace "Daz Library", ghi ra Workspace "UE5 Content") — vì vậy **không gắn `Pipeline` cứng vào 1 Workspace**, mà chọn **Agent** làm điểm neo, vì 1 Agent (1 máy vật lý) có thể có `WorkspaceAgent` phủ nhiều Workspace cùng lúc.
- Chưa cần Agent Scope (Personal/Shared/Project) — hệ thống hiện chỉ phục vụ 1 user, phân quyền Agent theo Project/Team chưa có nhu cầu thật. Để dành cho phase sau khi mở multi-user.
- Chưa cần tự động routing theo Resource Location (Tool "Resolve Location" tự chọn Agent) — **user tự chọn Agent tay** lúc Trigger, hệ thống chỉ **validate** và báo lỗi rõ ràng nếu thiếu.

---

## 2. Mục tiêu Refactor

1. `PipelineExecution` biết chính xác sẽ chạy trên **Agent nào**.
2. RabbitMQ route đúng task tới **queue riêng của Agent đó** — không dùng 1 queue chung nữa.
3. Trước khi cho phép Run, hệ thống **validate** Agent đã chọn có đủ `WorkspaceAgent` cho **mọi Workspace** mà Pipeline cần đọc/ghi — nếu thiếu, chặn lại và báo lỗi rõ ràng.
4. `stage_runner.py`/`pipeline_consumer.py` (phía Worker) cần biết `AgentId` của chính nó để lắng nghe đúng queue.

---

## 3. Thay đổi Domain Model

### 3.1. `PipelineExecution` — thêm `AgentId`

```csharp
public class PipelineExecution : BaseEntity<Guid>
{
    public Guid PipelineId { get; private set; }
    public Guid AgentId { get; private set; }          // MỚI — bắt buộc, chọn lúc Trigger
    public ExecutionStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }

    public PipelineExecution(Guid pipelineId, Guid agentId)
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        AgentId = agentId;
        Status = ExecutionStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
```

> **Không thêm `WorkspaceId` vào `Pipeline`.** Pipeline giữ nguyên chỉ biết `ProjectId` — Workspace nào bị chạm tới được suy ra động từ các Resource Pin của từng Node khi validate, không cố định ở cấp Pipeline.

### 3.2. Agent — giữ tối giản, KHÔNG thêm Scope

```csharp
public class Agent : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Offline"; // Online/Offline, cập nhật qua heartbeat (nếu có)
    // KHÔNG thêm Scope, KHÔNG thêm ProjectId — Agent vẫn là entity Global thuần túy
}
```

> Quyết định rõ: **không refactor Agent thành multi-scope** (Personal/Shared/Project) ở giai đoạn này. Đây là tính năng chỉ có giá trị khi hệ thống phục vụ nhiều user/team — chưa phải nhu cầu MVP hiện tại. Có thể bổ sung sau bằng migration nhẹ (`ALTER TABLE`) mà không phá vỡ gì đã có.

---

## 4. Contract mới — Validate Agent Coverage

### 4.1. Mục đích

Trước khi cho phép `Run Pipeline`, kiểm tra: Agent được chọn có `WorkspaceAgent` cho **mọi Workspace** mà các Node trong Pipeline cần đọc/ghi Resource hay không.

### 4.2. Interface (khai báo trong module `Pipeline`, implement ở module `Workspace`)

```csharp
namespace Automation.Pipeline.Application.Contracts;

public interface IWorkspaceAgentCoverageContract
{
    /// <summary>
    /// Trả về danh sách WorkspaceId mà agentId CHƯA có WorkspaceAgent tương ứng,
    /// trong số các workspaceIds được truyền vào.
    /// Danh sách rỗng nghĩa là Agent phủ đủ toàn bộ.
    /// </summary>
    Task<List<Guid>> GetUncoveredWorkspaces(
        Guid agentId,
        List<Guid> requiredWorkspaceIds,
        CancellationToken ct);
}
```

### 4.3. Logic suy ra `requiredWorkspaceIds` từ Pipeline

```csharp
public async Task<List<Guid>> GetWorkspacesTouchedByPipeline(Guid pipelineId, CancellationToken ct)
{
    var pipeline = await LoadPipelineWithNodesAndEdges(pipelineId);
    var workspaceIds = new HashSet<Guid>();

    foreach (var node in pipeline.Nodes)
    {
        var (inputs, outputs) = await _pinResolver.ResolvePins(node);

        // Chỉ quan tâm Pin kiểu EntityRef trỏ tới Resource/ResourceVersion
        foreach (var pin in inputs.Concat(outputs))
        {
            if (pin.PrimitiveType == PinPrimitiveType.EntityRef &&
                pin.EntityType is "ResourceVersion" or "Resource")
            {
                var resolvedValue = ResolveNodeValue(node, pin); // giá trị user đã điền/nối dây
                if (resolvedValue is Guid resourceVersionId)
                {
                    var workspaceId = await _resourceQuery.GetWorkspaceId(resourceVersionId, ct);
                    workspaceIds.Add(workspaceId);
                }
            }
        }
    }

    return workspaceIds.ToList();
}
```

### 4.4. Validate trước khi Run

```csharp
public async Task<Result> ValidateBeforeRun(Guid pipelineId, Guid agentId, CancellationToken ct)
{
    var requiredWorkspaces = await GetWorkspacesTouchedByPipeline(pipelineId, ct);
    var uncovered = await _workspaceAgentCoverage.GetUncoveredWorkspaces(agentId, requiredWorkspaces, ct);

    if (uncovered.Any())
    {
        var names = await GetWorkspaceNames(uncovered, ct);
        return Result.Fail(
            $"Agent chưa được gắn vào Workspace: {string.Join(", ", names)}. " +
            $"Vui lòng thêm WorkspaceAgent cho Agent này trước khi chạy Pipeline.");
    }

    return Result.Ok();
}
```

---

## 5. RabbitMQ — Routing theo Agent

### 5.1. Thay đổi queue naming — từ 1 queue chung sang queue riêng theo Agent

**Trước:**

```python
QUEUE_TASKS = "stage_tasks"   # DÙNG CHUNG cho mọi Worker, không phân biệt máy nào
```

**Sau:**

```python
QUEUE_TASKS_TEMPLATE = "stage_tasks.{agent_id}"
```

### 5.2. `pipeline_consumer.py` — Worker cần biết `AgentId` của chính nó

```python
class PipelineConsumer:
    def __init__(self, agent_id: str, host: str = None, port: int = None):
        self.agent_id = agent_id
        self.queue_name = f"stage_tasks.{agent_id}"
        # ... phần connection giữ nguyên

        self._channel.queue_declare(queue=self.queue_name, durable=True)
        self._channel.queue_declare(queue=QUEUE_RESULTS, durable=True)   # kết quả vẫn gửi về 1 queue chung
        self._channel.queue_declare(queue=QUEUE_PROGRESS, durable=True)
        self._channel.basic_qos(prefetch_count=1)

    def start(self):
        logger.info(f"[*] Agent '{self.agent_id}' listening on '{self.queue_name}'.")
        self._channel.basic_consume(
            queue=self.queue_name,
            on_message_callback=self._on_message,
        )
        self._channel.start_consuming()
```

> `AgentId` được truyền vào lúc khởi động Worker (config file, biến môi trường, hoặc CLI argument) — mỗi máy chạy Worker cần cấu hình đúng `AgentId` tương ứng với record `Agent` đã đăng ký trong DB.

### 5.3. `.NET` — publish với routing key đúng Agent

```csharp
public async Task PublishStageTask(StageTaskMessage message, Guid agentId, CancellationToken ct)
{
    var queueName = $"stage_tasks.{agentId}";
    await _bus.PublishAsync(message, routingKey: queueName, ct);
}
```

`QUEUE_RESULTS`/`QUEUE_PROGRESS` **giữ nguyên dùng chung 1 queue** — vì `.NET` chỉ có 1 nơi lắng nghe kết quả, không cần phân biệt theo Agent ở chiều ngược lại.

---

## 6. Flow tổng — từ lúc User bấm Run tới lúc Worker nhận việc

```text
1. User mở màn hình Trigger Pipeline
   → Dropdown "Chọn Agent" liệt kê các Agent đang Online

2. User chọn Agent (VD "Alice-PC") → bấm Run

3. .NET gọi ValidateBeforeRun(pipelineId, agentId)
   → Suy requiredWorkspaces từ mọi Node có Pin kiểu ResourceVersion
   → Gọi IWorkspaceAgentCoverageContract kiểm tra Agent có đủ WorkspaceAgent không

   NẾU THIẾU:
     → Trả lỗi 422, Frontend hiện: "Agent 'Alice-PC' chưa gắn vào Workspace 'UE5 Content'.
        Vui lòng thêm WorkspaceAgent trước."
     → DỪNG, không tạo PipelineExecution

   NẾU ĐỦ:
     → Tạo PipelineExecution { PipelineId, AgentId, Status: Pending }

4. .NET chạy Topological Sort (Kahn's Algorithm), Execute tuần tự:
   → Tool Node: chạy NGAY trong .NET
   → Session Node: publish StageTaskMessage tới queue "stage_tasks.{agentId}"

5. Worker (đang chạy trên đúng Alice-PC, đã config AgentId="alice-pc-id")
   → Nhận task từ ĐÚNG queue của mình
   → Resolve physical path bằng WorkspaceAgent.RootPath + Resource.RelativePath
     (WorkspaceAgent này CHẮC CHẮN tồn tại vì đã validate ở bước 3)
   → Chạy Blender, xử lý, gửi kết quả về QUEUE_RESULTS chung

6. .NET nhận kết quả, tiếp tục Node tiếp theo, cho tới khi Pipeline hoàn thành.
```

---

## 7. Việc CHƯA làm trong lần refactor này (để dành phase sau)

- **Agent Capability Matching** (kiểu GitHub Actions Label) — gợi ý tự động Agent phù hợp thay vì bắt user chọn tay. Chỉ cần khi có nhiều Agent cùng khả năng, MVP hiện tại giả định số lượng Agent ít, user tự chọn được.
- **Agent Scope** (Personal/Shared/Project) — chỉ cần khi mở multi-user/multi-team.
- **Multi-Agent trong 1 Pipeline Execution** (chia Node ra nhiều Agent khác nhau chạy song song) — vượt quá phạm vi MVP, giữ ràng buộc "1 Execution = 1 Agent".
- **Tool "Resolve Resource Location" tự động** — không cần vì đã chuyển sang cơ chế validate + user chọn tay.
- **Heartbeat / Agent Online-Offline tracking chi tiết** — MVP có thể tạm dùng trạng thái tĩnh hoặc kiểm tra đơn giản (thời gian consumer connect gần nhất), không cần cơ chế phức tạp.

---

## 8. Checklist Migration

### Phase 1 — Domain

- [ ] Thêm `AgentId` (bắt buộc, FK) vào `PipelineExecution`.
- [ ] Không thêm field nào mới vào `Pipeline` (giữ nguyên, không gắn `WorkspaceId`).
- [ ] Không thêm `Scope`/`ProjectId` vào `Agent` — giữ nguyên tối giản.
- [ ] Migration EF Core cho `PipelineExecution.AgentId`.

### Phase 2 — Contract & Validation

- [ ] Khai báo `IWorkspaceAgentCoverageContract` trong module `Pipeline` (`Application/Contracts/`).
- [ ] Implement contract này trong module `Workspace`, đăng ký DI ở composition root.
- [ ] Viết `GetWorkspacesTouchedByPipeline()` — duyệt Node, lọc Pin kiểu `EntityRef` liên quan Resource.
- [ ] Viết `ValidateBeforeRun()` — gọi trước khi tạo `PipelineExecution`, trả lỗi rõ ràng nếu thiếu coverage.
- [ ] Endpoint Trigger Pipeline gọi `ValidateBeforeRun()` trước khi cho phép tạo Execution.

### Phase 3 — RabbitMQ Routing

- [ ] Đổi `QUEUE_TASKS` từ string cố định sang template `stage_tasks.{agent_id}`.
- [ ] Sửa `PipelineConsumer.__init__` nhận `agent_id`, declare đúng queue riêng.
- [ ] Sửa `.NET` publish dùng routing key `stage_tasks.{agentId}` thay vì queue cố định.
- [ ] Cấu hình mỗi máy chạy Worker với đúng `AgentId` tương ứng (biến môi trường hoặc config file).
- [ ] `QUEUE_RESULTS`/`QUEUE_PROGRESS` giữ nguyên không đổi.

### Phase 4 — Frontend

- [ ] Màn hình Trigger Pipeline: thêm dropdown chọn Agent.
- [ ] Hiển thị lỗi rõ ràng khi Validate Coverage thất bại (tên Workspace còn thiếu).
- [ ] (Optional) Màn hình quản lý Agent hiển thị danh sách WorkspaceAgent đã gắn, để user tự thêm khi thiếu.

### Phase 5 — Cleanup & Test

- [ ] Xóa/note rõ code `legacy_params` cũ trong `stage_runner.py` nếu còn tồn tại song song (không thuộc phạm vi plan này nhưng nên dọn cùng đợt nếu tiện).
- [ ] Test case: Agent thiếu 1 WorkspaceAgent → Validate chặn đúng, thông báo rõ ràng.
- [ ] Test case: Pipeline chạm 2 Workspace khác nhau, Agent có đủ cả 2 → chạy thành công end-to-end.
- [ ] Test case: 2 Agent cùng online, publish đúng route tới Agent đã chọn, Agent còn lại không nhận nhầm task.

---

## 9. Tóm tắt quyết định cốt lõi

| Quyết định                                                     | Lý do                                                                                                             |
| -------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| `PipelineExecution.AgentId`, không phải `Pipeline.WorkspaceId` | 1 Pipeline có thể đọc/ghi nhiều Workspace khác nhau; Agent là điểm neo hợp lý vì 1 máy có thể phủ nhiều Workspace |
| User tự chọn Agent tay, hệ thống chỉ validate                  | Đơn giản hơn tự động routing; đủ dùng khi số lượng Agent còn ít                                                   |
| Không thêm Agent Scope                                         | Chưa có nhu cầu multi-user thật; để dành migration nhẹ khi cần                                                    |
| RabbitMQ: 1 queue riêng mỗi Agent                              | Đảm bảo task luôn tới đúng máy có file, tránh crash do sai Agent                                                  |
| Giữ nguyên ràng buộc "1 Execution = 1 Agent"                   | Đúng thực tế sử dụng hiện tại (chưa từng cần multi-agent trong 1 lần chạy)                                        |
