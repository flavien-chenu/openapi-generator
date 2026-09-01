using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using TeknixIT.OpenApiGenerator.Server.Contracts;
using TeknixIT.OpenApiGenerator.Server.Controllers;

namespace TeknixIT.OpenApiGenerator.Server;

/// <summary>
/// MSBuild task that generates C# contracts and controllers from OpenAPI specifications.
/// Runs BeforeTargets="CoreCompile" so generated files are visible to all source generators.
/// </summary>
public class OpenApiGeneratorTask : Task
{
    /// <summary>
    /// The OpenApiGeneratorServer items, each pointing to an OpenAPI file with metadata.
    /// </summary>
    [Required]
    public ITaskItem[] OpenApiFiles { get; set; } = [];

    /// <summary>
    /// Directory where generated .cs files will be written (typically $(IntermediateOutputPath)OpenApiGenerated\).
    /// </summary>
    [Required]
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// The list of generated .cs file paths, injected into the compilation by the .targets file.
    /// </summary>
    [Output]
    public ITaskItem[] GeneratedFiles { get; set; } = [];

    /// <inheritdoc />
    public override bool Execute()
    {
        Directory.CreateDirectory(OutputDirectory);

        var allGeneratedFiles = new List<ITaskItem>();

        foreach (var item in OpenApiFiles)
        {
            try
            {
                var configuration = BuildConfiguration(item);
                foreach (var file in GenerateFromConfiguration(configuration))
                {
                    allGeneratedFiles.Add(new TaskItem(file));
                }
            }
            catch (Exception ex)
            {
                Log.LogError(
                    subcategory: null,
                    errorCode: "TEKX0001",
                    helpKeyword: null,
                    file: item.ItemSpec,
                    lineNumber: 0,
                    columnNumber: 0,
                    endLineNumber: 0,
                    endColumnNumber: 0,
                    message: "Error during OpenAPI processing for '{0}': {1}",
                    messageArgs: [item.ItemSpec, ex.Message]);
            }
        }

        GeneratedFiles = allGeneratedFiles.ToArray();
        return !Log.HasLoggedErrors;
    }

    private GeneratorConfiguration BuildConfiguration(ITaskItem item)
    {
        return new GeneratorConfiguration
        {
            OpenApiFile = item.ItemSpec,
            OutputDirectory = OutputDirectory,
            GenerateControllers = GetBooleanMetadata(item, "GenerateControllers", true),
            UseRecords = GetBooleanMetadata(item, "UseRecords", true),
            GenerateValidationAttributes = GetBooleanMetadata(item, "GenerateValidationAttributes", true),
            GenerateXmlDocumentation = GetBooleanMetadata(item, "GenerateXmlDocumentation", true),
            UseAsyncControllers = GetBooleanMetadata(item, "UseAsyncControllers", true),
            AddApiControllerAttribute = GetBooleanMetadata(item, "AddApiControllerAttribute", true),
            BaseNamespace = GetStringMetadata(item, "BaseNamespace", Constants.Defaults.BaseNamespace),
            ContractsNamespace = GetStringMetadata(item, "ContractsNamespace", Constants.Defaults.ContractsNamespace),
            ControllersNamespace = GetStringMetadata(item, "ControllersNamespace", Constants.Defaults.ControllersNamespace),
            ControllerBaseClass = GetStringMetadata(item, "ControllerBaseClass", Constants.Defaults.ControllerBaseClass),
            ControllerGroupingStrategy = GetEnumMetadata(item, "ControllerGroupingStrategy", ControllerGroupingStrategy.ByTag),
        };
    }

    private IEnumerable<string> GenerateFromConfiguration(GeneratorConfiguration configuration)
    {
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();
        settings.AddJsonReader();

        var (document, diagnostic) = OpenApiDocument.LoadAsync(configuration.OpenApiFile, settings)
            .GetAwaiter()
            .GetResult();

        if (document == null || (diagnostic != null && diagnostic.Errors.Any()))
        {
            Log.LogError(
                subcategory: null,
                errorCode: "TEKX0002",
                helpKeyword: null,
                file: configuration.OpenApiFile,
                lineNumber: 0,
                columnNumber: 0,
                endLineNumber: 0,
                endColumnNumber: 0,
                message: "Unable to parse OpenAPI document: {0}.\nDetails:\n\t{1}",
                messageArgs:
                [
                    configuration.OpenApiFile,
                    diagnostic != null ? string.Join("\n\t", diagnostic.Errors.Select(e => $"{e.Pointer}: {e.Message}")) : "No diagnostic information available."
                ]
            );
            return [];
        }

        var generatedFiles = new List<string>();

        if (document.Components?.Schemas != null)
        {
            var contractGenerator = new ContractGenerator(configuration);
            generatedFiles.AddRange(contractGenerator.Generate(document));
        }

        if (configuration.GenerateControllers)
        {
            var controllerGenerator = new ControllerGenerator(configuration);
            generatedFiles.AddRange(controllerGenerator.Generate(document));
        }

        return generatedFiles;
    }

    private static bool GetBooleanMetadata(ITaskItem item, string name, bool defaultValue)
    {
        var value = item.GetMetadata(name);
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    private static string GetStringMetadata(ITaskItem item, string name, string defaultValue)
    {
        var value = item.GetMetadata(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static TEnum GetEnumMetadata<TEnum>(ITaskItem item, string name, TEnum defaultValue)
        where TEnum : struct
    {
        var value = item.GetMetadata(name);
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        return Enum.TryParse<TEnum>(value, true, out var result) ? result : defaultValue;
    }
}
