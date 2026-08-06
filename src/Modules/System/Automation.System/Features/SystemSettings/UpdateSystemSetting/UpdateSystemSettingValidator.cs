namespace Automation.SystemModule.Features.SystemSettings.UpdateSystemSetting;

internal class UpdateSystemSettingValidator : Validator<UpdateSystemSettingCommand>
{
    public UpdateSystemSettingValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
    }
}

