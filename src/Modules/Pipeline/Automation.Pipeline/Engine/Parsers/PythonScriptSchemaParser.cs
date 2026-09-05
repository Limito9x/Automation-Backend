using System.Text.RegularExpressions;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Engine.Parsers;

public record ParsedScriptSchemaResult(
    string SuggestedName,
    string SuggestedLabel,
    string Executor,
    string? Description,
    List<PinDefinition> Inputs,
    List<PinDefinition> Outputs
);

public static class PythonScriptSchemaParser
{
    public static ParsedScriptSchemaResult Parse(string scriptContent, string? fileName = null)
    {
        var suggestedName = !string.IsNullOrWhiteSpace(fileName) 
            ? Path.GetFileNameWithoutExtension(fileName) 
            : "CustomScriptNode";
        
        var suggestedLabel = ToLabel(suggestedName);
        var executor = (scriptContent.Contains("import unreal") || scriptContent.Contains("from unreal import"))
            ? "unreal"
            : (scriptContent.Contains("import bpy") || scriptContent.Contains("from bpy import") ? "blender" : "python");
        
        var inputs = new List<PinDefinition>();
        var outputs = new List<PinDefinition>();
        string? description = null;

        // 1. Extract docstring if present
        var docstringMatch = Regex.Match(scriptContent, @"def\s+main\s*\([^)]*\)\s*(?:->[^:]+)?:\s*[""']{3}([\s\S]*?)[""']{3}");
        if (docstringMatch.Success)
        {
            description = docstringMatch.Groups[1].Value.Trim();
        }

        // 2. Extract main function signature (or alias / fallback)
        var mainMatch = Regex.Match(scriptContent, @"def\s+main\s*\(([\s\S]*?)\)\s*(?:->\s*([^:]+))?:");
        if (!mainMatch.Success)
        {
            var aliasMatch = Regex.Match(scriptContent, @"\bmain\s*=\s*([a-zA-Z0-9_]+)");
            if (aliasMatch.Success)
            {
                var targetFunc = aliasMatch.Groups[1].Value;
                mainMatch = Regex.Match(scriptContent, $@"def\s+{targetFunc}\s*\(([\s\S]*?)\)\s*(?:->\s*([^:]+))?:");
            }
        }
        if (!mainMatch.Success)
        {
            mainMatch = Regex.Match(scriptContent, @"def\s+run\s*\(([\s\S]*?)\)\s*(?:->\s*([^:]+))?:");
        }
        if (!mainMatch.Success)
        {
            mainMatch = Regex.Match(scriptContent, @"def\s+execute\s*\(([\s\S]*?)\)\s*(?:->\s*([^:]+))?:");
        }

        if (mainMatch.Success)
        {
            var rawParams = mainMatch.Groups[1].Value;
            var paramList = SplitParameters(rawParams);

            foreach (var p in paramList)
            {
                var trimmed = p.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed == "self" || trimmed == "*args" || trimmed == "**kwargs")
                    continue;

                var pin = ParseInputPin(trimmed);
                inputs.Add(pin);
            }
        }

        // 3. Extract return statement outputs STRICTLY from main function body
        string mainBody = scriptContent;
        if (mainMatch.Success)
        {
            var fromMain = scriptContent.Substring(mainMatch.Index);
            var afterMainSig = fromMain.Substring(mainMatch.Length);
            // End of main function is at next unindented top-level definition or block
            var nextTopLevelMatch = Regex.Match(afterMainSig, @"\r?\n(def\s+|class\s+|if\s+__name__|[a-zA-Z_][a-zA-Z0-9_]*\s*=)");
            if (nextTopLevelMatch.Success)
            {
                mainBody = fromMain.Substring(0, mainMatch.Length + nextTopLevelMatch.Index);
            }
            else
            {
                mainBody = fromMain;
            }
        }

        var returnDictMatch = Regex.Match(mainBody, @"return\s*\{([\s\S]*?)\}");
        if (!returnDictMatch.Success && !mainMatch.Success)
        {
            // Fallback for flat scripts without def main
            returnDictMatch = Regex.Match(scriptContent, @"return\s*\{([\s\S]*?)\}");
        }

        if (returnDictMatch.Success)
        {
            var dictContent = returnDictMatch.Groups[1].Value;
            var keyMatches = Regex.Matches(dictContent, @"[""']([a-zA-Z0-9_]+)[""']\s*:");
            
            foreach (Match kMatch in keyMatches)
            {
                var outKey = kMatch.Groups[1].Value;
                if (!outputs.Any(o => o.Id == outKey))
                {
                    var isPath = outKey.Contains("path", StringComparison.OrdinalIgnoreCase) || 
                                 outKey.Contains("file", StringComparison.OrdinalIgnoreCase) || 
                                 outKey.Contains("dir", StringComparison.OrdinalIgnoreCase);

                    outputs.Add(new PinDefinition
                    {
                        Id = outKey,
                        Label = ToLabel(outKey),
                        PrimitiveType = isPath ? PinPrimitiveType.Path : PinPrimitiveType.String,
                        Cardinality = outKey.EndsWith("s", StringComparison.OrdinalIgnoreCase) && !outKey.EndsWith("pass", StringComparison.OrdinalIgnoreCase)
                            ? PinCardinality.Array 
                            : PinCardinality.Single,
                        IsRequired = true
                    });
                }
            }
        }
        else
        {
            // Fallback: If return is a variable (e.g. return manifest_data or return result)
            var returnVarMatch = Regex.Match(mainBody, @"return\s+([a-zA-Z0-9_]+)\s*(?:#.*)?$");
            if (returnVarMatch.Success)
            {
                var varName = returnVarMatch.Groups[1].Value;
                if (varName != "None" && varName != "True" && varName != "False")
                {
                    var pinId = varName.Contains("manifest", StringComparison.OrdinalIgnoreCase) ? "manifest" : varName;
                    outputs.Add(new PinDefinition
                    {
                        Id = pinId,
                        Label = ToLabel(pinId),
                        PrimitiveType = PinPrimitiveType.String,
                        Cardinality = PinCardinality.Single,
                        IsRequired = true
                    });
                }
            }
        }

