namespace TeknixIT.OpenApiGenerator.Server.Tests;

/// <summary>
/// Tests for data type mappings from OpenAPI to C# types
/// </summary>
[TestFixture]
public class TypeMappingTests : TestBase
{
    [Test]
    public void TypesApi_ShouldMapAllPrimitiveTypes()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "types-api.yaml");
        var config = CreateDefaultConfiguration();

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var typesModel = GetGeneratedSource(result, "TypesModel.g.cs");
        var sourceText = typesModel.SourceText.ToString();

        // Verify all type mappings
        AssertContainsLinesInOrder(sourceText,
            "namespace TestApp.Contracts;",
            "public record TypesModel"
        );

        // UUID -> Guid
        Assert.That(sourceText, Does.Contain("public required Guid Id { get; set; }"),
            "UUID should map to Guid");

        // Integer (default) -> int
        Assert.That(sourceText, Does.Contain("public required int Count { get; set; }"),
            "Integer should map to int");

        // Integer (int64) -> long
        Assert.That(sourceText, Does.Contain("public required long BigCount { get; set; }"),
            "Integer with format int64 should map to long");

        // Number (default) -> decimal
        Assert.That(sourceText, Does.Contain("public required decimal Price { get; set; }"),
            "Number should map to decimal by default");

        // Number (float) -> float
        Assert.That(sourceText, Does.Contain("public required float Temperature { get; set; }"),
            "Number with format float should map to float");

        // Number (double) -> double
        Assert.That(sourceText, Does.Contain("public required double Weight { get; set; }"),
            "Number with format double should map to double");

        // Date -> DateOnly
        Assert.That(sourceText, Does.Contain("public required DateOnly CreatedDate { get; set; }"),
            "String with format date should map to DateOnly");

        // Date-time -> DateTime
        Assert.That(sourceText, Does.Contain("public required DateTime UpdatedAt { get; set; }"),
            "String with format date-time should map to DateTime");

        // Byte -> byte[]
        Assert.That(sourceText, Does.Contain("public required byte[] Data { get; set; }"),
            "String with format byte should map to byte[]");

        // Dictionary with string values
        Assert.That(sourceText, Does.Contain("public required IDictionary<string, string> Metadata { get; set; }"),
            "Object with additionalProperties of type string should map to IDictionary<string, string>");

        // Dictionary with integer values (not required, so nullable)
        Assert.That(sourceText, Does.Contain("public IDictionary<string, int>? Tags { get; set; }"),
            "Object with additionalProperties of type integer should map to IDictionary<string, int>? when not required");
    }

    [Test]
    public void TypesApi_WithXmlDocumentation_ShouldIncludeDescriptions()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "types-api.yaml");
        var config = CreateDefaultConfiguration();
        config["build_metadata.AdditionalFiles.GenerateXmlDocumentation"] = "true";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var typesModel = GetGeneratedSource(result, "TypesModel.g.cs");
        var sourceText = typesModel.SourceText.ToString();

        // Verify descriptions are included
        Assert.That(sourceText, Does.Contain("/// Unique identifier"),
            "UUID property should have description");
        Assert.That(sourceText, Does.Contain("/// Regular integer (int32)"),
            "Integer property should have description");
        Assert.That(sourceText, Does.Contain("/// Large integer (int64)"),
            "Int64 property should have description");
        Assert.That(sourceText, Does.Contain("/// Base64 encoded byte array"),
            "Byte array property should have description");
    }

    [Test]
    public void TypesApi_WithClass_ShouldGenerateClass()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "types-api.yaml");
        var config = CreateDefaultConfiguration();
        config["build_metadata.AdditionalFiles.UseRecords"] = "false";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var typesModel = GetGeneratedSource(result, "TypesModel.g.cs");
        var sourceText = typesModel.SourceText.ToString();

        Assert.That(sourceText, Does.Contain("public class TypesModel"),
            "Should generate class when UseRecords is false");
        Assert.That(sourceText, Does.Not.Contain("public record TypesModel"),
            "Should not generate record when UseRecords is false");
    }
}
