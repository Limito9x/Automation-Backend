using System.CommandLine;
using System.IO;
using Automation.Cli.Services;

namespace Automation.Cli.Commands;

public static class AddModuleCommand
{
    public static Command Create()
    {
        var nameArgument = new Argument<string>("name", "The name of the module (e.g. Orders, Billing)");

        var command = new Command("add-module", "Scaffolds a new module")
        {
            nameArgument
        };

        command.SetHandler(async (string name) =>
        {
            Console.WriteLine($"Scaffolding module: {name}...");

            var scaffoldingService = new ScaffoldingService();
            var solutionService = new SolutionService();
            var registryUpdater = new ModuleRegistryUpdater();

            // Lùi về root nếu đang chạy từ thư mục tools
            var currentDir = Directory.GetCurrentDirectory();
            var rootDir = currentDir;
            if (Path.GetFileName(currentDir).Equals("Automation.Cli", StringComparison.OrdinalIgnoreCase))
            {
                rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
            }

            var moduleDir = Path.Combine(rootDir, "src", "Modules", name, $"Automation.{name}");
            var projectFile = Path.Combine(moduleDir, $"Automation.{name}.csproj");

            var model = new { ModuleName = name };

            // 1. Tạo csproj
            await scaffoldingService.RenderTemplateAsync(
                "Module/Module.csproj.sbn",
                projectFile,
                model
            );

            // 2. Tạo GlobalUsing.cs
            await scaffoldingService.RenderTemplateAsync(
                "Module/GlobalUsing.sbn",
                Path.Combine(moduleDir, "GlobalUsing.cs"),
                model
            );

            // 3. Tạo ModuleEntry.cs
            await scaffoldingService.RenderTemplateAsync(
                "Module/ModuleEntry.sbn",
                Path.Combine(moduleDir, $"{name}Module.cs"),
                model
            );
            
            // 3.5. Tạo DbContext
            await scaffoldingService.RenderTemplateAsync(
                "Module/DbContext.sbn",
                Path.Combine(moduleDir, "Infrastructure", "Persistence", $"{name}DbContext.cs"),
                model
            );

            // 3.6 Tạo Constants/Permissions.cs
            await scaffoldingService.RenderTemplateAsync(
                "Module/Permissions.sbn",
                Path.Combine(moduleDir, "Constants", $"{name}Permissions.cs"),
                model
            );

            // 4. Link to solution
            await solutionService.AddProjectToSolutionAsync($"src/Modules/{name}/Automation.{name}/Automation.{name}.csproj", rootDir);

            // 5. Add reference to Api project
            var apiProject = Path.Combine(rootDir, "src", "Automation.Api", "Automation.Api.csproj");
            var moduleProject = Path.Combine(moduleDir, $"Automation.{name}.csproj");
            await solutionService.AddProjectReferenceAsync(apiProject, moduleProject, rootDir);
            
            // 6. Update ModuleRegistry
            await registryUpdater.UpdateRegistryAsync(name);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Module {name} created successfully!");
            Console.ResetColor();

        }, nameArgument);

        return command;
    }
}


