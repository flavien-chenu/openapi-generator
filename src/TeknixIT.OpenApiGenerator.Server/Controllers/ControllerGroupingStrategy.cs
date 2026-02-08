namespace TeknixIT.OpenApiGenerator.Server.Controllers;

/// <summary>
/// Defines how controllers are grouped from OpenAPI paths.
/// </summary>
public enum ControllerGroupingStrategy
{
    /// <summary>
    /// Group operations by their OpenAPI tags.
    /// If an operation has no tag, falls back to the first path segment.
    /// </summary>
    ByTag = 0,

    /// <summary>
    /// Group operations by the first segment of their path.
    /// For example, /products/... and /products/{id}/... will be grouped together.
    /// </summary>
    ByFirstPathSegment = 1,

    /// <summary>
    /// Create one controller per unique path (excluding parameters).
    /// For example, /products and /products/{id} will be in separate controllers.
    /// </summary>
    ByPath = 2
}
