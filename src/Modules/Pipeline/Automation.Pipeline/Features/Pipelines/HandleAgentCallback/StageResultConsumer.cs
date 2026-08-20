using System.Text.Json;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Engine;
using Automation.Pipeline.Engine.Messages;
using Automation.Pipeline.Engine.Models;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.HandleAgentCallback;

[NonTransactional]
public class StageResultConsumer(
    PipelineDbContext db,
    IPipelineExecutionEngine engine,
    IExecutionStateStore stateStore,
    ILogger<StageResultConsumer> logger
)
{
    public async Task HandleAsync(StageResultMessage message, CancellationToken ct)
    {
        logger.LogInformation("Received StageResultMessage for StageExecutionId: {StageExecutionId}, Succeeded: {Succeeded}",
            message.StageExecutionId, message.Succeeded);

        var execution = await db.PipelineExecutions
            .FirstOrDefaultAsync(x => x.CurrentBatchId == message.StageExecutionId, ct);

        if (execution == null)
        {
            logger.LogWarning("No PipelineExecution found waiting for StageExecutionId: {StageExecutionId}", message.StageExecutionId);
            return;
        }

        if (execution.Status != ExecutionStatus.WaitingForAgent && execution.Status != ExecutionStatus.Running)
        {
            logger.LogWarning("PipelineExecution {ExecutionId} is in status {Status}, skipping callback", execution.Id, execution.Status);
            return;
        }

        if (!message.Succeeded)
        {
            var err = message.ErrorMessage ?? "Agent execution failed with unspecified error.";
            execution.MarkFailed(err);
            await db.SaveChangesAsync(ct);
            logger.LogError("PipelineExecution {ExecutionId} failed during Agent stage {StageId}: {Error}", execution.Id, message.StageExecutionId, err);
            return;
        }

        var state = PipelineExecutionState.FromJsonDocument(execution.ExecutionState);

        foreach (var stepResult in message.StepResults)
        {
            if (Guid.TryParse(stepResult.StepExecutionId, out var nodeId))
            {
                if (stepResult.Outputs != null && stepResult.Outputs.Count > 0)
                {
                    state.SetNodeOutputs(nodeId, stepResult.Outputs);
                    await stateStore.SetNodeOutputsAsync(execution.Id, nodeId, stepResult.Outputs, ct);
                }

                var outputDoc = JsonDocument.Parse(JsonSerializer.Serialize(stepResult.Outputs ?? new()));
                JsonDocument? logDoc = null;
                if (!string.IsNullOrWhiteSpace(stepResult.Log))
                {
                    try { logDoc = JsonDocument.Parse(JsonSerializer.Serialize(stepResult.Log)); } catch { /* ignore */ }
                }

                var nodeExec = new NodeExecution(execution.Id, nodeId, outputDoc);
                if (stepResult.Succeeded)
                {
                    nodeExec.MarkSucceeded(outputDoc, logDoc);
                    await stateStore.SetNodeStatusAsync(execution.Id, nodeId, ExecutionStatus.Succeeded.ToString(), ct);
                }
                else
                {
                    nodeExec.MarkFailed(stepResult.ErrorMessage ?? "Step failed", logDoc);
                    await stateStore.SetNodeStatusAsync(execution.Id, nodeId, ExecutionStatus.Failed.ToString(), ct);
                }
                db.NodeExecutions.Add(nodeExec);
            }
        }

        await stateStore.SaveFullStateAsync(execution.Id, state, ct);
        execution.SetState(state.ToJsonDocument(), execution.NextNodeIndex);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Resuming PipelineExecution {ExecutionId} from node index {NextNodeIndex}", execution.Id, execution.NextNodeIndex);

        // Resume scheduler
        await engine.ExecuteOrResumeAsync(execution.Id, ct: ct);
    }
}
