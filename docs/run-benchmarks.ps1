# Benchmark Runner Script
# Runs all demo apps and collects results for diploma research

param(
    [int]$Iterations = 10,
    [string]$ApiBase = "http://localhost:5000"
)

$serverRoot = "$PSScriptRoot/../appointment-management-application/server"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Hair Salon Appointments - Benchmark Suite" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:"
Write-Host "  API Base:    $ApiBase"
Write-Host "  Iterations:  $Iterations"
Write-Host "  Server root: $serverRoot"
Write-Host ""

# Set environment variables (read by each Demo Program.cs)
$env:API_BASE   = $ApiBase
$env:ITERATIONS = $Iterations

# Clear old results
$resultsDir = "$PSScriptRoot/docs/results"
if (Test-Path $resultsDir) {
    Remove-Item "$resultsDir/*.csv" -Force -ErrorAction SilentlyContinue
}
New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null

# Build all demo projects once
Write-Host "Building demo projects..." -ForegroundColor Yellow
dotnet build "$serverRoot/HairSalonAppointments.sln" -c Release -v quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Aborting." -ForegroundColor Red
    exit 1
}
Write-Host "Build OK." -ForegroundColor Green
Write-Host ""

Write-Host "Running benchmarks..." -ForegroundColor Yellow
Write-Host ""

# Run ICS Demo
Write-Host "[1/4] Running ICS Demo..." -ForegroundColor Green
dotnet run --project "$serverRoot/HairSalonAppointments.Demo.Ics" --no-build -c Release 2>&1 | Out-Null
Write-Host "      Done!" -ForegroundColor Green

# Run CalDAV Demo
Write-Host "[2/4] Running CalDAV Demo..." -ForegroundColor Green
dotnet run --project "$serverRoot/HairSalonAppointments.Demo.CalDav" --no-build -c Release 2>&1 | Out-Null
Write-Host "      Done!" -ForegroundColor Green

# Run Google Demo (requires Google:ServiceAccountKeyPath + Google:CalendarId in appsettings)
Write-Host "[3/4] Running Google Demo..." -ForegroundColor Green
dotnet run --project "$serverRoot/HairSalonAppointments.Demo.Google" --no-build -c Release 2>&1 | Out-Null
Write-Host "      Done!" -ForegroundColor Green

# Run API Demo (appointments + suggestions)
Write-Host "[4/4] Running API Demo..." -ForegroundColor Green
dotnet run --project "$serverRoot/HairSalonAppointments.Demo.Api" --no-build -c Release 2>&1 | Out-Null
Write-Host "      Done!" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Benchmarks complete!" -ForegroundColor Cyan
Write-Host "Results saved to: $resultsDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files:"
Get-ChildItem "$resultsDir/*.csv" -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "  - $($_.Name)" }
Write-Host ""
Write-Host "Run ./analyze-results.ps1 to generate summary table."
