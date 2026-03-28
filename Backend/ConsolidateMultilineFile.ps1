# Script para consolidar archivo multi-linea en formato importable para SQL Server
# Cada registro principal y sus lineas de detalle se consolidan en una sola linea

param(
    [string]$InputFile = "c:\Users\mauricio.perez\Downloads\lecturas_clean.txt",
    [string]$OutputFile = "c:\Users\mauricio.perez\Downloads\lecturas_consolidated.txt"
)

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  Consolidador de Archivos Multi-Linea" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Archivo de entrada: $InputFile" -ForegroundColor Yellow
Write-Host "Archivo de salida:  $OutputFile" -ForegroundColor Yellow
Write-Host ""

# Verificar que el archivo de entrada existe
if (-not (Test-Path $InputFile)) {
    Write-Host "ERROR: El archivo de entrada no existe: $InputFile" -ForegroundColor Red
    exit 1
}

# Variables para procesar el archivo
$currentMainLine = $null
$detailValues = @()
$recordCount = 0
$lineCount = 0

# Crear StreamWriter para mejor rendimiento con archivos grandes
$outputStream = [System.IO.StreamWriter]::new($OutputFile, $false, [System.Text.Encoding]::UTF8)

try {
    # Leer archivo linea por linea
    $reader = [System.IO.StreamReader]::new($InputFile, [System.Text.Encoding]::UTF8)

    Write-Host "Procesando archivo..." -ForegroundColor Green

    while ($null -ne ($line = $reader.ReadLine())) {
        $lineCount++

        # Mostrar progreso cada 1000 lineas
        if ($lineCount % 1000 -eq 0) {
            Write-Host "  Procesadas $lineCount lineas..." -ForegroundColor Gray
        }

        # Ignorar lineas vacias
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        # Verificar si es una linea principal (empieza con numero seguido de TAB)
        if ($line -match '^\d+\t') {
            # Si ya habia una linea principal anterior, escribirla con sus detalles
            if ($null -ne $currentMainLine) {
                if ($detailValues.Count -gt 0) {
                    $consolidatedLine = $currentMainLine + "`t" + ($detailValues -join "`t")
                } else {
                    $consolidatedLine = $currentMainLine
                }
                $outputStream.WriteLine($consolidatedLine)
                $recordCount++
            }

            # Iniciar nuevo registro
            $currentMainLine = $line
            $detailValues = @()
        }
        # Es una linea de detalle
        else {
            # Extraer el contenido despues de los dos puntos
            # Patrones: "Desc: valor", "Dato 1: valor", "ID: valor", "LO: valor", etc.
            if ($line -match '^[^:]+:\s*(.*)$') {
                $detailContent = $matches[1].Trim()

                # Si el contenido tiene TABs, dividir y agregar todas las partes
                if ($detailContent -match '\t') {
                    $parts = $detailContent -split '\t'
                    foreach ($part in $parts) {
                        if (-not [string]::IsNullOrWhiteSpace($part)) {
                            $detailValues += $part.Trim()
                        }
                    }
                } else {
                    # Agregar el valor completo
                    if (-not [string]::IsNullOrWhiteSpace($detailContent)) {
                        $detailValues += $detailContent
                    }
                }
            }
        }
    }

    # No olvidar escribir el ultimo registro
    if ($null -ne $currentMainLine) {
        if ($detailValues.Count -gt 0) {
            $consolidatedLine = $currentMainLine + "`t" + ($detailValues -join "`t")
        } else {
            $consolidatedLine = $currentMainLine
        }
        $outputStream.WriteLine($consolidatedLine)
        $recordCount++
    }

    $reader.Close()
    $outputStream.Close()

    Write-Host ""
    Write-Host "=====================================================" -ForegroundColor Green
    Write-Host "  Consolidacion completada exitosamente" -ForegroundColor Green
    Write-Host "=====================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Estadisticas:" -ForegroundColor Yellow
    Write-Host "  Lineas procesadas: $lineCount" -ForegroundColor White
    Write-Host "  Registros consolidados: $recordCount" -ForegroundColor White

    $inputSize = (Get-Item $InputFile).Length
    $outputSize = (Get-Item $OutputFile).Length
    Write-Host "  Tamanio entrada: $([math]::Round($inputSize/1MB, 2)) MB" -ForegroundColor White
    Write-Host "  Tamanio salida: $([math]::Round($outputSize/1MB, 2)) MB" -ForegroundColor White
    Write-Host ""
    Write-Host "Archivo consolidado: $OutputFile" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Proximos pasos:" -ForegroundColor Yellow
    Write-Host "  1. Verificar las primeras lineas del archivo consolidado" -ForegroundColor White
    Write-Host "  2. Importar en SQL Server usando Import/Export Wizard" -ForegroundColor White
    Write-Host "     - Delimitador: TAB (tabulador)" -ForegroundColor White
    Write-Host "     - Tipo columnas: VARCHAR(MAX) o NVARCHAR(MAX)" -ForegroundColor White
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "ERROR al procesar el archivo:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($null -ne $reader) { $reader.Close() }
    if ($null -ne $outputStream) { $outputStream.Close() }
    exit 1
}
