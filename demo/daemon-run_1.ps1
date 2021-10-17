Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon_1"
dotnet run --project ../src/Omnius.Xeus.Service.Daemon/ -- --config "./$Env:BuildTargetName/config.yml" --storage "./$Env:BuildTargetName/storage" --logs "./$Env:BuildTargetName/logs" -v
