using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Tag.Features.TagLinks.CreateTagLink;

public class CreateTagLinkHandler(TagDbContext db)
{
    public async Task<Result<TagLinkDto>> HandleAsync(
        CreateTagLinkCommand command,
        CancellationToken ct
    )
    {
        var tag = await db
            .TagItems.Include(x => x.TagGroup)
            .FirstOrDefaultAsync(x => x.Id == command.TagId, ct);
        if (tag is null)
            return Result.Fail("Tag not found.");

        // Allow duplicate tag on same entity — each link is independent (especially with metadata)
        var link = new Domain.Entities.TagLink(
            tag.TagGroup.ProjectId,
            command.TagId,
            command.EntityType,
            command.EntityId,
            command.Metadata
        );
        db.TagLinks.Add(link);
        await db.SaveChangesAsync(ct);

        var dto = new TagLinkDto(
            link.Id,
            link.TagId,
            link.EntityType,
            link.EntityId,
            link.Metadata?.RootElement.ToString()
        );
        return Result.Ok(dto);
    }
}
