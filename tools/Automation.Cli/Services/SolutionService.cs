using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Automation.Cli.Services;

public class SolutionService
{
    public async Task AddProjectToSolutionAsync(string projectPath, string workingDirectory)
    {
        Console.WriteLine($"Adding {projectPath} to Solution...");
        await RunCommandAsync("dotnet", $"sln add {projectPath}", workingDirectory);
    }

    public async Task AddProjectReferenceAsync(string targetProject, string referenceProject, string workingDirectory)
    {
        Console.WriteLine($"Adding reference {referenceProject} to {targetProject}...");
        await RunCommandAsync("dotnet", $"add {targetProject} reference {referenceProject}", workingDirectory);
    }

    public async Task RemoveProjectFromSolutionAsync(string projectPath, string workingDirectory)
    {
        Console.WriteLine($"Removing {projectPath} from Solution...");
        await RunCommandAsync("dotnet", $"sln remove {projectPath}", workingDirectory);
    }

    public async Task RemoveProjectReferenceAsync(string targetProject, string referenceProject, string workingDirectory)
    {
        Console.WriteLine($"Removing reference {referenceProject} from {targetProject}...");
        await RunCommandAsync("dotnet", $"remove {targetProject} reference {referenceProject}", workingDirectory);
    }

    private async Task RunCommandAsync(string command, string arguments, string workingDirectory)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error running {command} {arguments}:");
            Console.WriteLine(error);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(output.Trim());
            Console.ResetColor();
        }
    }
}


