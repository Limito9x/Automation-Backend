using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.InspectorRules.UpdateInspectorRule;

[Transactional(typeof(InspectionDbContext))]
public class UpdateInspectorRuleHandler(InspectionDbContext db)
{
    public async Task<Result<InspectorRuleDto>> HandleAsync(UpdateInspectorRuleCommand command, CancellationToken ct)
    {
        var rule = await db.InspectorRules
            .Include(x => x.Inspector)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (rule is null)
            return Result.Fail($"Inspector rule with ID '{command.Id}' was not found.");

        rule.Update(command.Enabled, command.PlatformExtensionId, command.ContentTypeId);
        await db.SaveChangesAsync(ct);

        var dto = new InspectorRuleDto(
            rule.Id,
            rule.ProjectId,
            rule.PlatformExtensionId,
            rule.ContentTypeId,
            rule.InspectorId,
            rule.Inspector.Name,
            rule.Inspector.Key,
            rule.Inspector.ExecutorKey,
            rule.Enabled,
            rule.CreatedAt
        );

        return Result.Ok(dto);
    }
}
