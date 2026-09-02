using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Automation.Pipeline.Hubs;

[Authorize]
public class WorkflowExecutionHub : Hub
{
    public async Task JoinWorkflow(string workflowId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"workflow_{workflowId}");
    }

    public async Task LeaveWorkflow(string workflowId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"workflow_{workflowId}");
    }
}
