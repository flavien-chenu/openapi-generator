using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Argon.OpenApiGenerator.Controllers;
using Argon.OpenApiGenerator.Dtos;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;

namespace Argon.OpenApiGenerator;

/// <summary>
/// Générateur incrémental pour créer des DTOs et contrôleurs à partir de schémas OpenAPI.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class OpenApiSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. On récupère les fichiers et leurs métadonnées
        var provider = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select((pair, _) =>
            {
                var text = pair.Left;
                var options = pair.Right.GetOptions(text);

                // On filtre : on ne prend que nos items "OpenApiGenerator"
                if (!options.TryGetValue("build_metadata.AdditionalFiles.SourceItemType", out var type) || type != "OpenApiGenerator")
                    return null;

                return new { Text = text, Options = options };
            })
            .Where(x => x != null);

        // 2. On groupe tout pour créer une seule instance de GeneratorOptions
        var collectedConfigurations = provider.Collect().Select((items, _) =>
        {
            var configurations = new List<GeneratorConfiguration>();

            foreach (var item in items)
            {
                if (item is null) continue;
                configurations.Add(new GeneratorConfiguration
                {
                    OpenApiFile = item.Text.Path,
                    GenerateDtos = GetBool(item.Options, "GenerateDtos", true),
                    GenerateControllers = GetBool(item.Options, "GenerateControllers", true),
                    UseRecords = GetBool(item.Options, "UseRecords", true),
                    GenerateValidationAttributes = GetBool(item.Options, "GenerateValidationAttributes", true),
                    GenerateXmlDocumentation = GetBool(item.Options, "GenerateXmlDocumentation", true),
                    UseAsyncControllers = GetBool(item.Options, "UseAsyncControllers", true),
                    AddApiControllerAttribute = GetBool(item.Options, "AddApiControllerAttribute", true),
                    BaseNamespace = GetString(item.Options, "BaseNamespace", "Generated"),
                    DtosNamespace = GetString(item.Options, "DtosNamespace", "Dtos"),
                    ControllersNamespace = GetString(item.Options, "ControllersNamespace", "Controllers"),
                    ControllerBaseClass = GetString(item.Options, "ControllerBaseClass", "ControllerBase")
                });
            }
            return configurations;
        });

        // Génère le code
        context.RegisterSourceOutput(collectedConfigurations, (spc, configurations) =>
        {
            foreach (var configuration in configurations)
            {
                try
                {
                    GenerateFromConfiguration(spc, configuration);
                }
                catch (Exception ex)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "ARGON001",
                            "Erreur de génération OpenAPI",
                            $"Erreur lors du traitement de {configuration}: {ex.Message}",
                            "Argon.OpenApiGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.None));
                }
            }
        });
    }

    private static bool GetBool(AnalyzerConfigOptions options, string key, bool defaultValue)
    {
        return options.TryGetValue($"build_metadata.AdditionalFiles.{key}", out var s)
            ? bool.TryParse(s, out var b) ? b : defaultValue
            : defaultValue;
    }

    private static string GetString(AnalyzerConfigOptions options, string key, string defaultValue)
    {
        return options.TryGetValue($"build_metadata.AdditionalFiles.{key}", out var s) && !string.IsNullOrWhiteSpace(s)
            ? s
            : defaultValue;
    }

    private static void GenerateFromConfiguration(
        SourceProductionContext context,
        GeneratorConfiguration configuration)
    {
        var fileName = Path.GetFileNameWithoutExtension(configuration.OpenApiFile);

        // Parse le document OpenAPI
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();
        settings.AddJsonReader();

        var (document, diagnostic) = OpenApiDocument.LoadAsync(configuration.OpenApiFile, settings).GetAwaiter().GetResult();

        if (document == null || diagnostic != null && diagnostic.Errors.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "ARGON002",
                    "Document OpenAPI invalide",
                    $"Impossible de parser le document OpenAPI: {configuration.OpenApiFile}",
                    "Argon.OpenApiGenerator",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true),
                Location.None));
            return;
        }

        // Génère les DTOs
        if (configuration.GenerateDtos && document.Components?.Schemas != null)
        {
            var dtoGenerator = new DtoGenerator(configuration);
            dtoGenerator.Generate(document, context);
        }

        // Generate controllers
        if (!configuration.GenerateControllers) return;

        var controllerGenerator = new ControllerGenerator(configuration);
        controllerGenerator.Generate(document, context);
    }
}
