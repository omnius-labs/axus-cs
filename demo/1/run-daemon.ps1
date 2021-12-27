Set-Location $PSScriptRoot

$Env:BuildTargetName = "daemon-1"
dotnet run --project ../../src/Omnius.Axis.Daemon/ -- -s ./storage/daemon -l "tcp(ip4(127.0.0.1),43201)" -v true
