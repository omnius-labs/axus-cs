Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon_2"
dotnet run --project ../../src/Omnius.Axis.Daemon/ -- --config "./config.yml"
