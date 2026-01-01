$dllPath = "C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll"
$asm = [System.Reflection.Assembly]::LoadFrom($dllPath)

Write-Host "=== PLAYER TYPES ===" -ForegroundColor Green
$asm.GetTypes() | Where-Object { $_.FullName -like '*Player*' -and $_.IsClass } | ForEach-Object { Write-Host $_.FullName }

Write-Host "`n=== MONEY/ECONOMY TYPES ===" -ForegroundColor Green
$asm.GetTypes() | Where-Object { ($_.FullName -like '*Money*' -or $_.FullName -like '*Economy*' -or $_.FullName -like '*Cash*') -and $_.IsClass } | ForEach-Object { Write-Host $_.FullName }

Write-Host "`n=== HEALTH/DAMAGE TYPES ===" -ForegroundColor Green
$asm.GetTypes() | Where-Object { ($_.FullName -like '*Health*' -or $_.FullName -like '*Damage*') -and $_.IsClass } | ForEach-Object { Write-Host $_.FullName }

Write-Host "`n=== POLICE/LAW TYPES ===" -ForegroundColor Green
$asm.GetTypes() | Where-Object { ($_.FullName -like '*Police*' -or $_.FullName -like '*Law*' -or $_.FullName -like '*Cop*') -and $_.IsClass } | ForEach-Object { Write-Host $_.FullName }

Write-Host "`n=== ITEM/INVENTORY TYPES ===" -ForegroundColor Green
$asm.GetTypes() | Where-Object { ($_.FullName -like '*Item*' -or $_.FullName -like '*Inventory*') -and $_.IsClass } | Select-Object -First 30 | ForEach-Object { Write-Host $_.FullName }

Write-Host "`n=== TIME/WORLD TYPES ===" -ForegroundColor Green
$asm.GetTypes() | Where-Object { ($_.FullName -like '*Time*' -or $_.FullName -like '*World*' -or $_.FullName -like '*Day*') -and $_.IsClass } | ForEach-Object { Write-Host $_.FullName }

Write-Host "`n=== MANAGER/SINGLETON TYPES ===" -ForegroundColor Green
$asm.GetTypes() | Where-Object { ($_.FullName -like '*Manager*' -or $_.FullName -like '*Singleton*') -and $_.IsClass } | ForEach-Object { Write-Host $_.FullName }

Write-Host "`n=== SCHEDULEONE NAMESPACE (first 50) ===" -ForegroundColor Green
$asm.GetTypes() | Where-Object { $_.FullName -like 'ScheduleOne*' -and $_.IsClass } | Select-Object -First 50 | ForEach-Object { Write-Host $_.FullName }
