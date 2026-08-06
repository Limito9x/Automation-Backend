using Automation.SharedKernel.Abstractions.Modules;
using Automation.Identity;
using Automation.Files;
using Automation.Notifications;

namespace Automation.Api;

public static class ModuleRegistry
{
    public static readonly IModule[] All = 
    [
        new IdentityModule(),
        new SystemModule.SystemModule(),
        new FilesModule(),
        new NotificationsModule()
    ];

    public static List<Type> AllEndpoints
    {
        get
        {
            var endpoints = All.SelectMany(m => m.Endpoints ?? []).ToList();
#if DEBUG
            endpoints.Add(typeof(Dev.TestNotificationEndpoint));
#endif
            return endpoints;
        }
    }
}
