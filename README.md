# Argon OpenAPI Generator

Générateur de source .NET pour créer automatiquement des DTOs et des contrôleurs à partir de schémas OpenAPI 3.1.1.

## 🚀 Caractéristiques

- ✅ Support complet d'**OpenAPI 3.1.1** (via `Microsoft.OpenApi`)
- ✅ Compatible avec **.NET 8, 9, 10+**
- ✅ Génération de **DTOs** (classes ou records)
- ✅ Génération de **Contrôleurs ASP.NET Core**
- ✅ **Générateur incrémental** pour des performances optimales
- ✅ **Hautement configurable** via des propriétés MSBuild
- ✅ Support des attributs de validation
- ✅ Génération de documentation XML
- ✅ Support des formats JSON et YAML

## 📦 Installation

### Option 1 : Via NuGet (quand publié)

```bash
dotnet add package Argon.OpenApiGenerator
```

### Option 2 : Référence locale

```xml
<ItemGroup>
  <ProjectReference Include="..\Argon.OpenApiGenerator\Argon.OpenApiGenerator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## 📖 Utilisation

### 1. Ajouter un fichier OpenAPI à votre projet

Placez votre fichier OpenAPI (`.json`, `.yaml`, ou `.yml`) dans votre projet et marquez-le comme `AdditionalFiles` :

```xml
<ItemGroup>
  <AdditionalFiles Include="openapi.json" />
</ItemGroup>
```

### 2. Configurer les options (optionnel)

Ajoutez des propriétés dans votre `.csproj` pour personnaliser la génération :

```xml
<PropertyGroup>
  <!-- Génération des DTOs (défaut: true) -->
  <ArgonOpenApi_GenerateDtos>true</ArgonOpenApi_GenerateDtos>
  
  <!-- Génération des contrôleurs (défaut: true) -->
  <ArgonOpenApi_GenerateControllers>true</ArgonOpenApi_GenerateControllers>
  
  <!-- Utiliser des records au lieu de classes (défaut: true) -->
  <ArgonOpenApi_UseRecords>true</ArgonOpenApi_UseRecords>
  
  <!-- Namespace de base (défaut: Generated) -->
  <ArgonOpenApi_BaseNamespace>MyApi</ArgonOpenApi_BaseNamespace>
  
  <!-- Namespace pour les DTOs (défaut: Dtos) -->
  <ArgonOpenApi_DtosNamespace>Models</ArgonOpenApi_DtosNamespace>
  
  <!-- Namespace pour les contrôleurs (défaut: Controllers) -->
  <ArgonOpenApi_ControllersNamespace>Controllers</ArgonOpenApi_ControllersNamespace>
  
  <!-- Générer les attributs de validation (défaut: true) -->
  <ArgonOpenApi_GenerateValidationAttributes>true</ArgonOpenApi_GenerateValidationAttributes>
  
  <!-- Générer la documentation XML (défaut: true) -->
  <ArgonOpenApi_GenerateXmlDocumentation>true</ArgonOpenApi_GenerateXmlDocumentation>
  
  <!-- Utiliser des contrôleurs async (défaut: true) -->
  <ArgonOpenApi_UseAsyncControllers>true</ArgonOpenApi_UseAsyncControllers>
  
  <!-- Ajouter [ApiController] (défaut: true) -->
  <ArgonOpenApi_AddApiControllerAttribute>true</ArgonOpenApi_AddApiControllerAttribute>
</PropertyGroup>
```

### 3. Build et utilisation

```bash
dotnet build
```

Le générateur créera automatiquement :
- `{FileName}.Dtos.g.cs` - Les DTOs générés
- `{FileName}.Controllers.g.cs` - Les contrôleurs générés

## 📝 Exemples

### Exemple 1 : DTOs uniquement

```xml
<PropertyGroup>
  <ArgonOpenApi_GenerateDtos>true</ArgonOpenApi_GenerateDtos>
  <ArgonOpenApi_GenerateControllers>false</ArgonOpenApi_GenerateControllers>
  <ArgonOpenApi_UseRecords>true</ArgonOpenApi_UseRecords>
</PropertyGroup>

<ItemGroup>
  <AdditionalFiles Include="api-schema.yaml" />
</ItemGroup>
```

### Exemple 2 : Contrôleurs uniquement avec classes

```xml
<PropertyGroup>
  <ArgonOpenApi_GenerateDtos>false</ArgonOpenApi_GenerateDtos>
  <ArgonOpenApi_GenerateControllers>true</ArgonOpenApi_GenerateControllers>
  <ArgonOpenApi_UseRecords>false</ArgonOpenApi_UseRecords>
</PropertyGroup>

<ItemGroup>
  <AdditionalFiles Include="openapi.json" />
</ItemGroup>
```

### Exemple 3 : Configuration complète

```xml
<PropertyGroup>
  <ArgonOpenApi_GenerateDtos>true</ArgonOpenApi_GenerateDtos>
  <ArgonOpenApi_GenerateControllers>true</ArgonOpenApi_GenerateControllers>
  <ArgonOpenApi_UseRecords>true</ArgonOpenApi_UseRecords>
  <ArgonOpenApi_BaseNamespace>MyCompany.Api</ArgonOpenApi_BaseNamespace>
  <ArgonOpenApi_DtosNamespace>Contracts</ArgonOpenApi_DtosNamespace>
  <ArgonOpenApi_ControllersNamespace>Endpoints</ArgonOpenApi_ControllersNamespace>
</PropertyGroup>

<ItemGroup>
  <AdditionalFiles Include="users-api.yaml" />
  <AdditionalFiles Include="products-api.json" />
</ItemGroup>
```

## 🔧 Schéma OpenAPI exemple

```yaml
openapi: 3.1.0
info:
  title: Sample API
  version: 1.0.0
paths:
  /users/{id}:
    get:
      summary: Get user by ID
      operationId: getUserById
      tags:
        - Users
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
        - username
        - email
      properties:
        id:
          type: string
          format: uuid
          description: Unique identifier
        username:
          type: string
          minLength: 3
          maxLength: 50
          description: Username
        email:
          type: string
          format: email
          description: Email address
        createdAt:
          type: string
          format: date-time
```

## 📊 Code généré

### DTO généré (avec records)

```csharp
// <auto-generated />
#nullable enable

using System;
using System.ComponentModel.DataAnnotations;

namespace Generated.Dtos;

/// <summary>
/// User model
/// </summary>
public record User
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    [Required]
    public string Email { get; set; }

    /// <summary>
    /// Created date
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
```

### Contrôleur généré

```csharp
// <auto-generated />
#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Generated.Controllers;

/// <summary>
/// Contrôleur pour Users
/// </summary>
[ApiController]
[Route("[controller]")]
public partial class UsersController : ControllerBase
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User identifier</param>
    [HttpGet("users/{id}")]
    public async Task<ActionResult<User>> GetUserById([FromRoute] Guid id)
    {
        // TODO: Implémenter la logique
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}
```

## 🛠️ Développement

### Build du projet

```bash
dotnet build
```

### Test avec un projet exemple

Voir le dossier `Samples/` pour des exemples d'utilisation.

## 📄 Licence

MIT License

## 🤝 Contributions

Les contributions sont les bienvenues ! N'hésitez pas à ouvrir une issue ou une PR.

## 🔗 Ressources

- [Spécification OpenAPI 3.1](https://spec.openapis.org/oas/v3.1.0)
- [Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [Microsoft.OpenApi](https://github.com/microsoft/OpenAPI.NET)
