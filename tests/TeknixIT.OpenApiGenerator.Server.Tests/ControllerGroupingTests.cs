using Microsoft.CodeAnalysis;

namespace TeknixIT.OpenApiGenerator.Server.Tests;

/// <summary>
/// Tests for controller grouping strategies (ByTag, ByPath, ByFirstPathSegment)
/// </summary>
[TestFixture]
public class ControllerGroupingTests : TestBase
{
    [Test]
    public void ControllerGroupingStrategy_ByTag_ShouldGroupByTags()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "grouping-api.yaml");
        var config = CreateDefaultConfiguration();
        config["build_metadata.AdditionalFiles.ControllerGroupingStrategy"] = "ByTag";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);

        var controllers = result.GeneratedSources.Where(s => s.HintName.EndsWith("Controller.g.cs")).ToArray();
        Assert.That(controllers.Length, Is.EqualTo(3), "Should have 3 controllers when grouping by tag");

        Assert.That(controllers.Any(s => s.HintName == "UsersController.g.cs"), Is.True,
            "UsersController should exist");
        Assert.That(controllers.Any(s => s.HintName == "ProductsController.g.cs"), Is.True,
            "ProductsController should exist");
        Assert.That(controllers.Any(s => s.HintName == "OrdersController.g.cs"), Is.True,
            "OrdersController should exist");

        var usersController = controllers.First(s => s.HintName == "UsersController.g.cs");
        var usersSource = usersController.SourceText.ToString();
        AssertContainsLine(usersSource, "ListUsers");
        AssertContainsLine(usersSource, "GetUser");
    }

    [Test]
    public void ControllerGroupingStrategy_ByFirstPathSegment_ShouldGroupByFirstSegment()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "grouping-api.yaml");
        var config = CreateDefaultConfiguration();
        config["build_metadata.AdditionalFiles.ControllerGroupingStrategy"] = "ByFirstPathSegment";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);

        var controllers = result.GeneratedSources.Where(s => s.HintName.EndsWith("Controller.g.cs")).ToArray();
        Assert.That(controllers.Length, Is.EqualTo(3),
            "Should have 3 controllers when grouping by first path segment");

        Assert.That(controllers.Any(s => s.HintName == "UsersController.g.cs"), Is.True,
            "UsersController should exist");
        Assert.That(controllers.Any(s => s.HintName == "ProductsController.g.cs"), Is.True,
            "ProductsController should exist");
        Assert.That(controllers.Any(s => s.HintName == "OrdersController.g.cs"), Is.True,
            "OrdersController should exist");

        var ordersController = controllers.First(s => s.HintName == "OrdersController.g.cs");
        var ordersSource = ordersController.SourceText.ToString();
        AssertContainsLine(ordersSource, "ListPendingOrders");
        AssertContainsLine(ordersSource, "ListCompletedOrders");
    }

    [Test]
    public void ControllerGroupingStrategy_ByPath_ShouldCreateSeparateControllers()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "grouping-api.yaml");
        var config = CreateDefaultConfiguration();
        config["build_metadata.AdditionalFiles.ControllerGroupingStrategy"] = "ByPath";

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);

        var controllers = result.GeneratedSources.Where(s => s.HintName.EndsWith("Controller.g.cs")).ToArray();
        Assert.That(controllers.Length, Is.EqualTo(4), "Should have 4 controllers when grouping by path");

        Assert.That(controllers.Any(s => s.HintName == "UsersController.g.cs"), Is.True,
            "UsersController should exist");
        Assert.That(controllers.Any(s => s.HintName == "ProductsController.g.cs"), Is.True,
            "ProductsController should exist");
        Assert.That(controllers.Any(s => s.HintName == "OrdersPendingController.g.cs"), Is.True,
            "OrdersPendingController should exist");
        Assert.That(controllers.Any(s => s.HintName == "OrdersCompletedController.g.cs"), Is.True,
            "OrdersCompletedController should exist");

        var usersController = controllers.First(s => s.HintName == "UsersController.g.cs");
        var usersSource = usersController.SourceText.ToString();
        AssertContainsLine(usersSource, "ListUsers");
        AssertContainsLine(usersSource, "GetUser");
    }

    [Test]
    public void ControllerGroupingStrategy_DefaultIsByTag()
    {
        // Arrange
        var openApiFile = Path.Combine(TestDataDirectory, "grouping-api.yaml");
        var config = CreateDefaultConfiguration();
        // Don't set ControllerGroupingStrategy - should default to ByTag

        // Act
        var result = RunGenerator(openApiFile, config);

        // Assert
        AssertNoErrors(result);

        var controllers = result.GeneratedSources.Where(s => s.HintName.EndsWith("Controller.g.cs")).ToArray();
        Assert.That(controllers.Length, Is.EqualTo(3), "Default grouping strategy should be ByTag");
    }
}
