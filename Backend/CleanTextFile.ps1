# Script para limpiar caracteres especiales de archivo de texto
# Mantiene el formato multi-linea pero elimina BOM y caracteres problematicos

param(
    [string]$InputFile = "c:\Users\mauricio.perez\Downloads\lecturas.txt",
    [string]$OutputFile = "c:\Users\mauricio.perez\Downloads\lecturas_clean.txt"
)

Write-Host "Limpiando archivo: $InputFile" -ForegroundColor Cyan

# Leer el archivo con codificacion UTF-8
$content = Get-Content -Path $InputFile -Encoding UTF8 -Raw

# Remover BOM (Byte Order Mark)
$content = $content.TrimStart([char]0xFEFF)

# Reemplazar caracteres especiales usando sus codigos Unicode
$content = $content -replace [char]0x00E1, 'a'  # a con acento
$content = $content -replace [char]0x00E9, 'e'  # e con acento
$content = $content -replace [char]0x00ED, 'i'  # i con acento
$content = $content -replace [char]0x00F3, 'o'  # o con acento
$content = $content -replace [char]0x00FA, 'u'  # u con acento
$content = $content -replace [char]0x00C1, 'A'  # A con acento
$content = $content -replace [char]0x00C9, 'E'  # E con acento
$content = $content -replace [char]0x00CD, 'I'  # I con acento
$content = $content -replace [char]0x00D3, 'O'  # O con acento
$content = $content -replace [char]0x00DA, 'U'  # U con acento
$content = $content -replace [char]0x00F1, 'n'  # enye minuscula
$content = $content -replace [char]0x00D1, 'N'  # enye mayuscula
$content = $content -replace [char]0x00FC, 'u'  # u con dieresis
$content = $content -replace [char]0x00DC, 'U'  # U con dieresis
$content = $content -replace [char]0x00BF, ''   # signo de interrogacion invertido
$content = $content -replace [char]0x00A1, ''   # signo de exclamacion invertido
$content = $content -replace [char]0x00B0, ''   # simbolo de grado
$content = $content -replace [char]0x20AC, 'EUR' # simbolo euro
$content = $content -replace [char]0x00A3, 'GBP' # simbolo libra
$content = $content -replace [char]0x00A2, 'c'   # simbolo centavo
$content = $content -replace [char]0x2122, 'TM'  # trademark
$content = $content -replace [char]0x00AE, 'R'   # registrado
$content = $content -replace [char]0x00A9, 'C'   # copyright
$content = $content -replace [char]0x2013, '-'   # en dash
$content = $content -replace [char]0x2014, '-'   # em dash
$content = $content -replace [char]0x201C, '"'   # comilla izquierda
$content = $content -replace [char]0x201D, '"'   # comilla derecha
$content = $content -replace [char]0x2018, "'"   # apostrofe izquierdo
$content = $content -replace [char]0x2019, "'"   # apostrofe derecho
$content = $content -replace [char]0x2026, '...' # puntos suspensivos

# Opcional: Remover cualquier otro caracter no ASCII
# Descomenta la siguiente linea si quieres eliminar TODOS los caracteres no-ASCII
# $content = $content -replace '[^\x00-\x7F]', ''

# Guardar el archivo limpio con codificacion UTF-8 sin BOM
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($OutputFile, $content, $utf8NoBom)

Write-Host "Archivo limpio guardado en: $OutputFile" -ForegroundColor Green
Write-Host ""
Write-Host "Estadisticas:" -ForegroundColor Yellow

# Mostrar informacion del archivo
$originalSize = (Get-Item $InputFile).Length
$cleanSize = (Get-Item $OutputFile).Length
$originalLines = (Get-Content $InputFile -Encoding UTF8).Count
$cleanLines = (Get-Content $OutputFile -Encoding UTF8).Count

Write-Host "  Tamanio original: $([math]::Round($originalSize/1MB, 2)) MB"
Write-Host "  Tamanio limpio: $([math]::Round($cleanSize/1MB, 2)) MB"
Write-Host "  Lineas originales: $originalLines"
Write-Host "  Lineas limpias: $cleanLines"
Write-Host ""
Write-Host "Ahora puedes importar el archivo: $OutputFile" -ForegroundColor Cyan
