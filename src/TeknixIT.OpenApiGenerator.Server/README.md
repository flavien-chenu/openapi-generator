# TeknixIT.OpenApiGenerator.Server

A C# source generator that creates ASP.NET Core contracts (DTOs) and controllers from OpenAPI specifications at compile time.

## Features

- **Compile-time code generation** - No runtime overhead, fully integrated into your build process
- **OpenAPI 3.x support** - Works with YAML and JSON specifications
- **Highly configurable** - Control every aspect of code generation
- **Contract generation** - Generate DTOs as records or classes
- **Controller generation** - Generate controller stubs with proper routing
- **Validation attributes** - Automatic generation of validation attributes from schema constraints
- **XML documentation** - Generate XML docs from OpenAPI descriptions
- **Async support** - Generate async controller methods
- **.NET Standard 2.0** - Compatible with .NET 8, 9, 10+

## Installation

```bash
dotnet add package TeknixIT.OpenApiGenerator.Server
```

## Quick Start

1. Add your OpenAPI specification file to your project (e.g., `openapi.yaml`)

2. Configure the generator in your `.csproj` file:

```xml
<ItemGroup>
  <OpenApiGeneratorServer Include="openapi.yaml" />
</ItemGroup>
```

3. Build your project - contracts and controllers will be generated automatically

## Configuration

### Basic Configuration

The simplest setup uses default settings:

```xml
<ItemGroup>
  <OpenApiGeneratorServer Include="openapi.yaml" />
</ItemGroup>
```

### Advanced Configuration

Customize generation with metadata:

```xml
<ItemGroup>
  <OpenApiGeneratorServer Include="openapi.yaml">
    <GenerateControllers>true</GenerateControllers>
    <UseRecords>true</UseRecords>
    <BaseNamespace>MyApp.Generated</BaseNamespace>
    <ContractsNamespace>Contracts</ContractsNamespace>
    <ControllersNamespace>Controllers</ControllersNamespace>
    <GenerateValidationAttributes>true</GenerateValidationAttributes>
    <GenerateXmlDocumentation>true</GenerateXmlDocumentation>
    <UseAsyncControllers>true</UseAsyncControllers>
    <AddApiControllerAttribute>true</AddApiControllerAttribute>
    <ControllerBaseClass>ControllerBase</ControllerBaseClass>
    <ControllerGroupingStrategy>ByTag</ControllerGroupingStrategy>
  </OpenApiGeneratorServer>
</ItemGroup>
```

### Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `GenerateControllers` | `true` | Generate controller stubs |
| `UseRecords` | `true` | Use records instead of classes for DTOs |
| `BaseNamespace` | `Generated` | Root namespace for generated code |
| `ContractsNamespace` | `Contracts` | Namespace for contracts (appended to BaseNamespace) |
| `ControllersNamespace` | `Controllers` | Namespace for controllers (appended to BaseNamespace) |
| `GenerateValidationAttributes` | `true` | Add validation attributes (Required, StringLength, Range, RegularExpression) from schema constraints |
| `GenerateXmlDocumentation` | `true` | Generate XML documentation comments |
| `UseAsyncControllers` | `true` | Generate async controller methods |
| `AddApiControllerAttribute` | `true` | Add [ApiController] attribute to controllers |
| `ControllerBaseClass` | `ControllerBase` | Base class for generated controllers |
| `ControllerGroupingStrategy` | `ByTag` | Strategy for grouping operations into controllers: `ByTag` (group by OpenAPI tags), `ByFirstPathSegment` (group by first path segment), or `ByPath` (one controller per path) |

## Example

Given an OpenAPI specification:

```yaml
openapi: 3.1.0
info:
  title: My API
  version: 1.0.0
paths:
  /users/{id}:
    get:
      operationId: GetUser
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        '200':
          description: Success
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/User'
components:
  schemas:
    User:
      type: object
      required:
        - id
        - name
      properties:
        id:
          type: string
          format: uuid
        name:
          type: string
          maxLength: 100
```

The generator creates:

**Contract (DTO):**
```csharp
namespace Generated.Contracts;

/// <summary>
/// User
/// </summary>
public record User
{
    /// <summary>
    /// Gets or sets Id.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets Name.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
}
```

**Controller:**
```csharp
namespace Generated.Controllers;

[ApiController]
public abstract class UsersControllerBase : ControllerBase
{
    /// <summary>
    /// GetUser
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("/users/{id}")]
    public abstract Task<IActionResult> GetUser([FromRoute] Guid id);
}
```

## Requirements

- .NET 8, 9, 10+
- OpenAPI 3.x specification (YAML or JSON)

## Repository

https://github.com/flavien-chenu/openapi-generator

## License

MIT
