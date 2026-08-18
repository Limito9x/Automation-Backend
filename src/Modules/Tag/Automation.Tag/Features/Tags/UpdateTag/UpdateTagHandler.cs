using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Tag.Features.Tags.UpdateTag;

public class UpdateTagHandler(TagDbContext db)
{
    public async Task<Result<TagItemDto>> HandleAsync(UpdateTagCommand command, CancellationToken ct)
    {
        var tag = await db.TagItems.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (tag is null)
            return Result.Fail("Tag not found.");

        var duplicate = await db.TagItems.AnyAsync(
            x => x.TagGroupId == tag.TagGroupId && x.Name == command.Name && x.Id != command.Id, ct);
        if (duplicate)
            return Result.Fail("A tag with the same name already exists in this group.");

        tag.Update(command.Name, command.Color);
        await db.SaveChangesAsync(ct);

        var dto = tag.Adapt<TagItemDto>();
        return Result.Ok(dto);
    }
}