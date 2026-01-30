using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Argon.OpenApiGenerator;

namespace TestProject1;

/// <summary>
/// Tests pour le générateur de source OpenAPI
/// </summary>
[TestFixture]
public class OpenApiSourceGeneratorTests
{
    private string _testOpenApiFilePath = null!;

    [SetUp]
    public void Setup()
    {
        // Obtenir le chemin du fichier OpenAPI de test
        var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        _testOpenApiFilePath = Path.Combine(testDirectory!, "TestData", "test-openapi.json");

        // Créer le répertoire TestData s'il n'existe pas
        var testDataDir = Path.GetDirectoryName(_testOpenApiFilePath);
        if (!Directory.Exists(testDataDir))
        {
            Directory.CreateDirectory(testDataDir!);
        }

        // Copier le fichier de test s'il n'existe pas
        var sourceFile = Path.Combine(
            Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(testDirectory!))))!,
            "TestProject1", "TestData", "test-openapi.json");

        if (File.Exists(sourceFile) && !File.Exists(_testOpenApiFilePath))
        {
            File.Copy(sourceFile, _testOpenApiFilePath);
        }
    }

    [Test]
    public void Generator_ShouldReadOpenApiFile_WithoutErrors()
    {
        // Arrange - Vérifier que le fichier existe
        Assert.That(File.Exists(_testOpenApiFilePath), Is.True,
            $"Le fichier OpenAPI de test doit exister: {_testOpenApiFilePath}");

        // Arrange - Créer une compilation de test
        var compilation = CreateCompilation();

        // Créer un fichier additionnel avec le fichier OpenAPI
        var additionalFiles = ImmutableArray.Create<AdditionalText>(
            new TestAdditionalText(_testOpenApiFilePath));

        // Configurer les options de l'analyseur
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(
            _testOpenApiFilePath,
            new Dictionary<string, string>
            {
                ["build_metadata.AdditionalFiles.SourceItemType"] = "OpenApiGenerator",
                ["build_metadata.AdditionalFiles.GenerateDtos"] = "true",
                ["build_metadata.AdditionalFiles.GenerateControllers"] = "true",
                ["build_metadata.AdditionalFiles.BaseNamespace"] = "TestNamespace",
                ["build_metadata.AdditionalFiles.UseAsyncControllers"] = "true"
            });
        
        // Act - Créer le générateur et l'exécuter
        var generator = new OpenApiSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: additionalFiles,
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        // Assert
        var runResult = driver.GetRunResult();

        // Vérifier qu'il n'y a pas d'erreurs critiques
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Any())
        {
            Console.WriteLine("Erreurs détectées:");
            foreach (var error in errors)
            {
                Console.WriteLine($"  - {error.Id}: {error.GetMessage()}");
            }
        }
        Assert.That(errors, Is.Empty,
            $"Le générateur a produit des erreurs: {string.Join(", ", errors.Select(e => e.GetMessage()))}");

        // Vérifier que le générateur a bien été exécuté
        Console.WriteLine($"Nombre d'arbres générés: {runResult.GeneratedTrees.Length}");

        // Vérifier qu'il n'y a pas eu d'exception
        var generatorResult = runResult.Results[0];
        Assert.That(generatorResult.Exception, Is.Null,
            $"Le générateur a levé une exception: {generatorResult.Exception?.Message}");

        // Afficher les fichiers générés pour débogage
        Console.WriteLine($"Fichiers générés: {generatorResult.GeneratedSources.Length}");
        foreach (var source in generatorResult.GeneratedSources)
        {
            Console.WriteLine($"- {source.HintName}");
            Console.WriteLine($"  Taille: {source.SourceText.Length} caractères");
        }

        // Le test réussit si aucune erreur n'a été levée
        // (le générateur peut ne pas produire de code si le fichier est vide ou invalide,
        // mais il ne devrait pas crasher)
    }

    private static CSharpCompilation CreateCompilation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace TestNamespace
{
    public class TestClass { }
}");

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
}

/// <summary>
/// Classe pour simuler un fichier additionnel
/// </summary>
internal class TestAdditionalText : AdditionalText
{
    private readonly string _path;

    public TestAdditionalText(string path)
    {
        _path = path;
    }

    public override string Path => _path;

    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return SourceText.From(File.ReadAllText(_path), Encoding.UTF8);
    }
}

/// <summary>
/// Classe pour simuler les options de configuration de l'analyseur
/// </summary>
internal class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
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

/// <summary>
/// Classe pour simuler les options de configuration
/// </summary>
internal class TestAnalyzerConfigOptions : AnalyzerConfigOptions
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
