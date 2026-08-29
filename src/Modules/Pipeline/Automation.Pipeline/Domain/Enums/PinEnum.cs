namespace Automation.Pipeline.Domain.Enums;

public enum PinPrimitiveType
{
    String,
    Number,
    Boolean,
    Path,
    EntityRef,
    Asset,
    Variable,
}

public enum PinCardinality
{
    Single,
    Array,
    Map,
}
