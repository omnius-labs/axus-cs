Set-Location $PSScriptRoot

$Env:BuildTargetName = "ui-desktop_2"
dotnet run --project ../../src/Omnius.Xeus.Ui.Desktop/ -- --config "./config.yml"
