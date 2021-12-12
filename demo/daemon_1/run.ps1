Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon_1"
dotnet run --project ../../src/Omnius.Axis.Daemon/ -- --config "./config.yml"
