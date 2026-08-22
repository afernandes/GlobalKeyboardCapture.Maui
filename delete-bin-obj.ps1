# Wipes every bin/, obj/ and .vs/ folder under the repo root (the script's own folder),
# regardless of the current working directory. Use when stale build artifacts cause
# weird MAUI/Android errors.
Write-Host "Deleting all bin, obj and .vs folders under $PSScriptRoot ..." -ForegroundColor Cyan

Get-ChildItem -Path $PSScriptRoot -Recurse -Directory -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in 'bin', 'obj', '.vs' -and $_.FullName -notmatch '\\node_modules\\' } |
    ForEach-Object {
        Write-Host "Deleting:" $_.FullName -ForegroundColor Yellow
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }

Write-Host "Done." -ForegroundColor Green
