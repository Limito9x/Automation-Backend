using System.Text.Json;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.UpdatePipelineNode;

[Transactional(typeof(PipelineDbContext))]
public class UpdatePipelineNodeHandler(PipelineDbContext db)
{
    public async Task<Result> HandleAsync(
        UpdatePipelineNodeCommand command,
        CancellationToken ct
    )
    {
        var node = await db.PipelineNodes
            .FirstOrDefaultAsync(x => x.Id == command.NodeId && x.PipelineId == command.PipelineId, ct);

        if (node == null)
        {
            return Result.Fail($"Node '{command.NodeId}' not found in Pipeline '{command.PipelineId}'.");
        }

        if (command.PositionX.HasValue && command.PositionY.HasValue)
        {
            node.Update(command.PositionX.Value, command.PositionY.Value);
        }

        if (command.ConfigValues != null)
        {
            JsonDocument? configDoc = command.ConfigValues.Count > 0
                ? JsonDocument.Parse(JsonSerializer.Serialize(command.ConfigValues))
                : null;

            node.UpdateConfig(configDoc);
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
