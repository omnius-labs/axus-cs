Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon_2"
dotnet run --project ../../src/Omnius.Xeus.Daemon/ -- --config "./config.yml"
