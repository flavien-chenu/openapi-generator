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

    [Test]
    public void ValidationApi_ShouldGenerateRegularExpressionAttribute()
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

        // Check for RegularExpression attribute with escaped pattern
        Assert.That(sourceText, Does.Contain("[RegularExpression(@\"^\\+?[1-9]\\d{1,14}$\")]"),
            "RegularExpression attribute should be present with correct pattern");

        AssertContainsLinesInOrder(sourceText,
            "using System.ComponentModel.DataAnnotations;",
            "/// International phone number in E.164 format",
            "[Required]",
            "[RegularExpression(@\"^\\+?[1-9]\\d{1,14}$\")]",
            "public required string PhoneNumber { get; set; }"
        );
    }

    [Test]
    public void ValidationApi_WithValidationDisabled_ShouldNotGenerateRegularExpressionAttribute()
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

        Assert.That(sourceText, Does.Not.Contain("[RegularExpression"),
            "RegularExpression attribute should not be present when validation is disabled");
    }
}
