using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Automation.Platform.Infrastructure.Persistence;

namespace Automation.Platform;

public sealed class PlatformModule : IModule, IPermissionModule
{
    public string Name => "Platform";
    public string SchemaName => "platform";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<PlatformDbContext>(config, SchemaName);
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.PlatformPermissions().GetPermissions();

    public List<Type> Endpoints => [];
}
