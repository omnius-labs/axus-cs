Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon_1"
dotnet run --project ../src/Omnius.Xeus.Service.Daemon/ -- "./$Env:BuildTargetName/state" "./$Env:BuildTargetName/logs" -v
