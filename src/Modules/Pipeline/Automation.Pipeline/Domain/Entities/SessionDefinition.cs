using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class SessionDefinition : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public WorkerType WorkerType { get; private set; }
    public JsonDocument Flow { get; private set; } = null!;

    protected SessionDefinition() { }

    public SessionDefinition(string name, WorkerType workerType, JsonDocument flow)
    {
        Id = Guid.NewGuid();
        Name = name;
        WorkerType = workerType;
        Flow = flow;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

