using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Tag.Features.TagGroups.UpdateTagGroup;

public class UpdateTagGroupHandler(TagDbContext db)
{
    public async Task<Result<TagGroupDto>> HandleAsync(UpdateTagGroupCommand command, CancellationToken ct)
    {
        var group = await db.TagGroups.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (group is null)
            return Result.Fail("Tag group not found.");

        var duplicate = await db.TagGroups.AnyAsync(
            x => x.Scope == command.Scope && x.Name == command.Name && x.Id != command.Id, ct);
        if (duplicate)
            return Result.Fail("A tag group with the same scope and name already exists.");

        group.Update(command.Scope, command.Name);
        await db.SaveChangesAsync(ct);

        var dto = group.Adapt<TagGroupDto>();
        return Result.Ok(dto);
    }
}