# Script de build et test pour Argon OpenAPI Generator

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Build", "Test", "Pack", "Clean", "All")]
    [string]$Action = "Build",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$GeneratorProject = Join-Path $ProjectRoot "Argon.OpenApiGenerator\Argon.OpenApiGenerator.csproj"
$SampleProject = Join-Path $ProjectRoot "Samples\Sample.Api\Sample.Api.csproj"

function Write-Step {
    param([string]$Message)
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Build-Generator {
    Write-Step "Construction du générateur Argon.OpenApiGenerator"
    dotnet build $GeneratorProject -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Échec de la construction du générateur"
    }
    Write-Host "✓ Générateur construit avec succès" -ForegroundColor Green
}

function Test-Generator {
    Write-Step "Test du générateur avec le projet exemple"
    
    # Nettoie le projet exemple
    Write-Host "Nettoyage du projet exemple..."
    dotnet clean $SampleProject --verbosity quiet
    
    # Construit le projet exemple
    Write-Host "Construction du projet exemple..."
    dotnet build $SampleProject -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Échec de la construction du projet exemple"
    }
    
    # Vérifie les fichiers générés
    $ObjPath = Join-Path $ProjectRoot "Samples\Sample.Api\obj\$Configuration\net10.0"
    Write-Host "`nRecherche des fichiers générés dans: $ObjPath"
    
    $GeneratedFiles = Get-ChildItem -Path $ObjPath -Filter "*.g.cs" -Recurse -ErrorAction SilentlyContinue
    
    if ($GeneratedFiles.Count -gt 0) {
        Write-Host "✓ Fichiers générés trouvés:" -ForegroundColor Green
        foreach ($file in $GeneratedFiles) {
            Write-Host "  - $($file.FullName)" -ForegroundColor Gray
        }
    } else {
        Write-Host "⚠ Aucun fichier généré trouvé (vérifiez les logs de build)" -ForegroundColor Yellow
    }
    
    Write-Host "✓ Test complété" -ForegroundColor Green
}

function Pack-Generator {
    Write-Step "Création du package NuGet"
    dotnet pack $GeneratorProject -c Release --output "$ProjectRoot\nupkgs"
    if ($LASTEXITCODE -ne 0) {
        throw "Échec de la création du package"
    }
    
    $PackagePath = Join-Path $ProjectRoot "nupkgs"
    $Packages = Get-ChildItem -Path $PackagePath -Filter "*.nupkg"
    
    Write-Host "`n✓ Package créé avec succès:" -ForegroundColor Green
    foreach ($pkg in $Packages) {
        Write-Host "  - $($pkg.FullName)" -ForegroundColor Gray
    }
}

function Clean-All {
    Write-Step "Nettoyage de tous les projets"
    
    dotnet clean $GeneratorProject
    dotnet clean $SampleProject
    
    # Supprime les dossiers bin et obj
    Get-ChildItem -Path $ProjectRoot -Include bin,obj,nupkgs -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    
    Write-Host "✓ Nettoyage terminé" -ForegroundColor Green
}

# Exécution principale
try {
    Write-Host "Argon OpenAPI Generator - Build Script" -ForegroundColor Magenta
    Write-Host "Configuration: $Configuration" -ForegroundColor Gray
    Write-Host "Action: $Action" -ForegroundColor Gray
    
    switch ($Action) {
        "Build" {
            Build-Generator
        }
        "Test" {
            Build-Generator
            Test-Generator
        }
        "Pack" {
            Build-Generator
            Pack-Generator
        }
        "Clean" {
            Clean-All
        }
        "All" {
            Clean-All
            Build-Generator
            Test-Generator
            Pack-Generator
        }
    }
    
    Write-Host "`n✓ Opération terminée avec succès!" -ForegroundColor Green
}
catch {
    Write-Host "`n✗ Erreur: $_" -ForegroundColor Red
    exit 1
}
