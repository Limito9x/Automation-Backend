using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.CreatePipeline;

[Transactional(typeof(PipelineDbContext))]
public class CreatePipelineHandler(PipelineDbContext db)
{
    public async Task<Result<PipelineSummaryDto>> HandleAsync(
        CreatePipelineCommand command,
        CancellationToken ct
    )
    {
        var exists = await db.Pipelines.AnyAsync(
            x => x.ProjectId == command.ProjectId && x.Name.ToLower() == command.Name.Trim().ToLower(),
            ct
        );

        if (exists)
        {
            return Result.Fail<PipelineSummaryDto>($"A pipeline with name '{command.Name}' already exists in this project.");
        }

        var pipeline = new Domain.Entities.Pipeline(command.ProjectId, command.Name.Trim());
        db.Pipelines.Add(pipeline);

        var startNode = new PipelineNode(
            Guid.NewGuid(),
            pipeline.Id,
            "Start",
            Constants.PipelineNodeKind.Start,
            80,
            150,
            null
        );
        db.PipelineNodes.Add(startNode);
        await db.SaveChangesAsync(ct);

        var dto = new PipelineSummaryDto(
            pipeline.Id,
            pipeline.ProjectId,
            pipeline.Name,
            0,
            0,
            pipeline.CreatedAt
        );

        return Result.Ok(dto);
    }
}
