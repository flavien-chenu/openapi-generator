using System.Collections.Generic;

namespace Argon.OpenApiGenerator.Controllers;

public class ControllerDefinition
{
    public ControllerDefinition(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public string Route { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public List<ControllerMethodDefinition> Methods { get; set; } = new();
}
