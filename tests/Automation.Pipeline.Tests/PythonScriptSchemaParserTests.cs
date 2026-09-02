using Automation.Pipeline.Engine.Parsers;
using FluentAssertions;
using Xunit;

namespace Automation.Pipeline.Tests;

public class PythonScriptSchemaParserTests
{
    [Fact]
    public void Parse_ScriptWithHelperFunctions_ShouldOnlyExtractMainReturnOutputs()
    {
        var scriptContent = """
            def extract_helper(data):
                return {
                    "field1": 1,
                    "field2": 2,
                    "field3": 3,
                    "field4": 4,
                    "field5": 5,
                    "field6": 6,
                    "field7": 7,
                    "field8": 8,
                    "field9": 9
                }

            def main(input_path: str) -> dict:
                data = extract_helper(input_path)
                return {
                    "metadata": data
                }

            if __name__ == "__main__":
                pass
            """;

        var result = PythonScriptSchemaParser.Parse(scriptContent, "daz_inspector.py");

        result.Inputs.Should().HaveCount(1);
        result.Inputs[0].Id.Should().Be("input_path");

        result.Outputs.Should().HaveCount(1);
        result.Outputs[0].Id.Should().Be("metadata");
    }
}
