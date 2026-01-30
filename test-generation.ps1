# Script pour tester la génération
cd "C:\Users\flavienchenu\Documents\repos\argon\argon-openapi-generator"

# Clean
Remove-Item -Path "TestProject1\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "TestProject1\bin" -Recurse -Force -ErrorAction SilentlyContinue

# Build le générateur
Write-Host "Building generator..." -ForegroundColor Cyan
dotnet build Argon.OpenApiGenerator/Argon.OpenApiGenerator.csproj -v quiet

# Build le projet de test
Write-Host "`nBuilding test project..." -ForegroundColor Cyan
dotnet build TestProject1/TestProject1.csproj -v quiet

# Chercher les fichiers générés
Write-Host "`nSearching for generated files..." -ForegroundColor Cyan
$generatedFiles = Get-ChildItem -Path "TestProject1\obj" -Filter "*.g.cs" -Recurse -ErrorAction SilentlyContinue

if ($generatedFiles) {
    Write-Host "`nGenerated files found:" -ForegroundColor Green
    foreach ($file in $generatedFiles) {
        Write-Host "  $($file.FullName)" -ForegroundColor Yellow
        Write-Host "`nContent preview:" -ForegroundColor Cyan
        Get-Content $file.FullName | Select-Object -First 50
        Write-Host "`n---`n"
    }
} else {
    Write-Host "No generated files found!" -ForegroundColor Red
}
