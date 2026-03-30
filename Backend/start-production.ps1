# =============================================================================
# Script de arranque para producción (Windows)
# Ejecutar desde la carpeta Backend/
# Requiere: .NET 8 Runtime instalado en el servidor
# =============================================================================

$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS        = "http://*:5000"

Set-Location "$PSScriptRoot\publish"
dotnet LaundryManagement.API.dll
