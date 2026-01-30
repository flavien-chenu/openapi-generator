# Guide d'utilisation du générateur Argon OpenAPI

## Résumé

Votre générateur de source OpenAPI est maintenant créé ! Il supporte :

- ✅ **OpenAPI 3.1.1** via Microsoft.OpenApi et Microsoft.OpenApi.Readers
- ✅ **Génération de DTOs** (records ou classes)
- ✅ **Génération de contrôleurs** ASP.NET Core
- ✅ **Configuration flexible** via propriétés MSBuild
- ✅ **Compatible .NET 8, 9, 10+**

## Structure créée

```
Argon.OpenApiGenerator/
├── Argon.OpenApiGenerator.csproj      # Projet du générateur (netstandard2.0)
├── OpenApiSourceGenerator.cs          # Générateur incrémental principal
├── GeneratorOptions.cs                # Options de configuration
├── OpenApiDocumentParser.cs           # Parseur OpenAPI 3.1.1
├── DtoGenerator.cs                    # Générateur de DTOs
├── ControllerGenerator.cs             # Générateur de contrôleurs
└── README.md                          # Documentation

Samples/Sample.Api/
├── Sample.Api.csproj                  # Projet exemple configuré
├── openapi.yaml                       # Schéma OpenAPI complet
└── test.yaml                          # Schéma OpenAPI simple pour test
```

## Test et débogage

Pour tester si le générateur fonctionne :

### 1. Vérifier la compilation du générateur

```bash
cd Argon.OpenApiGenerator
dotnet build
```

Le projet doit compiler sans erreur.

### 2. Tester avec le projet exemple

```bash
cd Samples/Sample.Api
dotnet clean
dotnet build -v detailed > build.log 2>&1
```

Cherchez dans `build.log` les messages contenant "ARGON" pour voir si le générateur s'exécute.

### 3. Vérifier les fichiers générés

Les fichiers générés se trouvent normalement dans :
```
Samples/Sample.Api/obj/Debug/net10.0/generated/Argon.OpenApiGenerator/Argon.OpenApiGenerator.OpenApiSourceGenerator/
```

Ou cherchez tous les fichiers `.g.cs` :
```powershell
Get-ChildItem -Path "Samples\Sample.Api\obj" -Filter "*.g.cs" -Recurse
```

## Prochaines étapes recommandées

### 1. Créer un package NuGet

Pour distribuer votre générateur, créez un package NuGet :

```xml
<!-- Ajoutez dans Argon.OpenApiGenerator.csproj -->
<PropertyGroup>
  <PackageId>Argon.OpenApiGenerator</PackageId>
  <Version>1.0.0</Version>
  <Authors>Votre Nom</Authors>
  <Description>Générateur de source pour OpenAPI 3.1.1 - Génère des DTOs et contrôleurs</Description>
  <PackageTags>openapi;source-generator;dto;controller;aspnetcore</PackageTags>
  <IncludeBuildOutput>false</IncludeBuildOutput>
  <DevelopmentDependency>true</DevelopmentDependency>
</PropertyGroup>

<ItemGroup>
  <None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
  <None Include="$(PKGMicrosoft_OpenApi)\lib\netstandard2.0\Microsoft.OpenApi.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
  <None Include="$(PKGMicrosoft_OpenApi_Readers)\lib\netstandard2.0\Microsoft.OpenApi.Readers.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
</ItemGroup>
```

Puis :
```bash
dotnet pack
```

### 2. Ajouter des tests unitaires

Créez un projet de tests pour le générateur :

```bash
dotnet new xunit -n Argon.OpenApiGenerator.Tests
cd Argon.OpenApiGenerator.Tests
dotnet add package Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing.XUnit
dotnet add reference ../Argon.OpenApiGenerator/Argon.OpenApiGenerator.csproj
```

### 3. Améliorer le générateur

Fonctionnalités supplémentaires possibles :
- Support des authentifications (OAuth, JWT, etc.)
- Génération de clients HTTP (similaire à Refit)
- Support des webhooks OpenAPI 3.1
- Génération de Swagger UI
- Support des polymorphismes (`oneOf`, `anyOf`, `allOf`)
- Génération de validators Fluent Validation
- Support des exemples OpenAPI

### 4. Documentation

Créez des exemples pour chaque cas d'usage :
- DTOs seulement
- Contrôleurs seulement
- Avec validation
- Avec documentation XML
- Multiples fichiers OpenAPI

## Dépannage

### Le générateur ne s'exécute pas

1. Vérifiez que le fichier OpenAPI est bien dans `AdditionalFiles`
2. Nettoyez et reconstruisez : `dotnet clean && dotnet build`
3. Vérifiez les logs de build avec `-v detailed`
4. Assurez-vous que le générateur est référencé avec `OutputItemType="Analyzer"`

### Erreurs de chargement d'assemblies

Si vous voyez des erreurs comme "Could not load file or assembly 'Microsoft.OpenApi'", vérifiez :
- Les packages sont marqués avec `GeneratePathProperty="true"`
- Le target `GetDependencyTargetPaths` est bien défini
- Les chemins dans `TargetPathWithTargetPlatformMoniker` sont corrects

### Les types générés ne sont pas visibles

1. Vérifiez que les namespaces correspondent à votre configuration
2. Reconstruisez le projet
3. Redémarrez l'IDE

## Configuration complète exemple

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    
    <!-- Générateur Argon OpenAPI -->
    <ArgonOpenApi_GenerateDtos>true</ArgonOpenApi_GenerateDtos>
    <ArgonOpenApi_GenerateControllers>true</ArgonOpenApi_GenerateControllers>
    <ArgonOpenApi_UseRecords>true</ArgonOpenApi_UseRecords>
    <ArgonOpenApi_BaseNamespace>MyApp</ArgonOpenApi_BaseNamespace>
    <ArgonOpenApi_DtosNamespace>Contracts</ArgonOpenApi_DtosNamespace>
    <ArgonOpenApi_ControllersNamespace>Api</ArgonOpenApi_ControllersNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Argon.OpenApiGenerator" Version="1.0.0" 
                      OutputItemType="Analyzer" 
                      ReferenceOutputAssembly="false" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include="Schemas\**\*.yaml" />
    <AdditionalFiles Include="Schemas\**\*.json" />
  </ItemGroup>
</Project>
```

## Ressources

- [Spécification OpenAPI 3.1](https://spec.openapis.org/oas/v3.1.0)
- [Source Generators (.NET)](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [Microsoft.OpenApi](https://github.com/microsoft/OpenAPI.NET)
- [ASP.NET Core Controllers](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
