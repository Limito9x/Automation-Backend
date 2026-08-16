using Automation.Inspection.Domain.Entities;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.InspectorRules.CreateInspectorRule;

[Transactional(typeof(InspectionDbContext))]
public class CreateInspectorRuleHandler(InspectionDbContext db)
{
    public async Task<Result<InspectorRuleDto>> HandleAsync(CreateInspectorRuleCommand command, CancellationToken ct)
    {
        var inspector = await db.Inspectors
            .FirstOrDefaultAsync(x => x.Id == command.InspectorId, ct);

        if (inspector is null)
            return Result.Fail($"Inspector with ID '{command.InspectorId}' was not found.");

        var rule = new InspectorRule(
            command.ProjectId,
            command.PlatformExtensionId,
            command.InspectorId,
            command.ContentTypeId,
            command.Enabled
        );

        db.InspectorRules.Add(rule);
        await db.SaveChangesAsync(ct);

        var dto = new InspectorRuleDto(
            rule.Id,
            rule.ProjectId,
            rule.PlatformExtensionId,
            rule.ContentTypeId,
            rule.InspectorId,
            inspector.Name,
            inspector.Key,
            inspector.ExecutorKey,
            rule.Enabled,
            rule.CreatedAt
        );

        return Result.Ok(dto);
    }
}
