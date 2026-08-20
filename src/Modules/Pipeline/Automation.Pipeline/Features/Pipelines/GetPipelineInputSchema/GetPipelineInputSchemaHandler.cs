using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.GetPipelineInputSchema;

[NonTransactional]
public class GetPipelineInputSchemaHandler(PipelineDbContext db)
{
    public async Task<Result<IReadOnlyList<PipelineInputDto>>> HandleAsync(
        GetPipelineInputSchemaQuery query,
        CancellationToken ct
    )
    {
        var exists = await db.Pipelines.AnyAsync(x => x.Id == query.PipelineId, ct);
        if (!exists)
        {
            return Result.Fail<IReadOnlyList<PipelineInputDto>>($"Pipeline '{query.PipelineId}' not found.");
        }

        var inputs = await db.PipelineInputs
            .AsNoTracking()
            .Where(x => x.PipelineId == query.PipelineId)
            .OrderBy(x => x.Order)
            .Select(i => new PipelineInputDto(
                i.Id,
                i.Key,
                i.Label,
                i.Type,
                i.Cardinality,
                i.IsRequired,
                i.DefaultValue,
                i.Order
            ))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<PipelineInputDto>>(inputs);
    }
}
