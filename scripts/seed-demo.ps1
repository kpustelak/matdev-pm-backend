# Ładuje mock/demo data do bazy (Development API)
# Użycie:
#   powershell -File scripts/seed-demo.ps1          # tylko jeśli baza pusta
#   powershell -File scripts/seed-demo.ps1 -Reset   # czyści projekty i ładuje od nowa

param(
    [switch]$Reset,
    [string]$BaseUrl = "http://127.0.0.1:5196"
)

$endpoint = if ($Reset) { "reset-demo" } else { "seed-demo" }
$url = "$BaseUrl/api/dev/$endpoint"

Write-Host "POST $url" -ForegroundColor Cyan

try {
    $res = Invoke-RestMethod -Uri $url -Method POST -TimeoutSec 120
    Write-Host $res.message -ForegroundColor Green
    Write-Host ($res.data | ConvertTo-Json -Compress)
} catch {
    Write-Host "Blad: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Upewnij sie ze API dziala (Development) i Docker/PostgreSQL jest wlaczone."
    exit 1
}
