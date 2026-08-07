using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class Script : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public WorkerType WorkerType { get; private set; }
    public string ScriptPath { get; private set; } = string.Empty;
    public JsonDocument ParamsConfig { get; private set; } = null!;

    protected Script() { }

    public Script(string name, WorkerType workerType, string scriptPath, JsonDocument paramsConfig)
    {
        Id = Guid.NewGuid();
        Name = name;
        WorkerType = workerType;
        ScriptPath = scriptPath;
        ParamsConfig = paramsConfig;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
