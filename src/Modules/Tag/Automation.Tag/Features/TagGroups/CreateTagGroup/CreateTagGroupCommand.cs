using FluentValidation;

namespace Automation.Tag.Features.TagGroups.CreateTagGroup;

public record CreateTagGroupCommand(Guid ProjectId, string Scope, string Name);
