namespace TeknixIT.OpenApiGenerator.Server.Tests;

/// <summary>
/// Tests for oneOf/allOf/discriminator polymorphism support (Rules 1-6)
/// </summary>
[TestFixture]
public class PolymorphismTests : TestBase
{
    private GeneratorRunResult _result = null!;

    [SetUp]
    public void Setup()
    {
        var openApiFile = Path.Combine(TestDataDirectory, "polymorphism-api.yaml");
        var config = CreateDefaultConfiguration();
        _result = RunGenerator(openApiFile, config);
        AssertNoErrors(_result);
    }

    #region Rule 1 — oneOf + discriminator → abstract record with STJ attributes

    [Test]
    public void OneOfDiscriminator_ShouldGenerateAbstractRecord()
    {
        var source = GetGeneratedSource(_result, "CreateCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        AssertContainsLine(text, "public abstract record CreateCategoryAttributeRequest : CategoryAttributeCreateBase;");
    }

    [Test]
    public void OneOfDiscriminator_ShouldHaveJsonPolymorphicAttribute()
    {
        var source = GetGeneratedSource(_result, "CreateCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        AssertContainsLine(text, "[JsonPolymorphic(TypeDiscriminatorPropertyName = \"type\")]");
    }

    [Test]
    public void OneOfDiscriminator_ShouldHaveJsonDerivedTypeAttributes()
    {
        var source = GetGeneratedSource(_result, "CreateCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        AssertContainsLine(text, "[JsonDerivedType(typeof(CreateTextCategoryAttributeRequest), \"text\")]");
        AssertContainsLine(text, "[JsonDerivedType(typeof(CreateIntegerCategoryAttributeRequest), \"integer\")]");
    }

    [Test]
    public void OneOfDiscriminator_ShouldIncludeStjUsing()
    {
        var source = GetGeneratedSource(_result, "CreateCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        AssertContainsLine(text, "using System.Text.Json.Serialization;");
    }

    [Test]
    public void OneOfDiscriminator_ShouldNotHaveProperties()
    {
        var source = GetGeneratedSource(_result, "CreateCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        // Abstract record should not contain property declarations
        Assert.That(text, Does.Not.Contain("{ get; set; }"),
            "Abstract discriminated record should not have properties");
    }

    [Test]
    public void OneOfDiscriminator_AttributesShouldAppearInCorrectOrder()
    {
        var source = GetGeneratedSource(_result, "CreateCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        AssertContainsLinesInOrder(text,
            "[JsonPolymorphic(TypeDiscriminatorPropertyName = \"type\")]",
            "[JsonDerivedType(typeof(CreateTextCategoryAttributeRequest), \"text\")]",
            "[JsonDerivedType(typeof(CreateIntegerCategoryAttributeRequest), \"integer\")]",
            "[GeneratedCode(",
            "public abstract record CreateCategoryAttributeRequest : CategoryAttributeCreateBase;");
    }

    #endregion

    #region Rule 2 — allOf with inheritance → sealed record

    [Test]
    public void AllOfInheritance_TextType_ShouldInheritFromAbstractParent()
    {
        var source = GetGeneratedSource(_result, "CreateTextCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        AssertContainsLine(text, "public sealed record CreateTextCategoryAttributeRequest : CreateCategoryAttributeRequest");
    }

    [Test]
    public void AllOfInheritance_IntegerType_ShouldInheritFromAbstractParent()
    {
        var source = GetGeneratedSource(_result, "CreateIntegerCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        AssertContainsLine(text, "public sealed record CreateIntegerCategoryAttributeRequest : CreateCategoryAttributeRequest");
    }

    [Test]
    public void AllOfInheritance_ShouldGenerateOwnProperties()
    {
        var source = GetGeneratedSource(_result, "CreateTextCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        // Inline fragment properties should be present
        AssertContainsLine(text, "public string? DefaultValue { get; set; }");
        AssertContainsLine(text, "public string? Pattern { get; set; }");
        AssertContainsLine(text, "public ICollection<string>? AllowedValues { get; set; }");
    }

    [Test]
    public void AllOfInheritance_IntegerType_ShouldGenerateOwnProperties()
    {
        var source = GetGeneratedSource(_result, "CreateIntegerCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        AssertContainsLine(text, "public int? MinValue { get; set; }");
        AssertContainsLine(text, "public int? MaxValue { get; set; }");
    }

    [Test]
    public void AllOfInheritance_ShouldNotDuplicateInheritedProperties()
    {
        var source = GetGeneratedSource(_result, "CreateTextCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        // Properties from CategoryAttributeCreateBase should NOT be in the concrete type
        Assert.That(text, Does.Not.Contain("Code { get; set; }"),
            "Inherited property 'Code' should not be duplicated in concrete record");
        Assert.That(text, Does.Not.Contain("Label { get; set; }"),
            "Inherited property 'Label' should not be duplicated in concrete record");
        Assert.That(text, Does.Not.Contain("IsRequired { get; set; }"),
            "Inherited property 'IsRequired' should not be duplicated in concrete record");
    }

    [Test]
    public void AllOfInheritance_ShouldNotIncludeStjUsing()
    {
        var source = GetGeneratedSource(_result, "CreateTextCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        // Concrete types don't need STJ using — only the abstract root does
        Assert.That(text, Does.Not.Contain("System.Text.Json.Serialization"),
            "Concrete inherited record should not have STJ using");
    }

    #endregion

    #region Rule 3 — Base schema → inheritable record

    [Test]
    public void BaseSchema_ShouldGenerateOrdinaryRecord()
    {
        var source = GetGeneratedSource(_result, "CategoryAttributeCreateBase.g.cs");
        var text = source.SourceText.ToString();

        // Should be a regular non-sealed, non-abstract record
        AssertContainsLine(text, "public record CategoryAttributeCreateBase()");

        // Should NOT be abstract
        Assert.That(text, Does.Not.Contain("abstract"),
            "Base schema without oneOf should not be abstract");

        // Should NOT be sealed
        Assert.That(text, Does.Not.Contain("sealed"),
            "Base schema should not be sealed — it must be inheritable");
    }

    [Test]
    public void BaseSchema_ShouldHaveItsProperties()
    {
        var source = GetGeneratedSource(_result, "CategoryAttributeCreateBase.g.cs");
        var text = source.SourceText.ToString();

        AssertContainsLine(text, "public required string Code { get; set; }");
        AssertContainsLine(text, "public required string Label { get; set; }");
        AssertContainsLine(text, "public required bool IsRequired { get; set; }");
    }

    #endregion

    #region Common base properties accessible via abstract parent

    [Test]
    public void AbstractRecord_ShouldInheritFromCommonBase()
    {
        var source = GetGeneratedSource(_result, "CreateCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        // Abstract record inherits from common allOf base
        AssertContainsLine(text, ": CategoryAttributeCreateBase");
    }

    [Test]
    public void AbstractRecord_CommonBaseProperties_NotDuplicatedInAbstract()
    {
        var source = GetGeneratedSource(_result, "CreateCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        // The abstract record inherits properties via the base class, not by redeclaring them
        Assert.That(text, Does.Not.Contain("{ get; set; }"),
            "Abstract record should not declare properties directly — they come from the base class");
    }

    #endregion

    #region Rule 4 — Inheritance chain: Concrete → Abstract (oneOf root), not → Base

    [Test]
    public void InheritanceChain_ConcreteInheritsFromOneOfRoot_NotFromBase()
    {
        var source = GetGeneratedSource(_result, "CreateTextCategoryAttributeRequest.g.cs");
        var text = source.SourceText.ToString();

        // Should inherit from the oneOf abstract type, NOT from CategoryAttributeCreateBase
        AssertContainsLine(text, ": CreateCategoryAttributeRequest");
        Assert.That(text, Does.Not.Contain(": CategoryAttributeCreateBase"),
            "Concrete type should inherit from oneOf root, not from allOf $ref base");
    }

    #endregion

    #region Rule 5 — Discriminant property excluded

    [Test]
    public void DiscriminantProperty_ShouldBeExcluded()
    {
        var textSource = GetGeneratedSource(_result, "CreateTextCategoryAttributeRequest.g.cs");
        var textText = textSource.SourceText.ToString();

        // The 'type' property with enum: [text] should NOT appear as a C# property
        Assert.That(textText, Does.Not.Contain("Type { get; set; }"),
            "Discriminant property 'type' should not be generated as a C# property");

        var intSource = GetGeneratedSource(_result, "CreateIntegerCategoryAttributeRequest.g.cs");
        var intText = intSource.SourceText.ToString();

        Assert.That(intText, Does.Not.Contain("Type { get; set; }"),
            "Discriminant property 'type' should not be generated as a C# property");
    }

    #endregion

    #region Rule 6 — Controller uses abstract type for [FromBody]

    [Test]
    public void Controller_ShouldUseAbstractTypeForRequestBody()
    {
        var controller = GetGeneratedSource(_result, "CategoriesController.g.cs");
        var text = controller.SourceText.ToString();

        AssertContainsLine(text, "[FromBody] CreateCategoryAttributeRequest request");
    }

    [Test]
    public void Controller_GetEndpoint_ShouldIncludePathLevelParameter()
    {
        var controller = GetGeneratedSource(_result, "CategoriesController.g.cs");
        var text = controller.SourceText.ToString();

        // Path-level parameter categoryId must appear in GET method signature
        AssertContainsLine(text, "public abstract Task<IActionResult> GetCategory([FromRoute] Guid categoryId);");
    }

    [Test]
    public void Controller_PostEndpoint_ShouldIncludePathLevelParameterAndBody()
    {
        var controller = GetGeneratedSource(_result, "CategoriesController.g.cs");
        var text = controller.SourceText.ToString();

        // Path-level parameter categoryId + body parameter must both appear
        AssertContainsLine(text, "public abstract Task<IActionResult> AddCategoryAttribute([FromRoute] Guid categoryId, [FromBody] CreateCategoryAttributeRequest request);");
    }

    #endregion

    #region All generated files check

    [Test]
    public void ShouldGenerateAllExpectedFiles()
    {
        var fileNames = _result.GeneratedSources.Select(s => s.HintName).ToList();

        Assert.That(fileNames, Does.Contain("CategoryAttributeCreateBase.g.cs"));
        Assert.That(fileNames, Does.Contain("CreateCategoryAttributeRequest.g.cs"));
        Assert.That(fileNames, Does.Contain("CreateTextCategoryAttributeRequest.g.cs"));
        Assert.That(fileNames, Does.Contain("CreateIntegerCategoryAttributeRequest.g.cs"));
        Assert.That(fileNames, Does.Contain("CategoriesController.g.cs"));
    }

    #endregion
}
