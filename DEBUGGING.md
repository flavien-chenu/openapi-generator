# Guide de débogage du générateur Argon OpenAPI

## Vérification rapide

### 1. Le générateur compile-t-il ?

```powershell
cd Argon.OpenApiGenerator
dotnet build
```

✅ Devrait compiler sans erreur  
❌ Si erreurs, vérifier les usings et les dépendances

### 2. Le projet exemple compile-t-il ?

```powershell
cd Samples\Sample.Api
dotnet clean
dotnet build
```

✅ Devrait compiler (avec ou sans fichiers générés)  
❌ Si erreurs, vérifier la référence au générateur

### 3. Les fichiers sont-ils générés ?

```powershell
# Chercher tous les fichiers .g.cs générés
Get-ChildItem -Path "Samples\Sample.Api\obj" -Filter "*.g.cs" -Recurse

# Ou regarder dans le dossier généré
dir "Samples\Sample.Api\obj\Debug\net10.0\generated" -Recurse
```

## Débogage avancé

### Activer les logs détaillés

```powershell
cd Samples\Sample.Api
dotnet build -v diagnostic > build-log.txt 2>&1
```

Puis chercher dans `build-log.txt`:
- "ARGON" - Messages de notre générateur
- "OpenApiSourceGenerator" - Notre générateur s'exécute-t-il ?
- "error" - Erreurs de compilation
- "warning" - Avertissements

### Vérifier que le générateur est chargé

Dans les logs détaillés, cherchez:
```
Task "CoreCompile"
  ...
  Using shared compilation with compiler from directory: ...
  Loading analyzer from C:\...\Argon.OpenApiGenerator.dll
```

Si vous ne voyez pas "Loading analyzer", le générateur n'est pas chargé.

**Solutions:**
1. Vérifier que `OutputItemType="Analyzer"` est présent
2. Vérifier que `ReferenceOutputAssembly="false"` est présent
3. Nettoyer et reconstruire: `dotnet clean && dotnet build`

### Vérifier que les AdditionalFiles sont détectés

Dans les logs détaillés, cherchez:
```
AdditionalFiles:
  C:\...\test.yaml
```

Si le fichier n'apparaît pas:
1. Vérifier `<AdditionalFiles Include="test.yaml" />` dans le .csproj
2. Vérifier que le fichier existe
3. Nettoyer et reconstruire

### Erreurs de chargement d'assemblies

Si vous voyez:
```
Could not load file or assembly 'Microsoft.OpenApi'
```

**Solutions:**
1. Vérifier `GeneratePathProperty="true"` sur les packages
2. Vérifier le target `GetDependencyTargetPaths`
3. Vérifier que les DLL sont dans le bon chemin:
   ```powershell
   # Lister les packages NuGet
   ls "$env:USERPROFILE\.nuget\packages\microsoft.openapi\1.6.22\lib\netstandard2.0"
   ```

### Le générateur s'exécute mais ne produit rien

**Causes possibles:**

1. **Erreur de parsing OpenAPI**
   - Vérifier que le YAML/JSON est valide
   - Tester avec un schéma minimal (test.yaml)
   
2. **Exception silencieuse dans le générateur**
   - Ajouter des diagnostics dans le code
   - Vérifier les logs pour des erreurs ARGON001 ou ARGON002

3. **Configuration incorrecte**
   - Les options MSBuild sont-elles définies ?
   - `GenerateDtos` et/ou `GenerateControllers` sont-ils à `true` ?

### Tester avec un schéma minimal

Créez un fichier `minimal.yaml`:

```yaml
openapi: 3.1.0
info:
  title: Test
  version: 1.0.0
paths: {}
components:
  schemas:
    Simple:
      type: object
      required: [id]
      properties:
        id:
          type: integer
```

Ajoutez dans .csproj:
```xml
<AdditionalFiles Include="minimal.yaml" />
```

Construisez et vérifiez.

## Débogage dans l'IDE

### Avec Visual Studio / Rider

1. Ouvrir le projet générateur
2. Mettre un point d'arrêt dans `OpenApiSourceGenerator.Initialize`
3. Ouvrir le projet exemple dans une autre instance de l'IDE
4. Dans la première instance: Debug > Attach to Process > devenv.exe ou rider64.exe
5. Reconstruire le projet exemple

### Avec VS Code

Le débogage de générateurs de source est complexe dans VS Code.  
Alternative: ajouter des `ReportDiagnostic` pour logger:

```csharp
context.ReportDiagnostic(Diagnostic.Create(
    new DiagnosticDescriptor(
        "ARGON999",
        "Debug",
        $"Debug: {message}",
        "Debug",
        DiagnosticSeverity.Info,
        true),
    Location.None));
```

## Tests manuels

### Test 1: DTO simple

```yaml
components:
  schemas:
    Person:
      type: object
      required: [name]
      properties:
        name:
          type: string
```

Attendu: Un record `Person` avec une propriété `Name` de type `string` avec `[Required]`

### Test 2: Contrôleur simple

```yaml
paths:
  /test:
    get:
      responses:
        '200':
          description: OK
```

Attendu: Un contrôleur avec une méthode GET

### Test 3: Types complexes

```yaml
components:
  schemas:
    Complex:
      type: object
      properties:
        items:
          type: array
          items:
            type: string
        metadata:
          type: object
          additionalProperties:
            type: integer
```

Attendu: 
- `items` → `List<string>`
- `metadata` → `Dictionary<string, int>`

## Checklist de dépannage

- [ ] Le générateur compile sans erreur
- [ ] Le générateur est en netstandard2.0
- [ ] Les packages Microsoft.OpenApi sont installés
- [ ] Le projet exemple référence le générateur avec `OutputItemType="Analyzer"`
- [ ] Les fichiers OpenAPI sont dans `<AdditionalFiles>`
- [ ] Les fichiers OpenAPI sont valides (testés sur editor.swagger.io)
- [ ] Les options MSBuild sont définies (ou valeurs par défaut OK)
- [ ] `dotnet clean` a été exécuté
- [ ] L'IDE a été redémarré
- [ ] Les logs détaillés ont été vérifiés

## En dernier recours

Si rien ne fonctionne:

1. Créer un nouveau projet de test minimal
2. Copier uniquement test.yaml
3. Référencer le générateur
4. Construire
5. Si ça fonctionne → problème dans le projet original
6. Si ça ne fonctionne pas → problème dans le générateur

## Support

Si le problème persiste:
1. Créer un repo GitHub minimal qui reproduit le problème
2. Inclure:
   - Le fichier OpenAPI
   - Le .csproj
   - Les logs de build complets
   - La version de .NET: `dotnet --version`
   - L'OS et l'IDE utilisés
