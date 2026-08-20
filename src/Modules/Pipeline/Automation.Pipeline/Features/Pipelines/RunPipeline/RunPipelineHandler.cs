using System.Text.Json;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Engine;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Workspace.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.RunPipeline;

[NonTransactional]
public class RunPipelineHandler(
    PipelineDbContext db,
    IMessageBus messageBus,
    IWorkspaceApi workspaceApi
)
{
    public async Task<Result<PipelineExecutionDto>> HandleAsync(
        RunPipelineCommand command,
        CancellationToken ct
    )
    {
        var pipeline = await db.Pipelines
            .Include(x => x.Nodes)
            .Include(x => x.Inputs)
            .FirstOrDefaultAsync(x => x.Id == command.PipelineId, ct);

        if (pipeline == null)
        {
            return Result.Fail<PipelineExecutionDto>($"Pipeline '{command.PipelineId}' not found.");
        }

        if (command.AgentId == Guid.Empty)
        {
            return Result.Fail<PipelineExecutionDto>("A valid AgentId must be specified to run the pipeline.");
        }

        // Validate required Start Inputs
        var requiredMissing = pipeline.Inputs
            .Where(i => i.IsRequired && i.DefaultValue == null)
            .Where(i => command.RuntimeInputs == null ||
                        (!command.RuntimeInputs.ContainsKey(i.Key) && !command.RuntimeInputs.ContainsKey(i.Label)))
            .ToList();

        if (requiredMissing.Count > 0)
        {
            var missingLabels = string.Join(", ", requiredMissing.Select(i => $"'{i.Label}' ({i.Key})"));
            return Result.Fail<PipelineExecutionDto>($"Missing required pipeline start input(s): {missingLabels}.");
        }

        // 1. Collect required workspaces from pipeline nodes (e.g. SyncLocalChange or Workspace pins)
        var requiredWorkspaceIds = new HashSet<Guid>();
        foreach (var node in pipeline.Nodes)
        {
            if (node.Config != null)
            {
                try
                {
                    var root = node.Config.RootElement;
                    if (root.TryGetProperty("WorkspaceId", out var wElem))
                    {
                        var str = wElem.GetString();
                        if (Guid.TryParse(str, out var wGuid) && wGuid != Guid.Empty)
                        {
                            requiredWorkspaceIds.Add(wGuid);
                        }
                    }
                }
                catch
                {
                    // Ignore JSON parse errors for non-object configs
                }
            }
        }

        // 2. Validate Agent Coverage for required workspaces
        if (requiredWorkspaceIds.Count > 0)
        {
            var uncoveredResult = await workspaceApi.GetUncoveredWorkspacesAsync(command.AgentId, requiredWorkspaceIds, ct);
            if (uncoveredResult.IsFailed)
            {
                return Result.Fail<PipelineExecutionDto>(uncoveredResult.Errors);
            }

            var uncovered = uncoveredResult.Value;
            if (uncovered.Count > 0)
            {
                var namesResult = await workspaceApi.GetWorkspaceNamesAsync(uncovered, ct);
                var names = namesResult.IsSuccess && namesResult.Value.Count > 0
                    ? string.Join(", ", namesResult.Value.Values.Select(n => $"'{n}'"))
                    : string.Join(", ", uncovered);

                return Result.Fail<PipelineExecutionDto>(
                    $"Selected Agent is not assigned to required workspace(s): {names}. Please add WorkspaceAgent before running this pipeline."
                );
            }
        }

        // 3. Create Execution record and pre-save initial RuntimeInputs in ExecutionState
        var execution = new PipelineExecution(pipeline.Id, command.AgentId);

        var initialState = new Automation.Pipeline.Engine.Models.PipelineExecutionState();
        if (command.RuntimeInputs != null)
        {
            foreach (var (k, v) in command.RuntimeInputs)
            {
                initialState.RuntimeInputs[k] = v;
            }
        }
        execution.SetState(initialState.ToJsonDocument(), 0);

        db.PipelineExecutions.Add(execution);
        await db.SaveChangesAsync(ct);

        // 4. Trigger Execution Engine asynchronously (Fire-and-forget via Wolverine)
        await messageBus.PublishAsync(new TriggerPipelineExecutionMessage(execution.Id));

        var dto = new PipelineExecutionDto(
            execution.Id,
            execution.PipelineId,
            execution.AgentId,
            execution.Status,
            execution.StartedAt,
            execution.FinishedAt,
            execution.ErrorMessage,
            execution.NextNodeIndex,
            execution.CurrentBatchId,
            execution.ExecutionState
        );

        return Result.Ok(dto);
    }
}
