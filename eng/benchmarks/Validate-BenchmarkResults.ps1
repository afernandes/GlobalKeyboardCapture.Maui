param(
    [string]$ArtifactsPath = "BenchmarkDotNet.Artifacts/results",
    [string]$PolicyPath = "GlobalKeyboardCapture.Maui.Benchmarks/benchmark-policy.json"
)

$ErrorActionPreference = "Stop"

$policy = Get-Content -LiteralPath $PolicyPath -Raw | ConvertFrom-Json
$reports = Get-ChildItem -LiteralPath $ArtifactsPath -Filter "*-report-full-compressed.json"
if ($reports.Count -eq 0) {
    throw "No BenchmarkDotNet JSON reports were found in $ArtifactsPath."
}

$results = @{}
foreach ($report in $reports) {
    $document = Get-Content -LiteralPath $report.FullName -Raw | ConvertFrom-Json
    foreach ($benchmark in $document.Benchmarks) {
        $results[$benchmark.Method] = [pscustomobject]@{
            MeanNanoseconds = [double]$benchmark.Statistics.Mean
            AllocatedBytes = [double]$benchmark.Memory.BytesAllocatedPerOperation
        }
    }
}

foreach ($required in $policy.requiredBenchmarks) {
    if (-not $results.ContainsKey($required)) {
        throw "Required benchmark '$required' is missing."
    }
    if ($results[$required].MeanNanoseconds -le 0) {
        throw "Required benchmark '$required' did not produce a positive mean."
    }
}

$disabledAllocation = $results["MetricsDisabled"].AllocatedBytes
$enabledAllocation = $results["MetricsEnabled"].AllocatedBytes
$additionalAllocation = $enabledAllocation - $disabledAllocation
if ($additionalAllocation -gt $policy.allocationRegressionBytesPerOperation) {
    throw "Metrics add $additionalAllocation B/op; the policy allows at most $($policy.allocationRegressionBytesPerOperation) B/op."
}

$summary = @(
    "Benchmark baseline: $($policy.baselineVersion)",
    "Required benchmarks: $($policy.requiredBenchmarks.Count)",
    "Metrics-disabled allocation: $disabledAllocation B/op",
    "Metrics-enabled allocation: $enabledAllocation B/op",
    "Additional metrics allocation: $additionalAllocation B/op",
    "Historical time-regression investigation threshold: $($policy.regressionThresholdPercent)%"
) -join [Environment]::NewLine

Write-Output $summary
if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value "## Benchmark policy"
    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value '```text'
    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value $summary
    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value '```'
}
