Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon_3"
dotnet run --project ../src/Omnius.Xeus.Daemon/ -- --config "./$Env:BuildTargetName/config.yml" --storage "./$Env:BuildTargetName/storage" --logs "./$Env:BuildTargetName/logs" -v
