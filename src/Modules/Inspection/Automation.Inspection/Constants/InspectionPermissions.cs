using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Inspection.Constants;

public class InspectionPermissions
{
    public static InspectorFeature Inspector { get; } = new();
    public static InspectorRuleFeature InspectorRule { get; } = new();
    public static InspectionFeature Inspection { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Inspector", Inspector.All },
        { "InspectorRule", InspectorRule.All },
        { "Inspection", Inspection.All }
    };

    public class InspectorFeature() : BaseCrudPermission("inspector") { }
    public class InspectorRuleFeature() : BaseCrudPermission("inspector_rule") { }
    public class InspectionFeature() : BaseCrudPermission("inspection") { }
}
