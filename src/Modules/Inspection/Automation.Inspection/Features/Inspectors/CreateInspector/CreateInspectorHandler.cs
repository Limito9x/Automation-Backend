using Automation.Inspection.Domain.Entities;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspectors.CreateInspector;

[Transactional(typeof(InspectionDbContext))]
public class CreateInspectorHandler(InspectionDbContext db)
{
    public async Task<Result<InspectorDto>> HandleAsync(CreateInspectorCommand command, CancellationToken ct)
    {
        var key = command.Key.Trim().ToLowerInvariant();
        var exists = await db.Inspectors
            .AnyAsync(x => x.ProjectId == command.ProjectId && x.Key == key, ct);

        if (exists)
            return Result.Fail($"Inspector with key '{command.Key}' already exists in this project.");

        var inspector = new Inspector(
            command.ProjectId,
            key,
            command.Name,
            command.ExecutorKey,
            command.Description
        );

        db.Inspectors.Add(inspector);
        await db.SaveChangesAsync(ct);

        var dto = inspector.Adapt<InspectorDto>();
        return Result.Ok(dto);
    }
}
