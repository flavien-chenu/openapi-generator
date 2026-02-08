using Microsoft.CodeAnalysis;

namespace TeknixIT.OpenApiGenerator.Server.Tests;

/// <summary>
/// Tests for error handling and invalid input scenarios
/// </summary>
[TestFixture]
public class ErrorHandlingTests : TestBase
{
    [Test]
    public void InvalidOpenApiFile_ShouldReportError()
    {
        // Arrange
        var invalidFile = Path.Combine(TestDataDirectory, "invalid-api.yaml");
        File.WriteAllText(invalidFile, "this is not valid yaml: {[}]");

        var config = CreateDefaultConfiguration();

        try
        {
            // Act
            var result = RunGenerator(invalidFile, config);

            // Assert
            var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            Assert.That(errors, Is.Not.Empty, "Errors should be reported for an invalid file");
        }
        finally
        {
            // Cleanup
            if (File.Exists(invalidFile))
            {
                File.Delete(invalidFile);
            }
        }
    }

    [Test]
    public void NonExistentFile_ShouldReportError()
    {
        // Arrange
        var nonExistentFile = Path.Combine(TestDataDirectory, "does-not-exist.yaml");
        var config = CreateDefaultConfiguration();

        // Act
        var result = RunGenerator(nonExistentFile, config);

        // Assert
        var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.That(errors, Is.Not.Empty, "Errors should be reported for a non-existent file");
    }

    [Test]
    public void EmptyApi_ShouldNotGenerateErrorsOrFiles()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "empty-api.yaml");
        var config = CreateDefaultConfiguration();

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result, "No errors should be generated even for an empty API");
        Assert.That(result.GeneratedSources.Length, Is.EqualTo(0),
            "No files should be generated for an empty API");
    }
}