        return new ParsedScriptSchemaResult(
            suggestedName,
            suggestedLabel,
            executor,
            description,
            inputs,
            outputs
        );
    }

    private static PinDefinition ParseInputPin(string paramDef)
    {
        // Format can be:
        // name: type = default
        // name: type
        // name = default
        // name
        string name;
        string? typeStr = null;
        string? defaultStr = null;
        bool hasDefault = false;

        if (paramDef.Contains('='))
        {
            hasDefault = true;
            var eqParts = paramDef.Split('=', 2);
            defaultStr = eqParts[1].Trim();
            var left = eqParts[0].Trim();

            if (left.Contains(':'))
            {
                var colonParts = left.Split(':', 2);
                name = colonParts[0].Trim();
                typeStr = colonParts[1].Trim();
            }
            else
            {
                name = left;
            }
        }
        else if (paramDef.Contains(':'))
        {
            var colonParts = paramDef.Split(':', 2);
            name = colonParts[0].Trim();
            typeStr = colonParts[1].Trim();
        }
        else
        {
            name = paramDef;
        }

        var primitiveType = PinPrimitiveType.String;
        var cardinality = PinCardinality.Single;

        // Detect type from annotation or default or name
        var lowerType = (typeStr ?? "").ToLowerInvariant();
        var lowerName = name.ToLowerInvariant();

        if (lowerType.Contains("list") || lowerType.Contains("[]") || lowerType.Contains("array") || lowerType.Contains("set"))
        {
            cardinality = PinCardinality.Array;
            primitiveType = lowerType.Contains("int") || lowerType.Contains("float") 
                ? PinPrimitiveType.Number 
                : (lowerName.Contains("path") || lowerName.Contains("file") ? PinPrimitiveType.Path : PinPrimitiveType.String);
        }
        else if (lowerType.Contains("bool"))
        {
            primitiveType = PinPrimitiveType.Boolean;
        }
        else if (lowerType.Contains("int") || lowerType.Contains("float") || lowerType.Contains("number"))
        {
            primitiveType = PinPrimitiveType.Number;
        }
        else if (lowerType.Contains("path") || lowerName.Contains("path") || lowerName.Contains("file") || lowerName.Contains("dir") || lowerName.Contains("folder"))
        {
            primitiveType = PinPrimitiveType.Path;
        }
        else if (hasDefault && defaultStr != null)
        {
            if (defaultStr == "True" || defaultStr == "False")
                primitiveType = PinPrimitiveType.Boolean;
            else if (double.TryParse(defaultStr, out _))
                primitiveType = PinPrimitiveType.Number;
            else if (defaultStr.StartsWith('[') && defaultStr.EndsWith(']'))
                cardinality = PinCardinality.Array;
        }

        object? parsedDefault = null;
        if (hasDefault && defaultStr != null)
        {
            var cleanDefault = defaultStr.Trim('"', '\'');
            if (cleanDefault == "None")
            {
                parsedDefault = null;
            }
            else if (primitiveType == PinPrimitiveType.Boolean && bool.TryParse(cleanDefault.ToLower(), out var bVal))
            {
                parsedDefault = bVal;
            }
            else if (primitiveType == PinPrimitiveType.Number && double.TryParse(cleanDefault, out var nVal))
            {
                parsedDefault = nVal;
            }
            else
            {
                parsedDefault = cleanDefault;
            }
        }

        return new PinDefinition
        {
            Id = name,
            Label = ToLabel(name),
            PrimitiveType = primitiveType,
            Cardinality = cardinality,
            IsRequired = !hasDefault,
            DefaultValue = parsedDefault
        };
    }

    private static List<string> SplitParameters(string rawParams)
    {
        var result = new List<string>();
        var depth = 0;
        var current = "";

        foreach (var ch in rawParams)
        {
            if (ch is '(' or '[' or '{') depth++;
            else if (ch is ')' or ']' or '}') depth--;

            if (ch == ',' && depth == 0)
            {
                if (!string.IsNullOrWhiteSpace(current))
                    result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += ch;
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
            result.Add(current.Trim());

        return result;
    }

    private static string ToLabel(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;
        var words = Regex.Replace(key, "([a-z])([A-Z])", "$1 $2")
                         .Replace('_', ' ')
                         .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        return string.Join(" ", words.Select(w => char.ToUpper(w[0]) + (w.Length > 1 ? w[1..] : "")));
    }
}
