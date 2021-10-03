Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon_3"
dotnet run --project ../src/Omnius.Xeus.Service.Daemon/ -- "./$Env:BuildTargetName/state" "./$Env:BuildTargetName/logs" -v
