using Microsoft.CodeAnalysis;

namespace TeknixIT.OpenApiGenerator.Server.Tests;

/// <summary>
/// Tests for validation attribute generation from OpenAPI specifications
/// </summary>
[TestFixture]
public class ValidationTests : TestBase
{
    [Test]
    public void ValidationApi_WithValidationEnabled_ShouldGenerateValidationAttributes()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "validation-api.yaml");
        var config = CreateDefaultConfiguration();
        config["build_metadata.AdditionalFiles.GenerateValidationAttributes"] = "true";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var validationModel = GetGeneratedSource(result, "ValidationModel.g.cs");
        var sourceText = validationModel.SourceText.ToString();

        AssertContainsLinesInOrder(sourceText,
            "using System.ComponentModel.DataAnnotations;",
            "namespace TestApp.Contracts;",
            "public record ValidationModel",
            "[StringLength(20, MinimumLength = 3)]",
            "public required string Username",
            "[Range(18, 120)]",
            "public required int Age"
        );
    }

    [Test]
    public void ValidationApi_WithValidationDisabled_ShouldNotGenerateValidationAttributes()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "validation-api.yaml");
        var config = CreateDefaultConfiguration();
        config["build_metadata.AdditionalFiles.GenerateValidationAttributes"] = "false";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var validationModel = GetGeneratedSource(result, "ValidationModel.g.cs");
        var sourceText = validationModel.SourceText.ToString();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sourceText.Trim().Replace(" ", ""), Does.Not.Contain("[MinLength"),
                    "MinLength attribute should not be present");
            Assert.That(sourceText.Trim().Replace(" ", ""), Does.Not.Contain("[MaxLength"),
                "MaxLength attribute should not be present");
        }
    }
}
