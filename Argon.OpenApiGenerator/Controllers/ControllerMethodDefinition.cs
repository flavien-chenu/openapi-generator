using System.Collections.Generic;
using System.Net.Http;

namespace Argon.OpenApiGenerator.Controllers;

/// <summary>
/// Represents the definition of a controller action method.
/// </summary>
internal sealed record ControllerMethodDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerMethodDefinition"/> class.
    /// </summary>
    public ControllerMethodDefinition()
    {
        Parameters = new List<ControllerParameterDefinition>();
        ReturnType = string.Empty;
        Name = string.Empty;
        HttpMethod = HttpMethod.Get;
    }

    /// <summary>
    /// Gets or sets the method name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method (GET, POST, PUT, DELETE, etc.).
    /// </summary>
    public HttpMethod HttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the route template for this action.
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the XML documentation for this method.
    /// </summary>
    public string Documentation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of method parameters.
    /// </summary>
    public List<ControllerParameterDefinition> Parameters { get; set; }

    /// <summary>
    /// Gets or sets the return type of the method.
    /// </summary>
    public string ReturnType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this operation is deprecated.
    /// </summary>
    public bool Deprecated { get; set; } = false;
}
