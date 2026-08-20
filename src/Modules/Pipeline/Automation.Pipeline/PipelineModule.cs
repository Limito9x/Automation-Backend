using Automation.Pipeline.Engine;
using Automation.Pipeline.Engine.Messages;
using Automation.Pipeline.Extensions;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Pipeline.Infrastructure.Redis;
using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Automation.Pipeline;

public sealed class PipelineModule : IModule, IPermissionModule
{
    public string Name => "Pipeline";
    public string SchemaName => "pipeline";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<PipelineDbContext>(config, SchemaName);
        services.AddPipelineTools();
        services.AddPipelineAssetSlots();

        services.AddSingleton<IExecutionStateStore, RedisExecutionStateStore>();
        services.AddSingleton<IDagPlanner, DagPlanner>();
        services.AddScoped<IInputResolver, InputResolver>();
        services.AddScoped<IAgentBatchBuilder, AgentBatchBuilder>();
        services.AddScoped<IPipelineExecutionEngine, PipelineExecutionEngine>();
        services.AddPipelineGrpcServices();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // 1. Route StageTaskMessage out to RabbitMQ queue "stage_tasks"
        options.PublishMessage<StageTaskMessage>()
               .ToRabbitQueue("stage_tasks");

        // 2. Listen to "stage_results" from Agent worker
        options.ListenToRabbitQueue("stage_results")
               .DefaultIncomingMessage<StageResultMessage>();
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.PipelinePermissions().GetPermissions();

    public List<Type> Endpoints => [.. DiscoveredTypes.All];
}
