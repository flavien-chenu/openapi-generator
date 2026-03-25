using System.Reflection;
using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using TeknixIT.OpenApiGenerator.Server.Contracts;
using TeknixIT.OpenApiGenerator.Server.Controllers;

namespace TeknixIT.OpenApiGenerator.Server.Tests;

/// <summary>
/// Base class for all OpenAPI generator tests, providing common utilities and helpers
/// </summary>
public abstract class TestBase
{
    protected string TestDataDirectory { get; private set; } = string.Empty;

    [SetUp]
    public void BaseSetup()
    {
        var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (testDirectory == null)
        {
            throw new InvalidOperationException("Unable to determine test directory");
        }
        TestDataDirectory = Path.Combine(testDirectory, "TestData");

        if (!Directory.Exists(TestDataDirectory))
        {
            Directory.CreateDirectory(TestDataDirectory);
        }
    }

    #region Generator Execution

    protected static Dictionary<string, string> CreateDefaultConfiguration()
    {
        return new Dictionary<string, string>
        {
            ["GenerateControllers"] = "true",
            ["BaseNamespace"] = "TestApp",
            ["ContractsNamespace"] = "Contracts",
            ["ControllersNamespace"] = "Controllers",
            ["UseAsyncControllers"] = "true",
            ["GenerateValidationAttributes"] = "true",
            ["AddApiControllerAttribute"] = "true",
            ["ControllerBaseClass"] = "ControllerBase",
            ["UseRecords"] = "true",
            ["GenerateXmlDocumentation"] = "true"
        };
    }

    protected static GeneratorRunResult RunGenerator(string openApiFile, Dictionary<string, string> config)
    {
        var outputDir = Path.Combine(Path.GetTempPath(), "openapi-gen-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDir);

        try
        {
            var configuration = new GeneratorConfiguration
            {
                OpenApiFile = openApiFile,
                OutputDirectory = outputDir,
                GenerateControllers = GetBool(config, "GenerateControllers", true),
                UseRecords = GetBool(config, "UseRecords", true),
                GenerateValidationAttributes = GetBool(config, "GenerateValidationAttributes", true),
                GenerateXmlDocumentation = GetBool(config, "GenerateXmlDocumentation", true),
                UseAsyncControllers = GetBool(config, "UseAsyncControllers", true),
                AddApiControllerAttribute = GetBool(config, "AddApiControllerAttribute", true),
                BaseNamespace = GetString(config, "BaseNamespace", Constants.Defaults.BaseNamespace),
                ContractsNamespace = GetString(config, "ContractsNamespace", Constants.Defaults.ContractsNamespace),
                ControllersNamespace = GetString(config, "ControllersNamespace", Constants.Defaults.ControllersNamespace),
                ControllerBaseClass = GetString(config, "ControllerBaseClass", Constants.Defaults.ControllerBaseClass),
                ControllerGroupingStrategy = GetEnum(config, "ControllerGroupingStrategy", ControllerGroupingStrategy.ByTag),
            };

            var settings = new OpenApiReaderSettings();
            settings.AddYamlReader();
            settings.AddJsonReader();

            var (document, diagnostic) = OpenApiDocument.LoadAsync(openApiFile, settings)
                .GetAwaiter()
                .GetResult();

            if (document == null || (diagnostic != null && diagnostic.Errors.Any()))
            {
                return new GeneratorRunResult
                {
                    Errors = [$"TEKX0002: Unable to parse OpenAPI document: {openApiFile}"]
                };
            }

            var generatedFiles = new List<GeneratedFile>();

            if (document.Components?.Schemas != null)
            {
                var contractGenerator = new ContractGenerator(configuration);
                foreach (var file in contractGenerator.Generate(document))
                {
                    generatedFiles.Add(new GeneratedFile(Path.GetFileName(file), File.ReadAllText(file)));
                }
            }

            if (configuration.GenerateControllers)
            {
                var controllerGenerator = new ControllerGenerator(configuration);
                foreach (var file in controllerGenerator.Generate(document))
                {
                    generatedFiles.Add(new GeneratedFile(Path.GetFileName(file), File.ReadAllText(file)));
                }
            }

            return new GeneratorRunResult { GeneratedSources = generatedFiles };
        }
        catch (Exception ex)
        {
            return new GeneratorRunResult
            {
                Errors = [$"TEKX0001: Error during OpenAPI processing for '{openApiFile}': {ex.Message}"],
                Exception = ex
            };
        }
        finally
        {
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, recursive: true);
        }
    }

    private static bool GetBool(Dictionary<string, string> config, string key, bool defaultValue)
    {
        return config.TryGetValue(key, out var value) && bool.TryParse(value, out var result) ? result : defaultValue;
    }

    private static string GetString(Dictionary<string, string> config, string key, string defaultValue)
    {
        return config.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
    }

    private static TEnum GetEnum<TEnum>(Dictionary<string, string> config, string key, TEnum defaultValue)
        where TEnum : struct
    {
        return config.TryGetValue(key, out var value) && Enum.TryParse<TEnum>(value, true, out var result) ? result : defaultValue;
    }

    #endregion

    #region Assertion Helpers

    /// <summary>
    /// Verifies that a line of code exists in the source, ignoring leading whitespace
    /// </summary>
    protected static void AssertContainsLine(string source, string expectedLine)
    {
        var lines = source.Split('\n')
            .Select(line => line.TrimStart())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var normalizedExpected = expectedLine.TrimStart();

        var found = lines.Any(line => line.Contains(normalizedExpected));

        Assert.That(found, Is.True,
            $"Expected line not found:\n" +
            $"Expected: {normalizedExpected}\n" +
            $"In source:\n{source}");
    }

