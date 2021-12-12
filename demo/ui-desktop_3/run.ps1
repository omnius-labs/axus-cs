Set-Location $PSScriptRoot

$Env:BuildTargetName = "ui-desktop_3"
dotnet run --project ../../src/Omnius.Axis.Ui.Desktop/ -- --config "./config.yml"
