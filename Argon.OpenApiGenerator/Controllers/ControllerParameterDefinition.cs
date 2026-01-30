namespace Argon.OpenApiGenerator.Controllers;

public record ControllerParameterDefinition
{
    public ControllerParameterDefinition()
    {
        Name = string.Empty;
        Type = string.Empty;
        Source = ControllerParameterSource.Default;
        IsRequired = false;
    }

    public string Name { get; set; }
    public string Type { get; set; }
    public ControllerParameterSource Source { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string Documentation { get; set; } = string.Empty;
}
