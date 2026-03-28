# Eliminar BOM del archivo consolidado
$FilePath = "c:\Users\mauricio.perez\Downloads\lecturas_consolidated.txt"

$content = Get-Content -Path $FilePath -Encoding UTF8 -Raw
$content = $content.TrimStart([char]0xFEFF)

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($FilePath, $content, $utf8NoBom)

Write-Host "BOM eliminado correctamente de: $FilePath" -ForegroundColor Green
