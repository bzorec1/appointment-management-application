# Results Analysis Script
# Aggregates benchmark CSV results for diploma

$resultsDir = "./docs/results"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Benchmark Results Analysis" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$files = Get-ChildItem "$resultsDir/*.csv" -ErrorAction SilentlyContinue

if (-not $files) {
    Write-Host "No results found in $resultsDir" -ForegroundColor Red
    Write-Host "Run ./run-benchmarks.ps1 first." -ForegroundColor Yellow
    exit 1
}

# Process each CSV file
$allResults = @()

foreach ($file in $files) {
    $provider = $file.BaseName
    $data = Import-Csv $file -Header "Timestamp","Provider","Operation","Ms","Status","Success","Notes"

    foreach ($row in $data) {
        $allResults += [PSCustomObject]@{
            Provider = $row.Provider
            Operation = $row.Operation
            Ms = [int]$row.Ms
            Success = $row.Success -eq "true"
        }
    }
}

# Group by Provider and Operation
$grouped = $allResults | Group-Object Provider, Operation

Write-Host "| Provider | Operation | Count | Avg (ms) | Min | Max | Success % |" -ForegroundColor White
Write-Host "|----------|-----------|-------|----------|-----|-----|-----------|" -ForegroundColor White

foreach ($group in $grouped | Sort-Object Name) {
    $parts = $group.Name -split ", "
    $provider = $parts[0]
    $operation = $parts[1]

    $times = $group.Group | ForEach-Object { $_.Ms }
    $successes = ($group.Group | Where-Object { $_.Success }).Count

    $avg = [math]::Round(($times | Measure-Object -Average).Average, 0)
    $min = ($times | Measure-Object -Minimum).Minimum
    $max = ($times | Measure-Object -Maximum).Maximum
    $successRate = [math]::Round(($successes / $group.Count) * 100, 0)

    Write-Host "| $($provider.PadRight(8)) | $($operation.PadRight(9)) | $($group.Count.ToString().PadLeft(5)) | $($avg.ToString().PadLeft(8)) | $($min.ToString().PadLeft(3)) | $($max.ToString().PadLeft(3)) | $($successRate.ToString().PadLeft(7))% |"
}

Write-Host ""
Write-Host "Total operations: $($allResults.Count)" -ForegroundColor Gray

# Export summary to CSV
$summaryFile = "$resultsDir/summary.csv"
$summary = foreach ($group in $grouped | Sort-Object Name) {
    $parts = $group.Name -split ", "
    $times = $group.Group | ForEach-Object { $_.Ms }
    $successes = ($group.Group | Where-Object { $_.Success }).Count

    [PSCustomObject]@{
        Provider = $parts[0]
        Operation = $parts[1]
        Count = $group.Count
        AvgMs = [math]::Round(($times | Measure-Object -Average).Average, 2)
        MinMs = ($times | Measure-Object -Minimum).Minimum
        MaxMs = ($times | Measure-Object -Maximum).Maximum
        SuccessRate = [math]::Round(($successes / $group.Count) * 100, 2)
    }
}

$summary | Export-Csv $summaryFile -NoTypeInformation
Write-Host ""
Write-Host "Summary exported to: $summaryFile" -ForegroundColor Green