    /// <summary>
    /// Compares two source files line by line, ignoring leading/trailing whitespace differences
    /// </summary>
    protected static void AssertSourceEquals(string actualSource, string expectedSource, string fileName = "")
    {
        var actualLines = actualSource.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var expectedLines = expectedSource.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (actualLines.Count != expectedLines.Count)
        {
            var diff = new StringBuilder();
            diff.AppendLine($"Line count mismatch in {fileName}:");
            diff.AppendLine($"Expected: {expectedLines.Count} lines");
            diff.AppendLine($"Actual: {actualLines.Count} lines");
            diff.AppendLine();
            diff.AppendLine("Expected content:");
            diff.AppendLine(expectedSource);
            diff.AppendLine();
            diff.AppendLine("Actual content:");
            diff.AppendLine(actualSource);

            Assert.Fail(diff.ToString());
        }

        for (int i = 0; i < expectedLines.Count; i++)
        {
            if (actualLines[i] == expectedLines[i])
            {
                continue;
            }

            var diff = new StringBuilder();
            diff.AppendLine($"Line mismatch at line {i + 1} in {fileName}:");
            diff.AppendLine($"Expected: {expectedLines[i]}");
            diff.AppendLine($"Actual:   {actualLines[i]}");
            diff.AppendLine();
            diff.AppendLine($"Context (lines {Math.Max(0, i - 2)} to {Math.Min(expectedLines.Count - 1, i + 2)}):");
            for (int j = Math.Max(0, i - 2); j <= Math.Min(expectedLines.Count - 1, i + 2); j++)
            {
                string marker = j == i ? ">>> " : "    ";
                diff.AppendLine($"{marker}{j + 1}: {(j < actualLines.Count ? actualLines[j] : "[missing]")}");
            }

            Assert.Fail(diff.ToString());
        }
    }

    /// <summary>
    /// Verifies that multiple lines appear in order in the source, ignoring leading whitespace
    /// </summary>
    protected static void AssertContainsLinesInOrder(string source, params string[] expectedLines)
    {
        var lines = source.Split('\n')
            .Select(line => line.TrimStart())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var normalizedExpected = expectedLines.Select(line => line.TrimStart()).ToArray();

        int currentLineIndex = 0;
        int expectedIndex = 0;

        while (currentLineIndex < lines.Count && expectedIndex < normalizedExpected.Length)
        {
            if (lines[currentLineIndex].Contains(normalizedExpected[expectedIndex]))
            {
                expectedIndex++;
            }
            currentLineIndex++;
        }

        Assert.That(expectedIndex, Is.EqualTo(normalizedExpected.Length),
            $"Expected lines not found in order:\n" +
            $"Found {expectedIndex} of {normalizedExpected.Length} expected lines\n" +
            $"Missing line: {(expectedIndex < normalizedExpected.Length ? normalizedExpected[expectedIndex] : "none")}\n" +
            $"Expected lines:\n{string.Join("\n", normalizedExpected)}\n" +
            $"In source:\n{source}");
    }

    /// <summary>
    /// Verifies that a line appears after another line in the source
    /// </summary>
    protected static void AssertLineAfter(string source, string lineBefore, string lineAfter)
    {
        var lines = source.Split('\n')
            .Select(line => line.TrimStart())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var normalizedBefore = lineBefore.TrimStart();
        var normalizedAfter = lineAfter.TrimStart();

        int beforeIndex = -1;
        int afterIndex = -1;

        for (int i = 0; i < lines.Count; i++)
        {
            if (beforeIndex == -1 && lines[i].Contains(normalizedBefore))
            {
                beforeIndex = i;
            }
            else if (beforeIndex != -1 && lines[i].Contains(normalizedAfter))
            {
                afterIndex = i;
                break;
            }
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(beforeIndex, Is.GreaterThanOrEqualTo(0),
                $"Line '{normalizedBefore}' not found in source");
            Assert.That(afterIndex, Is.GreaterThan(beforeIndex),
                $"Line '{normalizedAfter}' should appear after '{normalizedBefore}'\n" +
                $"In source:\n{source}");
        }
    }

    /// <summary>
    /// Asserts that no errors were generated during source generation
    /// </summary>
    protected static void AssertNoErrors(GeneratorRunResult result, string message = "No errors should be generated")
    {
        Assert.That(result.Errors, Is.Empty, message);
    }

    /// <summary>
    /// Gets a generated source by hint name, asserting it exists
    /// </summary>
    protected static GeneratedFile GetGeneratedSource(GeneratorRunResult result, string hintName)
    {
        var source = result.GeneratedSources.FirstOrDefault(s => s.HintName == hintName);
        Assert.That(source, Is.Not.Null, $"The file '{hintName}' should be generated");
        return source!;
    }

    #endregion

    #region Result Types

    protected class GeneratorRunResult
    {
        public IReadOnlyList<GeneratedFile> GeneratedSources { get; init; } = [];
        public IReadOnlyList<string> Errors { get; init; } = [];
        public Exception? Exception { get; init; }
    }

    protected class GeneratedFile(string hintName, string content)
    {
        public string HintName { get; } = hintName;
        public GeneratedSourceText SourceText { get; } = new GeneratedSourceText(content);
    }

    protected class GeneratedSourceText(string text)
    {
        public override string ToString() => text;
    }

    #endregion
}
