# OpenAPI Generator

[![CI/CD Pipeline](https://github.com/flavien-chenu/openapi-generator/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/flavien-chenu/openapi-generator/actions/workflows/ci-cd.yml)

Source generators for OpenAPI specifications in .NET projects.

## Overview

This repository provides C# source generators that create code from OpenAPI specifications at compile time. The generators integrate directly into your build process with no runtime overhead.

## Available Packages

### TeknixIT.OpenApiGenerator.Server

[![NuGet](https://img.shields.io/nuget/v/TeknixIT.OpenApiGenerator.Server.svg)](https://www.nuget.org/packages/TeknixIT.OpenApiGenerator.Server)

Generate ASP.NET Core server-side code from OpenAPI specifications.

**Features:**
- Contract (DTO) generation as records or classes
- Controller stubs with proper routing
- Validation attributes from schema constraints
- XML documentation from OpenAPI descriptions
- Async controller support
- Highly configurable

**Installation:**
```bash
dotnet add package TeknixIT.OpenApiGenerator.Server
```

**Documentation:** [Package README](./src/TeknixIT.OpenApiGenerator.Server/README.md)

## Requirements

- .NET 8, 9, 10+
- OpenAPI 3.x specification (YAML or JSON)

## Building

```bash
git clone https://github.com/flavien-chenu/openapi-generator.git
cd openapi-generator
dotnet build
```

## Testing

```bash
dotnet test
```

## Contributing

Contributions are welcome. Please open an issue before submitting a pull request.

## License

MIT

