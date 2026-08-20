using System.Text.Json;
using Automation.Pipeline.Engine;
using Automation.Pipeline.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Features.Execution;

public class ExecutionStateGrpcService(
    IExecutionStateStore stateStore,
    ILogger<ExecutionStateGrpcService> logger) : ExecutionStateService.ExecutionStateServiceBase
{
    public override async Task<GetStepInputsResponse> GetStepInputs(
        GetStepInputsRequest request,
        ServerCallContext context)
    {
        var response = new GetStepInputsResponse
        {
            Success = true
        };

        if (!Guid.TryParse(request.PipelineExecutionId, out var pipelineExecId))
        {
            return new GetStepInputsResponse
            {
                Success = false,
                ErrorMessage = $"Invalid PipelineExecutionId: {request.PipelineExecutionId}"
            };
        }

        try
        {
            foreach (var mapping in request.InputMappings)
            {
                var pinKey = mapping.PinKey;
                object? resolvedValue = null;

                if (string.Equals(mapping.SourceKind, "start_input", StringComparison.OrdinalIgnoreCase))
                {
                    resolvedValue = await stateStore.GetStartInputAsync(
                        pipelineExecId,
                        mapping.SourcePinKey,
                        context.CancellationToken
                    );
                }
                else if (string.Equals(mapping.SourceKind, "node_output", StringComparison.OrdinalIgnoreCase))
                {
                    if (Guid.TryParse(mapping.SourceNodeId, out var sourceNodeId))
                    {
                        resolvedValue = await stateStore.GetNodeOutputAsync(
                            pipelineExecId,
                            sourceNodeId,
                            mapping.SourcePinKey,
                            context.CancellationToken
                        );
                    }
                    else
                    {
                        logger.LogWarning(
                            "Invalid source_node_id '{SourceNodeId}' for pin '{PinKey}' in execution {ExecId}",
                            mapping.SourceNodeId,
                            pinKey,
                            pipelineExecId
                        );
                    }
                }
                else if (string.Equals(mapping.SourceKind, "literal", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(mapping.LiteralValueJson))
                    {
                        response.InputsJson[pinKey] = mapping.LiteralValueJson;
                        continue;
                    }
                }

                if (resolvedValue != null)
                {
                    response.InputsJson[pinKey] = JsonSerializer.Serialize(resolvedValue);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to resolve step inputs for step {StepId}", request.StepExecutionId);
            return new GetStepInputsResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public override async Task<ReportStepOutputResponse> ReportStepOutput(
        ReportStepOutputRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.PipelineExecutionId, out var pipelineExecId) ||
            !Guid.TryParse(request.StepExecutionId, out var stepNodeId))
        {
            return new ReportStepOutputResponse
            {
                Success = false,
                ErrorMessage = "Invalid PipelineExecutionId or StepExecutionId."
            };
        }

        try
        {
            foreach (var (pinKey, jsonVal) in request.OutputsJson)
            {
                object? parsed = null;
                try
                {
                    parsed = JsonSerializer.Deserialize<JsonElement>(jsonVal);
                }
                catch
                {
                    parsed = jsonVal;
                }

                await stateStore.SetNodeOutputAsync(
                    pipelineExecId,
                    stepNodeId,
                    pinKey,
                    parsed,
                    context.CancellationToken
                );
            }

            return new ReportStepOutputResponse { Success = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to report step output for step {StepId}", request.StepExecutionId);
            return new ReportStepOutputResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
