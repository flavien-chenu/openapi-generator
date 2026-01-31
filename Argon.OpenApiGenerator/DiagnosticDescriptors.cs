using Microsoft.CodeAnalysis;
namespace Argon.OpenApiGenerator;
/// <summary>
/// Diagnostic descriptors for the OpenAPI source generator.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "Argon.OpenApiGenerator";
    public static readonly DiagnosticDescriptor GenerationError = new(
        id: "ARGON001",
        title: "OpenAPI Generation Error",
        messageFormat: "Error during OpenAPI processing for ''{0}'': {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An error occurred while generating code from the OpenAPI specification.");
    public static readonly DiagnosticDescriptor InvalidDocument = new(
        id: "ARGON002",
        title: "Invalid OpenAPI Document",
        messageFormat: "Unable to parse OpenAPI document: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The OpenAPI document could not be parsed or is invalid.");
    public static readonly DiagnosticDescriptor ConfigurationWarning = new(
        id: "ARGON003",
        title: "Configuration Warning",
        messageFormat: "Configuration issue: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A configuration issue was detected that may affect code generation.");
}