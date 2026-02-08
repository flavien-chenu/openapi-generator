using TeknixIT.OpenApiGenerator.Server.Controllers;

namespace TeknixIT.OpenApiGenerator.Server;

/// <summary>
/// Options for configuring code generation from OpenAPI schemas.
/// </summary>
public class GeneratorConfiguration
{
    /// <summary>
    /// Paths to OpenAPI files (relative to project or absolute).
    /// </summary>
    public string OpenApiFile { get; set; } = string.Empty;

    /// <summary>
    /// Generate controllers if true.
    /// </summary>
    public bool GenerateControllers { get; set; } = true;

    /// <summary>
    /// Use records instead of classes for DTOs.
    /// </summary>
    public bool UseRecords { get; set; } = true;

    /// <summary>
    /// Base namespace for generated types.
    /// </summary>
    public string BaseNamespace { get; set; } = "Generated";

    /// <summary>
    /// Namespace for contracts.
    /// </summary>
    public string ContractsNamespace { get; set; } = "Contracts";

    /// <summary>
    /// Namespace for controllers.
    /// </summary>
    public string ControllersNamespace { get; set; } = "Controllers";

    /// <summary>
    /// Add validation attributes (Required, StringLength, etc.).
    /// </summary>
    public bool GenerateValidationAttributes { get; set; } = true;

    /// <summary>
    /// Add XML documentation comments.
    /// </summary>
    public bool GenerateXmlDocumentation { get; set; } = true;

    /// <summary>
    /// Generate controllers with async methods.
    /// </summary>
    public bool UseAsyncControllers { get; set; } = true;

    /// <summary>
    /// Add [ApiController] attribute on controllers.
    /// </summary>
    public bool AddApiControllerAttribute { get; set; } = true;

    /// <summary>
    /// Base class for generated controllers. Default is "ControllerBase".
    /// </summary>
    public string ControllerBaseClass { get; set; } = "ControllerBase";

    /// <summary>
    /// Strategy for grouping operations into controllers.
    /// </summary>
    public ControllerGroupingStrategy ControllerGroupingStrategy { get; set; } = ControllerGroupingStrategy.ByTag;
}
