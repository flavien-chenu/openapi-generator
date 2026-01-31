using System.Collections.Generic;

namespace TeknixIT.OpenApiGenerator.Server.Controllers;

/// <summary>
/// Represents the definition of a generated controller.
/// </summary>
internal sealed class ControllerDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerDefinition"/> class.
    /// </summary>
    /// <param name="name">The controller name.</param>
    public ControllerDefinition(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the controller name (without "Controller" suffix).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the base route for the controller.
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the XML documentation for the controller.
    /// </summary>
    public string Documentation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of controller action methods.
    /// </summary>
    public List<ControllerMethodDefinition> Methods { get; set; } = new();
}
