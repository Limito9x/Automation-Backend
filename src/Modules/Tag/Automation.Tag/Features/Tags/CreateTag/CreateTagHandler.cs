using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Tag.Features.Tags.CreateTag;

public class CreateTagHandler(TagDbContext db)
{
    public async Task<Result<TagItemDto>> HandleAsync(CreateTagCommand command, CancellationToken ct)
    {
        var groupExists = await db.TagGroups.AnyAsync(x => x.Id == command.TagGroupId, ct);
        if (!groupExists)
            return Result.Fail("Tag group not found.");

        var duplicate = await db.TagItems.AnyAsync(
            x => x.TagGroupId == command.TagGroupId && x.Name == command.Name, ct);
        if (duplicate)
            return Result.Fail("A tag with the same name already exists in this group.");

        var tag = new Domain.Entities.TagItem(command.TagGroupId, command.Name, command.Color);
        db.TagItems.Add(tag);
        await db.SaveChangesAsync(ct);

        var dto = tag.Adapt<TagItemDto>();
        return Result.Ok(dto);
    }
}