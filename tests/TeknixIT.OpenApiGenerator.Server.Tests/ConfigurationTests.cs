namespace TeknixIT.OpenApiGenerator.Server.Tests;

/// <summary>
/// Tests for generator configuration options
/// </summary>
[TestFixture]
public class ConfigurationTests : TestBase
{
    [Test]
    public void Configuration_WithCustomNamespaces_ShouldUseCustomNamespaces()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "simple-api.yaml");
        var config = CreateDefaultConfiguration();
        config["BaseNamespace"] = "MyApp.Api";
        config["ContractsNamespace"] = "Models";
        config["ControllersNamespace"] = "Endpoints";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);

        var userContract = GetGeneratedSource(result, "User.g.cs");
        AssertContainsLinesInOrder(userContract.SourceText.ToString(),
            "namespace MyApp.Api.Models;",
            "public record User"
        );

        var controller = GetGeneratedSource(result, "UsersController.g.cs");
        AssertContainsLinesInOrder(controller.SourceText.ToString(),
            "using MyApp.Api.Models;",
            "namespace MyApp.Api.Endpoints;",
            "public abstract class UsersControllerBase"
        );
    }

    [Test]
    public void Configuration_WithGenerateControllersDisabled_ShouldOnlyGenerateContracts()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "simple-api.yaml");
        var config = CreateDefaultConfiguration();
        config["GenerateControllers"] = "false";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.GeneratedSources.Any(s => s.HintName == "User.g.cs"), Is.True,
                "Contracts should be generated");
            Assert.That(result.GeneratedSources.Any(s => s.HintName.EndsWith("Controller.g.cs")), Is.False,
                "Controllers should not be generated");
        }
    }

    [Test]
    public void Configuration_WithAsyncControllers_ShouldGenerateAsyncMethods()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "simple-api.yaml");
        var config = CreateDefaultConfiguration();
        config["UseAsyncControllers"] = "true";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var controller = GetGeneratedSource(result, "UsersController.g.cs");
        AssertContainsLine(controller.SourceText.ToString(), "Task<IActionResult>");
    }

    [Test]
    public void Configuration_WithSyncControllers_ShouldGenerateSyncMethods()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "simple-api.yaml");
        var config = CreateDefaultConfiguration();
        config["UseAsyncControllers"] = "false";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var controller = GetGeneratedSource(result, "UsersController.g.cs");
        var sourceText = controller.SourceText.ToString();
        AssertContainsLine(sourceText, "IActionResult GetUser(");
    }

    [Test]
    public void Configuration_WithApiControllerAttribute_ShouldAddAttribute()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "simple-api.yaml");
        var config = CreateDefaultConfiguration();
        config["AddApiControllerAttribute"] = "true";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var controller = GetGeneratedSource(result, "UsersController.g.cs");
        AssertContainsLine(controller.SourceText.ToString(), "[ApiController]");
    }

    [Test]
    public void Configuration_WithoutApiControllerAttribute_ShouldNotAddAttribute()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "simple-api.yaml");
        var config = CreateDefaultConfiguration();
        config["AddApiControllerAttribute"] = "false";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var controller = GetGeneratedSource(result, "UsersController.g.cs");
        var sourceText = controller.SourceText.ToString();
        Assert.That(sourceText.Trim().Replace(" ", ""), Does.Not.Contain("[ApiController]"),
            "[ApiController] attribute should not be present");
    }

    [Test]
    public void Configuration_WithCustomControllerBaseClass_ShouldInheritFromCustomClass()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "simple-api.yaml");
        var config = CreateDefaultConfiguration();
        config["ControllerBaseClass"] = "MyApp.Controllers.BaseApiController";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);
        var controller = GetGeneratedSource(result, "UsersController.g.cs");
        AssertContainsLine(controller.SourceText.ToString(), ": MyApp.Controllers.BaseApiController");
    }
}
