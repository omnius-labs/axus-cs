Set-Location $PSScriptRoot

$Env:BuildTargetName = "ui-desktop-3"
dotnet run --project ../../src/Omnius.Axus.Ui.Desktop/ -- -s ./storage/ui-desktop -l "tcp(ip4(127.0.0.1),43203)" -v true
