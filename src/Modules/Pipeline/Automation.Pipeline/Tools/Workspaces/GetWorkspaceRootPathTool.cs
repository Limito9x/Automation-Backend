using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Workspace.Contracts;

namespace Automation.Pipeline.Tools.Workspaces;

public class GetWorkspaceRootPathTool(IWorkspaceApi workspaceApi) : IResolverTool
{
    public string Key => "get_workspace_root_path";
    public string Label => "Get Workspace Root Path";
    public string Category => "Workspaces";

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "WorkspaceId",
            Label = "Workspace",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "RootPath",
            Label = "Root Path",
            PrimitiveType = PinPrimitiveType.Path,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        }
    ];

    public async Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        if (!inputs.TryGetValue("WorkspaceId", out var wsObj) || wsObj == null)
        {
            throw new ArgumentException("WorkspaceId is required.");
        }

        var wsStr = wsObj.ToString();
        if (!Guid.TryParse(wsStr, out var wsId))
        {
            throw new ArgumentException($"Invalid WorkspaceId GUID format: '{wsStr}'");
        }

        var result = await workspaceApi.GetWorkspaceRootPathAsync(wsId, context.AgentId, context.CancellationToken);
        if (result.IsFailed)
        {
            throw new InvalidOperationException($"Failed to get root path for Workspace '{wsId}': {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        return new Dictionary<string, object>
        {
            ["RootPath"] = result.Value
        };
    }
}
