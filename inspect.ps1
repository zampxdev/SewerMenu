$ErrorActionPreference = "Stop"

$gamePath = if ($args.Count -gt 0) { $args[0] } else { $env:SCHEDULE_I_PATH }
if ([string]::IsNullOrWhiteSpace($gamePath)) {
    $gamePath = "C:\Program Files (x86)\Steam\steamapps\common\Schedule I"
}

$filters = @("Player", "Money", "Health", "Police", "Item", "Time", "Manager", "ScheduleOne")

foreach ($filter in $filters) {
    Write-Host "`n=== $filter TYPES ===" -ForegroundColor Green
    dotnet run --project .\tools\SewerMenu.GameAssemblyVerifier\SewerMenu.GameAssemblyVerifier.csproj -- "$gamePath" --list $filter
}
