using System;
using System.Collections.Generic;
using System.Linq;
using TeknixIT.OpenApiGenerator.Server.Controllers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using TeknixIT.OpenApiGenerator.Server.Contracts;

namespace TeknixIT.OpenApiGenerator.Server;

/// <summary>
/// Incremental source generator that creates DTOs and controllers from OpenAPI specifications.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class OpenApiSourceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Retrieve additional files and their metadata
        var provider = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select((pair, _) =>
            {
                var text = pair.Left;
                var options = pair.Right.GetOptions(text);

                // Filter: only process items marked as "OpenApiGenerator"
                if (!options.TryGetValue(Constants.MetadataKeys.SourceItemType, out var type)
                    || type != Constants.Defaults.SourceItemTypeValue)
                {
                    return null;
                }

                return new { Text = text, Options = options };
            })
            .Where(x => x != null);

        // Step 2: Collect all configurations
        var collectedConfigurations = provider.Collect().Select((items, _) =>
        {
            var configurations = new List<GeneratorConfiguration>();

            foreach (var item in items)
            {
                if (item is null)
                    continue;

                configurations.Add(new GeneratorConfiguration
                {
                    OpenApiFile = item.Text.Path,
                    GenerateContracts = GetBooleanOption(item.Options, Constants.MetadataKeys.GenerateContracts, true),
                    GenerateControllers = GetBooleanOption(item.Options, Constants.MetadataKeys.GenerateControllers, true),
                    UseRecords = GetBooleanOption(item.Options, Constants.MetadataKeys.UseRecords, true),
                    GenerateValidationAttributes = GetBooleanOption(item.Options, Constants.MetadataKeys.GenerateValidationAttributes, true),
                    GenerateXmlDocumentation = GetBooleanOption(item.Options, Constants.MetadataKeys.GenerateXmlDocumentation, true),
                    UseAsyncControllers = GetBooleanOption(item.Options, Constants.MetadataKeys.UseAsyncControllers, true),
                    AddApiControllerAttribute = GetBooleanOption(item.Options, Constants.MetadataKeys.AddApiControllerAttribute, true),
                    BaseNamespace = GetStringOption(item.Options, Constants.MetadataKeys.BaseNamespace, Constants.Defaults.BaseNamespace),
                    ContractsNamespace = GetStringOption(item.Options, Constants.MetadataKeys.ContractsNamespace, Constants.Defaults.ContractsNamespace),
                    ControllersNamespace = GetStringOption(item.Options, Constants.MetadataKeys.ControllersNamespace, Constants.Defaults.ControllersNamespace),
                    ControllerBaseClass = GetStringOption(item.Options, Constants.MetadataKeys.ControllerBaseClass, Constants.Defaults.ControllerBaseClass)
                });
            }

            return configurations;
        });

        // Step 3: Generate source code
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
                        DiagnosticDescriptors.GenerationError,
                        Location.None,
                        configuration.OpenApiFile,
                        ex.Message));
                }
            }
        });
    }

    #region Helper Methods

    /// <summary>
    /// Gets a boolean configuration option value.
    /// </summary>
    /// <param name="options">The analyzer configuration options.</param>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value if the key is not found or invalid.</param>
    /// <returns>The boolean configuration value.</returns>
    private static bool GetBooleanOption(AnalyzerConfigOptions options, string key, bool defaultValue)
    {
        if (!options.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets a string configuration option value.
    /// </summary>
    /// <param name="options">The analyzer configuration options.</param>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value if the key is not found or empty.</param>
    /// <returns>The string configuration value.</returns>
    private static string GetStringOption(AnalyzerConfigOptions options, string key, string defaultValue)
    {
        return options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : defaultValue;
    }

    #endregion

    #region Code Generation

    /// <summary>
    /// Generates source code from a configuration.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="configuration">The generator configuration.</param>
    private static void GenerateFromConfiguration(
        SourceProductionContext context,
        GeneratorConfiguration configuration)
    {
        // Parse the OpenAPI document
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();
        settings.AddJsonReader();

        var (document, diagnostic) = OpenApiDocument.LoadAsync(configuration.OpenApiFile, settings)
            .GetAwaiter()
            .GetResult();

        if (document == null || (diagnostic != null && diagnostic.Errors.Any()))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.InvalidDocument,
                Location.None,
                configuration.OpenApiFile));
            return;
        }

        // Generate DTOs
        if (configuration.GenerateContracts && document.Components?.Schemas != null)
        {
            var dtoGenerator = new ContractGenerator(configuration);
            dtoGenerator.Generate(document, context);
        }

        // Generate Controllers
        if (configuration.GenerateControllers)
        {
            var controllerGenerator = new ControllerGenerator(configuration);
            controllerGenerator.Generate(document, context);
        }
    }

    #endregion
}
