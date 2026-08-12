using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Automation.Tag.Infrastructure.Persistence;

namespace Automation.Tag;

public sealed class TagModule : IModule, IPermissionModule
{
    public string Name => "Tag";
    public string SchemaName => "tag";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<TagDbContext>(config, SchemaName);
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.TagPermissions().GetPermissions();

    public List<Type> Endpoints => [];
}

