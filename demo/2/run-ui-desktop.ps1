Set-Location $PSScriptRoot

$Env:BuildTargetName = "ui-desktop-2"
dotnet run --project ../../src/Omnius.Axus.Ui.Desktop/ -- -s ./storage/ui-desktop -l "tcp(ip4(127.0.0.1),43202)" -v true
