using System.Text.Json;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Features.Workflows.Dtos;

public record WorkflowSummaryDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    bool IsActive,
    int NodesCount,
    int EdgesCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record WorkflowGraphDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    List<WorkflowNodeDto> Nodes,
    List<WorkflowEdgeDto> Edges
);

public record WorkflowNodeDto(
    Guid Id,
    Guid WorkflowId,
    string RefId,
    WorkflowNodeKind Kind,
    NodePosition Position,
    JsonDocument? Config
);

public record WorkflowEdgeDto(
    Guid Id,
    Guid WorkflowId,
    Guid SourceWorkflowNodeId,
    string SourcePin,
    Guid TargetWorkflowNodeId,
    string TargetPin
);

public record WorkflowExecutionDto(
    Guid Id,
    Guid WorkflowId,
    WorkflowEventType TriggerEventType,
    JsonDocument? TriggerPayload,
    ExecutionStatus Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    string? ErrorMessage,
    List<WorkflowNodeExecutionDto>? NodeExecutions = null
);

public record WorkflowNodeExecutionDto(
    Guid Id,
    Guid WorkflowExecutionId,
    Guid WorkflowNodeId,
    ExecutionStatus Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    JsonDocument? Output,
    string? ErrorMessage
);

public record WorkflowNodePaletteItemDto(
    string Kind,
    string Name,
    string Category,
    string Description,
    List<string> InputPins,
    List<string> OutputPins
);
