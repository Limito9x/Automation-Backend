namespace Automation.Pipeline.Domain.Enums;

public enum WorkflowNodeKind
{
    EventTrigger = 1,
    ConditionFilter = 2,
    ExecutePipeline = 3,
    SendNotification = 4
}
