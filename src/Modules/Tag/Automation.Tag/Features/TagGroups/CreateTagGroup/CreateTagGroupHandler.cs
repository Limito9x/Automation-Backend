using Automation.Tag.Domain.Entities;
using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Tag.Features.TagGroups.CreateTagGroup;

public class CreateTagGroupHandler(TagDbContext db)
{
    public async Task<Result<TagGroupDto>> HandleAsync(
        CreateTagGroupCommand command,
        CancellationToken ct
    )
    {
        var exists = await db.TagGroups.AnyAsync(
            x => x.Scope == command.Scope && x.Name == command.Name,
            ct
        );

        if (exists)
            return Result.Fail("A tag group with the same scope and name already exists.");

        var group = new TagGroup(command.ProjectId, command.Scope, command.Name);
        db.TagGroups.Add(group);
        await db.SaveChangesAsync(ct);

        var dto = group.Adapt<TagGroupDto>();
        return Result.Ok(dto);
    }
}
