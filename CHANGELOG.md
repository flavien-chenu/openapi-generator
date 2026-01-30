# Changelog

All notable changes to Argon OpenAPI Generator will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-29

### Added
- Initial release
- Source generator for OpenAPI 3.1.1 schemas
- Support for DTO generation (records or classes)
- Support for ASP.NET Core controller generation
- Configurable via MSBuild properties
- Support for JSON and YAML OpenAPI files
- Validation attributes generation ([Required], [StringLength], [Range], [RegularExpression])
- XML documentation generation
- Support for async controllers
- Complete OpenAPI type mapping to C# types:
  - Primitive types (string, int, bool, decimal, etc.)
  - Format types (date, date-time, uuid, byte, etc.)
  - Arrays → List<T>
  - Objects → Dictionary or custom classes
  - Schema references ($ref)
- HTTP verb support (GET, POST, PUT, DELETE, PATCH, etc.)
- Parameter binding attributes ([FromRoute], [FromQuery], [FromHeader], [FromBody])
- ActionResult<T> return types
- Incremental generator for optimal performance
- Compatible with .NET 8, 9, 10+

### Configuration Options
- `ArgonOpenApi_GenerateDtos` - Enable/disable DTO generation
- `ArgonOpenApi_GenerateControllers` - Enable/disable controller generation
- `ArgonOpenApi_UseRecords` - Use records instead of classes for DTOs
- `ArgonOpenApi_BaseNamespace` - Base namespace for generated code
- `ArgonOpenApi_DtosNamespace` - Namespace for DTOs
- `ArgonOpenApi_ControllersNamespace` - Namespace for controllers
- `ArgonOpenApi_GenerateValidationAttributes` - Generate validation attributes
- `ArgonOpenApi_GenerateXmlDocumentation` - Generate XML documentation
- `ArgonOpenApi_UseAsyncControllers` - Use async methods in controllers
- `ArgonOpenApi_AddApiControllerAttribute` - Add [ApiController] attribute

### Technical Details
- Target framework: netstandard2.0
- Dependencies:
  - Microsoft.CodeAnalysis.CSharp 4.12.0
  - Microsoft.OpenApi 1.6.22
  - Microsoft.OpenApi.Readers 1.6.22
- NuGet package ready

## [Unreleased]

### Planned Features
- Support for polymorphism (oneOf, anyOf, allOf)
- HTTP client generation
- Authentication support (OAuth2, JWT, API Key)
- Webhook support (OpenAPI 3.1)
- FluentValidation generator
- Example request/response generation
- Support for discriminators
- Custom template support
- Multi-file OpenAPI support with references
