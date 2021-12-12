Set-Location $PSScriptRoot

$Env:BuildTargetName = "ui-desktop_1"
dotnet run --project ../../src/Omnius.Axis.Ui.Desktop/ -- --config "./config.yml"
