namespace OpcUaWorkerService.Configuration;

public class OpcUaVariablesOptions
{
    public const string SectionName = "OpcUa:Variables";

    public List<VariableDefinition> Read { get; set; } = new();
    public List<VariableDefinition> Write { get; set; } = new();
}

public class VariableDefinition
{
    public string Name { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string DataType { get; set; } = "String";
    public object? DefaultValue { get; set; }
}