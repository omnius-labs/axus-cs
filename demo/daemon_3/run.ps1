Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon_3"
dotnet run --project ../../src/Omnius.Xeus.Daemon/ -- --config "./config.yml"
