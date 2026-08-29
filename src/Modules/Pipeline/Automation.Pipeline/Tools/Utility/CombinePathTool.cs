using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Utility;

public class CombinePathTool : IResolverTool
{
    public string Key => "CombinePath";
    public string Label => "Combine Path";
    public string? Category => "Utility";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "BasePath",
            Label = "Base Path",
            PrimitiveType = PinPrimitiveType.Path,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        },
        new()
        {
            Id = "SubFolder",
            Label = "Sub Folder",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = false,
            DefaultValue = ""
        },
        new()
        {
            Id = "FileName",
            Label = "File Name",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = false,
            DefaultValue = ""
        },
        new()
        {
            Id = "Extension",
            Label = "Extension",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = false,
            DefaultValue = ""
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "FullPath",
            Label = "Full Path",
            PrimitiveType = PinPrimitiveType.Path,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var basePath = inputs.GetValueOrDefault("BasePath")?.ToString()?.Trim() ?? string.Empty;
        var subFolder = inputs.GetValueOrDefault("SubFolder")?.ToString()?.Trim() ?? string.Empty;
        var fileName = inputs.GetValueOrDefault("FileName")?.ToString()?.Trim() ?? string.Empty;
        var extension = inputs.GetValueOrDefault("Extension")?.ToString()?.Trim() ?? string.Empty;

        var fullFileName = fileName;
        if (!string.IsNullOrEmpty(extension))
        {
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }
            if (!fullFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                fullFileName += extension;
            }
        }

        var combined = basePath;
        if (!string.IsNullOrEmpty(subFolder))
        {
            combined = Path.Combine(combined, subFolder);
        }
        if (!string.IsNullOrEmpty(fullFileName))
        {
            combined = Path.Combine(combined, fullFileName);
        }

        combined = combined.Replace('\\', '/');

        return Task.FromResult(new Dictionary<string, object>
        {
            ["FullPath"] = combined
        });
    }
}
