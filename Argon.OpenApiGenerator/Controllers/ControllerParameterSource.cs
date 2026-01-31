namespace Argon.OpenApiGenerator.Controllers;

/// <summary>
/// Specifies the source location of a controller parameter.
/// </summary>
internal enum ControllerParameterSource
{
    /// <summary>
    /// Default parameter binding (ASP.NET Core determines the source).
    /// </summary>
    Default,

    /// <summary>
    /// Parameter is bound from the route path.
    /// </summary>
    Path,

    /// <summary>
    /// Parameter is bound from the query string.
    /// </summary>
    Query,

    /// <summary>
    /// Parameter is bound from a request header.
    /// </summary>
    Header,

    /// <summary>
    /// Parameter is bound from the request body.
    /// </summary>
    Body
}
