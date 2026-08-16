using Automation.Inspection.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.InspectorRules.DeleteInspectorRule;

[Transactional(typeof(InspectionDbContext))]
public class DeleteInspectorRuleHandler(InspectionDbContext db)
{
    public async Task<Result> HandleAsync(DeleteInspectorRuleCommand command, CancellationToken ct)
    {
        var rule = await db.InspectorRules
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (rule is null)
            return Result.Fail($"Inspector rule with ID '{command.Id}' was not found.");

        db.InspectorRules.Remove(rule);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
