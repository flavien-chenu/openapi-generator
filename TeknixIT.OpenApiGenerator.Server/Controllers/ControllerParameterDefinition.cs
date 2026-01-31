namespace TeknixIT.OpenApiGenerator.Server.Controllers;

/// <summary>
/// Represents the definition of a controller action parameter.
/// </summary>
internal sealed record ControllerParameterDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerParameterDefinition"/> class.
    /// </summary>
    public ControllerParameterDefinition()
    {
        Name = string.Empty;
        Type = string.Empty;
        Source = ControllerParameterSource.Default;
        IsRequired = false;
    }

    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the C# type of the parameter.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the parameter source (path, query, body, header).
    /// </summary>
    public ControllerParameterSource Source { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this parameter is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the default value for the parameter, if any.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the XML documentation for this parameter.
    /// </summary>
    public string Documentation { get; set; } = string.Empty;
}
