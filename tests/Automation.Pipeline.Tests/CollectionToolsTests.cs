using Automation.Pipeline.Tools;
using Automation.Pipeline.Tools.Collections;
using FluentAssertions;
using Xunit;

namespace Automation.Pipeline.Tests;

public class CollectionToolsTests
{
    private readonly ToolExecutionContext _context = new(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, CancellationToken.None);

    [Fact]
    public async Task GetMapKeysTool_ShouldExtractAllKeys()
    {
        var tool = new GetMapKeysTool();
        var inputs = new Dictionary<string, object>
        {
            ["Map"] = new Dictionary<string, object?> { ["Head"] = "head.fbx", ["Body"] = "body.fbx" }
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        result["Keys"].Should().BeEquivalentTo(new[] { "Head", "Body" });
    }

    [Fact]
    public async Task GetMapValuesTool_ShouldExtractAllValues()
    {
        var tool = new GetMapValuesTool();
        var inputs = new Dictionary<string, object>
        {
            ["Map"] = new Dictionary<string, object?> { ["Head"] = "head.fbx", ["Body"] = "body.fbx" }
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        result["Values"].Should().BeEquivalentTo(new[] { "head.fbx", "body.fbx" });
    }

    [Fact]
    public async Task GetMapItemTool_ShouldReturnValueByKey()
    {
        var tool = new GetMapItemTool();
        var inputs = new Dictionary<string, object>
        {
            ["Map"] = new Dictionary<string, object?> { ["Head"] = "head.fbx", ["Weapon"] = "sword.fbx" },
            ["Key"] = "Weapon"
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        result["Found"].Should().Be(true);
        result["Value"].Should().Be("sword.fbx");
    }

    [Fact]
    public async Task GetMapItemTool_NotFound_ShouldReturnDefaultValue()
    {
        var tool = new GetMapItemTool();
        var inputs = new Dictionary<string, object>
        {
            ["Map"] = new Dictionary<string, object?> { ["Head"] = "head.fbx" },
            ["Key"] = "Shield",
            ["DefaultValue"] = "default_shield.fbx"
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        result["Found"].Should().Be(false);
        result["Value"].Should().Be("default_shield.fbx");
    }

    [Fact]
    public async Task GetArrayItemTool_ShouldReturnItemByIndex()
    {
        var tool = new GetArrayItemTool();
        var inputs = new Dictionary<string, object>
        {
            ["Array"] = new[] { "MeshA", "MeshB", "MeshC" },
            ["Index"] = 1
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        result["Found"].Should().Be(true);
        result["Item"].Should().Be("MeshB");
    }

    [Fact]
    public async Task GetArrayItemTool_NegativeIndex_ShouldReturnFromEnd()
    {
        var tool = new GetArrayItemTool();
        var inputs = new Dictionary<string, object>
        {
            ["Array"] = new[] { "MeshA", "MeshB", "MeshC" },
            ["Index"] = -1
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        result["Found"].Should().Be(true);
        result["Item"].Should().Be("MeshC");
    }

    [Fact]
    public async Task GetCollectionCountTool_ShouldReturnAccurateCount()
    {
        var tool = new GetCollectionCountTool();
        var inputs = new Dictionary<string, object>
        {
            ["Collection"] = new[] { "A", "B", "C", "D" }
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        result["Count"].Should().Be(4);
    }

    [Fact]
    public async Task ZipToMapTool_ShouldCombineKeysAndValues()
    {
        var tool = new ZipToMapTool();
        var inputs = new Dictionary<string, object>
        {
            ["Keys"] = new[] { "Head", "Weapon" },
            ["Values"] = new[] { "head.glb", "sword.glb" }
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        var map = result["Map"] as Dictionary<string, object?>;
        map.Should().NotBeNull();
        map!["Head"].Should().Be("head.glb");
        map!["Weapon"].Should().Be("sword.glb");
    }

    [Fact]
    public async Task MakeMapTool_ShouldCreateSingleEntryMap()
    {
        var tool = new Tools.Utility.MakeMapTool();
        var inputs = new Dictionary<string, object>
        {
            ["Key_0"] = "Diffuse",
            ["Value_0"] = "diffuse.png"
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        var map = result["Map"] as Dictionary<string, string>;
        map.Should().NotBeNull();
        map!["Diffuse"].Should().Be("diffuse.png");
    }

    [Fact]
    public async Task MergeMapsTool_ShouldMergeMapsAndOverrideDuplicates()
    {
        var tool = new MergeMapsTool();
        var inputs = new Dictionary<string, object>
        {
            ["MapA"] = new Dictionary<string, object?> { ["A"] = 1, ["B"] = 2 },
            ["MapB"] = new Dictionary<string, object?> { ["B"] = 20, ["C"] = 3 }
        };

        var result = await tool.ExecuteAsync(inputs, _context);

        var map = result["Map"] as Dictionary<string, object?>;
        map.Should().NotBeNull();
        map!["A"].Should().Be(1);
        map!["B"].Should().Be(20);
        map!["C"].Should().Be(3);
    }
}
