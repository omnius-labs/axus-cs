Set-Location $PSScriptRoot

$Env:BuildTargetName = "ui-desktop_2"
dotnet run --project ../src/Omnius.Xeus.Ui.Desktop/ -- "./$Env:BuildTargetName/state" "./$Env:BuildTargetName/logs"
