using System.CommandLine;
using System.IO;
using Automation.Cli.Services;

namespace Automation.Cli.Commands;

public static class AddCrudCommand
{
    public static Command Create()
    {
        var moduleArgument = new Argument<string>("module", "The name of the module (e.g. Orders)");
        var entityArgument = new Argument<string>("entity", "The name of the entity (e.g. Invoice)");

        var command = new Command("add-crud", "Scaffolds a complete CRUD set of slices for an entity")
        {
            moduleArgument,
            entityArgument
        };

        command.SetHandler(async (string module, string entity) =>
        {
            Console.WriteLine($"Scaffolding CRUD for {entity} in module {module}...");

            var scaffoldingService = new ScaffoldingService();
            
            var currentDir = Directory.GetCurrentDirectory();
            var rootDir = currentDir;
            if (Path.GetFileName(currentDir).Equals("Automation.Cli", StringComparison.OrdinalIgnoreCase))
            {
                rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
            }

            var moduleDir = Path.Combine(rootDir, "src", "Modules", module, $"Automation.{module}");
            var featuresDir = Path.Combine(moduleDir, "Features", $"{entity}s");

            var pluralEntity = $"{entity}s"; // simple pluralization
            var model = new { ModuleName = module, EntityName = entity, PluralEntityName = pluralEntity };

            // 1. Sinh Entity
            await scaffoldingService.RenderTemplateAsync(
                "Crud/Entity.sbn",
                Path.Combine(moduleDir, "Domain", $"{entity}.cs"),
                model
            );

            // 2. Sinh Dto
            await scaffoldingService.RenderTemplateAsync(
                "Crud/Dto.sbn",
                Path.Combine(moduleDir, "Shared", "Dtos", $"{entity}Dto.cs"),
                model
            );

            // 3. Sinh Group
            await scaffoldingService.RenderTemplateAsync(
                "Crud/EndpointGroup.sbn",
                Path.Combine(featuresDir, $"{pluralEntity}Group.cs"),
                model
            );

            // 4. Sinh 5 slices
            string[] slices = [ $"Create{entity}", $"Update{entity}", $"Delete{entity}", $"Get{entity}ById", $"Get{pluralEntity}" ];
            
            foreach (var slice in slices)
            {
                var sliceDir = Path.Combine(featuresDir, slice);
                var sliceModel = new { ModuleName = module, EntityName = entity, PluralEntityName = pluralEntity, ActionName = slice };

                if (slice.StartsWith("Get") && !slice.EndsWith("ById"))
                {
                    // Get All (Paged)
                    await scaffoldingService.RenderTemplateAsync("Query/PagedQuery.sbn", Path.Combine(sliceDir, $"{slice}Query.cs"), sliceModel);
                    await scaffoldingService.RenderTemplateAsync("Query/PagedQueryEndpoint.sbn", Path.Combine(sliceDir, $"{slice}Endpoint.cs"), sliceModel);
                    await scaffoldingService.RenderTemplateAsync("Query/PagedQueryHandler.sbn", Path.Combine(sliceDir, $"{slice}Handler.cs"), sliceModel);
                }
                else if (slice.EndsWith("ById"))
                {
                    // Get By Id
                    await scaffoldingService.RenderTemplateAsync("Query/ByIdQuery.sbn", Path.Combine(sliceDir, $"{slice}Query.cs"), sliceModel);
                    await scaffoldingService.RenderTemplateAsync("Query/QueryEndpoint.sbn", Path.Combine(sliceDir, $"{slice}Endpoint.cs"), sliceModel);
                    await scaffoldingService.RenderTemplateAsync("Query/ByIdQueryHandler.sbn", Path.Combine(sliceDir, $"{slice}Handler.cs"), sliceModel);
                }
                else if (slice.StartsWith("Delete"))
                {
                    // Delete
                    await scaffoldingService.RenderTemplateAsync("Slice/Command.sbn", Path.Combine(sliceDir, $"{slice}Command.cs"), sliceModel);
                    await scaffoldingService.RenderTemplateAsync("Slice/DeleteCommandEndpoint.sbn", Path.Combine(sliceDir, $"{slice}Endpoint.cs"), sliceModel);
                    await scaffoldingService.RenderTemplateAsync("Slice/DeleteCommandHandler.sbn", Path.Combine(sliceDir, $"{slice}Handler.cs"), sliceModel);
                }
                else
                {
                    // Create / Update
                    await scaffoldingService.RenderTemplateAsync("Slice/Command.sbn", Path.Combine(sliceDir, $"{slice}Command.cs"), sliceModel);
                    await scaffoldingService.RenderTemplateAsync("Slice/CommandEndpoint.sbn", Path.Combine(sliceDir, $"{slice}Endpoint.cs"), sliceModel);
                    await scaffoldingService.RenderTemplateAsync("Slice/CommandHandler.sbn", Path.Combine(sliceDir, $"{slice}Handler.cs"), sliceModel);
                    await scaffoldingService.RenderTemplateAsync("Slice/CommandValidator.sbn", Path.Combine(sliceDir, $"{slice}Validator.cs"), sliceModel);
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[IMPORTANT] Don't forget to add DbSet<{entity}> {pluralEntity} {{ get; set; }} to your DbContext in Automation.{module} module!");
            Console.ResetColor();

        }, moduleArgument, entityArgument);

        return command;
    }
}

