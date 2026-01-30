using System.Collections.Generic;
using System.Net.Http;

namespace Argon.OpenApiGenerator.Controllers;

public record ControllerMethodDefinition
{
    public ControllerMethodDefinition()
    {
        Parameters = new List<ControllerParameterDefinition>();
        ReturnType = string.Empty;
        Name = string.Empty;
        HttpMethod = HttpMethod.Get;
    }

    public string Name { get; set; }
    public HttpMethod HttpMethod { get; set; }
    public string Route { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public List<ControllerParameterDefinition> Parameters { get; set; }
    public string ReturnType { get; set; }
    public bool Deprecated { get; set; } = false;
}
