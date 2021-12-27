Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon-2"
dotnet run --project ../../src/Omnius.Axis.Daemon/ -- -s ./storage/daemon -l "tcp(ip4(127.0.0.1),43202)" -v true
