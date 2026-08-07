using Automation.SystemAbstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Identity.Extensions;

public static class IdentitySystemSettingExtensions
{
    public static IServiceCollection AddIdentitySystemSettings(this IServiceCollection services)
    {
        services.AddSystemSetting(Automation.Identity.Constants.IdentitySettings.DefaultRole, Automation.Identity.Constants.IdentityRoles.User, "string", "Vai trÃ² máº·c Ä‘á»‹nh cho ngÆ°á»i dÃ¹ng má»›i Ä‘Äƒng kÃ½");

        return services;
    }
}


