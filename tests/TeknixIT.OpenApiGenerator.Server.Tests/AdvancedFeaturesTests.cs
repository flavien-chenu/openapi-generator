namespace TeknixIT.OpenApiGenerator.Server.Tests;

/// <summary>
/// Tests for advanced OpenAPI features like nullable types
/// </summary>
[TestFixture]
public class AdvancedFeaturesTests : TestBase
{
    [Test]
    public void NullableTypes_WithOneOf_ShouldGenerateNullableProperties()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "advanced-features-api.yaml");
        var config = CreateDefaultConfiguration();

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var model = GetGeneratedSource(result, "NullableTypesModel.g.cs");
        var sourceText = model.SourceText.ToString();

        // Required properties should not be nullable
        Assert.That(sourceText, Does.Contain("public required string RequiredString { get; set; }"),
            "Required string should not be nullable");
        Assert.That(sourceText, Does.Contain("public required int RequiredInt { get; set; }"),
            "Required int should not be nullable");

        // oneOf with null should generate nullable types
        Assert.That(sourceText, Does.Contain("public string? NullableString { get; set; }"),
            "OneOf with null should generate nullable string");
        Assert.That(sourceText, Does.Contain("public int? NullableInt { get; set; }"),
            "OneOf with null should generate nullable int");

        // nullable: true should generate nullable types
        Assert.That(sourceText, Does.Contain("public decimal? NullableNumber { get; set; }"),
            "Nullable number should generate nullable decimal");
    }

    [Test]
    public void NullableProperties_ShouldNotHaveRequiredModifier()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "advanced-features-api.yaml");
        var config = CreateDefaultConfiguration();

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var model = GetGeneratedSource(result, "NullableTypesModel.g.cs");
        var sourceText = model.SourceText.ToString();

        // Verify nullable properties don't have 'required' modifier
        Assert.That(sourceText, Does.Not.Contain("public required string? NullableString"),
            "Nullable string should not have required modifier");
        Assert.That(sourceText, Does.Not.Contain("public required int? NullableInt"),
            "Nullable int should not have required modifier");
        Assert.That(sourceText, Does.Not.Contain("public required decimal? NullableNumber"),
            "Nullable number should not have required modifier");
    }

    [Test]
    public void RequiredProperties_ShouldHaveRequiredModifier()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "advanced-features-api.yaml");
        var config = CreateDefaultConfiguration();

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var model = GetGeneratedSource(result, "NullableTypesModel.g.cs");
        var sourceText = model.SourceText.ToString();

        // Verify required properties have 'required' modifier
        Assert.That(sourceText, Does.Contain("public required string RequiredString { get; set; }"),
            "Required string should have required modifier");
        Assert.That(sourceText, Does.Contain("public required int RequiredInt { get; set; }"),
            "Required int should have required modifier");
    }
}
