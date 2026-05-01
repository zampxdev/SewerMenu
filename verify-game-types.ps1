$ErrorActionPreference = "Stop"

$gamePath = if ($args.Count -gt 0) { $args[0] } else { $env:SCHEDULE_I_PATH }
if ([string]::IsNullOrWhiteSpace($gamePath)) {
    $gamePath = "C:\Program Files (x86)\Steam\steamapps\common\Schedule I"
}

dotnet build --configuration Debug
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project .\tools\SewerMenu.GameAssemblyVerifier\SewerMenu.GameAssemblyVerifier.csproj -- "$gamePath"
exit $LASTEXITCODE
