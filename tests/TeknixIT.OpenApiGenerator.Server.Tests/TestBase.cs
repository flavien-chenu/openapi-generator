using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

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
            ["build_metadata.AdditionalFiles.SourceItemType"] = "OpenApiGeneratorServer",
            ["build_metadata.AdditionalFiles.GenerateControllers"] = "true",
            ["build_metadata.AdditionalFiles.BaseNamespace"] = "TestApp",
            ["build_metadata.AdditionalFiles.ContractsNamespace"] = "Contracts",
            ["build_metadata.AdditionalFiles.ControllersNamespace"] = "Controllers",
            ["build_metadata.AdditionalFiles.UseAsyncControllers"] = "true",
            ["build_metadata.AdditionalFiles.GenerateValidationAttributes"] = "true",
            ["build_metadata.AdditionalFiles.AddApiControllerAttribute"] = "true",
            ["build_metadata.AdditionalFiles.ControllerBaseClass"] = "ControllerBase",
            ["build_metadata.AdditionalFiles.UseRecords"] = "true",
            ["build_metadata.AdditionalFiles.GenerateXmlDocumentation"] = "true"
        };
    }

    protected static GeneratorRunResult RunGenerator(string openApiFile, Dictionary<string, string> config)
    {
        var compilation = CreateCompilation();

        var additionalFiles = ImmutableArray.Create<AdditionalText>(
            new TestAdditionalText(openApiFile));

        var optionsProvider = new TestAnalyzerConfigOptionsProvider(openApiFile, config);

        var generator = new OpenApiSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: additionalFiles,
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out Compilation _,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        var generatorResult = runResult.Results[0];

        return new GeneratorRunResult
        {
            GeneratedSources = generatorResult.GeneratedSources,
            Diagnostics = diagnostics,
            Exception = generatorResult.Exception
        };
    }

    private static CSharpCompilation CreateCompilation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("""
                                                    namespace TestNamespace
                                                    {
                                                        public class TestClass { }
                                                    }
                                                    """);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        return CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
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
        var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.That(errors, Is.Empty, message);
    }

    /// <summary>
    /// Gets a generated source by hint name, asserting it exists
    /// </summary>
    protected static GeneratedSourceResult GetGeneratedSource(GeneratorRunResult result, string hintName)
    {
        var source = result.GeneratedSources.FirstOrDefault(s => s.HintName == hintName);
        Assert.That(source.HintName, Is.Not.Null, $"The file '{hintName}' should be generated");
        return source;
    }

    #endregion

    #region Test Helper Classes

    protected class GeneratorRunResult
    {
        public ImmutableArray<GeneratedSourceResult> GeneratedSources { get; set; }
        public ImmutableArray<Diagnostic> Diagnostics { get; set; }
        public Exception? Exception { get; set; }
    }

    private class TestAdditionalText : AdditionalText
    {
        private readonly string _path;

        public TestAdditionalText(string path)
        {
            _path = path;
        }

        public override string Path => _path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(!File.Exists(_path)
                ? string.Empty
                : File.ReadAllText(_path), Encoding.UTF8);
        }
    }

    private class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly TestAnalyzerConfigOptions _options;

        public TestAnalyzerConfigOptionsProvider(string filePath, Dictionary<string, string> options)
        {
            _options = new TestAnalyzerConfigOptions(filePath, options);
        }

        public override AnalyzerConfigOptions GlobalOptions => _options;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _options;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _options;
    }

    private class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _options;

        public TestAnalyzerConfigOptions(string _, Dictionary<string, string> options)
        {
            _options = new Dictionary<string, string>(options);
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _options.TryGetValue(key, out value!);
        }
    }

    #endregion
}
