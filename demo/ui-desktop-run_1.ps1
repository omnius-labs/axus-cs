Set-Location $PSScriptRoot

$Env:BuildTargetName = "ui-desktop_1"
dotnet run --project ../src/Omnius.Xeus.Ui.Desktop/ -- --config "./$Env:BuildTargetName/config.yml" --storage "./$Env:BuildTargetName/storage" --logs "./$Env:BuildTargetName/logs" -v
