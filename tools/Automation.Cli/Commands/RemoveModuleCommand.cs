using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Automation.Cli.Services;

namespace Automation.Cli.Commands;

public static class RemoveModuleCommand
{
    public static Command Create()
    {
        var moduleArgument = new Argument<string>("module-name", "The name of the module to remove (e.g. Billing)");

        var command = new Command("remove-module", "Permanently deletes a module from the project")
        {
            moduleArgument
        };

        command.SetHandler(async (moduleName) =>
        {
            await ExecuteAsync(moduleName);
        }, moduleArgument);

        return command;
    }

    private static async Task ExecuteAsync(string moduleName)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine("=================== DANGER ZONE ===================");
        Console.WriteLine($"WARNING: You are about to permanently delete the module '{moduleName}'.");
        Console.WriteLine("This action cannot be undone and will delete all files in the module's folder,");
        Console.WriteLine("remove its project references, and unregister it from the application.");
        Console.WriteLine();
        Console.Write($"To confirm, please type the exact module name '{moduleName}': ");
        Console.ResetColor();

        var input = Console.ReadLine();
        if (input != moduleName)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Confirmation failed. Aborting operation.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Starting removal process for module '{moduleName}'...");

        var currentDir = Directory.GetCurrentDirectory();
        var rootDir = currentDir;
        if (Path.GetFileName(currentDir).Equals("Automation.Cli", StringComparison.OrdinalIgnoreCase))
        {
            rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
        }

        var modulePath = Path.Combine(rootDir, "src", "Modules", moduleName);
        var projectFile = Path.Combine(modulePath, $"Automation.{moduleName}", $"Automation.{moduleName}.csproj");
        var relativeProjectFile = Path.Combine("src", "Modules", moduleName, $"Automation.{moduleName}", $"Automation.{moduleName}.csproj");

        var solutionService = new SolutionService();
        var registryUpdater = new ModuleRegistryUpdater();

        if (File.Exists(projectFile))
        {
            await solutionService.RemoveProjectReferenceAsync(
                Path.Combine("src", "Automation.Api", "Automation.Api.csproj"),
                relativeProjectFile,
                rootDir
            );

            await solutionService.RemoveProjectFromSolutionAsync(
                relativeProjectFile,
                rootDir
            );
        }

        await registryUpdater.RemoveRegistryAsync(moduleName);

        if (Directory.Exists(modulePath))
        {
            Console.WriteLine($"Deleting folder {modulePath}...");
            Directory.Delete(modulePath, true);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Module '{moduleName}' has been successfully removed.");
        Console.ResetColor();
    }
}


