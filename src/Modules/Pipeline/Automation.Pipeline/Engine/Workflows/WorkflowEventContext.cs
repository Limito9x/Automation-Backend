using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Engine.Workflows;

public class WorkflowEventContext
{
    public WorkflowEventType EventType { get; set; }
    public Guid ProjectId { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid AgentId { get; set; }
    public List<Guid> ResourceVersionIds { get; set; } = new();
    public string? RelativePath { get; set; }
    public string? Extension { get; set; }
    public Guid? PlatformExtensionId { get; set; }
    public JsonDocument? RawPayload { get; set; }

    // Dynamic data produced by previous nodes in the workflow (keyed by WorkflowNode.Id or custom key)
    public Dictionary<string, object?> NodeOutputs { get; set; } = new();
}
