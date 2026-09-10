param([switch]$NoRestore)
$ErrorActionPreference = 'Stop'
$resultDirectory = Join-Path $PSScriptRoot ('TestResults/' + [Guid]::NewGuid().ToString('N'))
$arguments = @('test', (Join-Path $PSScriptRoot 'CloudShopping.Tests.Units.csproj'), '--collect:XPlat Code Coverage', '--settings', (Join-Path $PSScriptRoot 'coverage.runsettings'), '--results-directory', $resultDirectory)
if ($NoRestore) { $arguments += '--no-restore' }
& dotnet @arguments
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$reports = @(Get-ChildItem -LiteralPath $resultDirectory -Recurse -Filter 'coverage.cobertura.xml')
if ($reports.Count -ne 1) { throw 'Era esperado exatamente um relatório de cobertura desta execução.' }
[xml]$coverage = Get-Content -LiteralPath $reports[0].FullName -Raw
$failed = $false
foreach ($assembly in @('CloudShopping.Domain', 'CloudShopping.Application')) {
    $package = @($coverage.coverage.packages.package | Where-Object { $_.name -eq $assembly })
    if ($package.Count -ne 1) { throw "Camada ausente ou duplicada no relatório: $assembly" }
    $rate = [double]::Parse($package[0].GetAttribute('line-rate'), [Globalization.CultureInfo]::InvariantCulture)
    $branchRate = [double]::Parse($package[0].GetAttribute('branch-rate'), [Globalization.CultureInfo]::InvariantCulture)
    Write-Host ('{0}: linhas {1:P2}; branches {2:P2}; meta de linhas 90%' -f $assembly, $rate, $branchRate)
    if ($rate -lt 0.90) { $failed = $true }
}
Write-Host "Relatório: $($reports[0].FullName)"
if ($failed) { Write-Error 'Meta não atingida: cada camada deve ter pelo menos 90% de cobertura de linhas.' -ErrorAction Continue; exit 1 }
exit 0
