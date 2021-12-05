Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon_1"
dotnet run --project ../../src/Omnius.Xeus.Daemon/ -- --config "./config.yml"
