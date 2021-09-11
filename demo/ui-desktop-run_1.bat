cd /d %~dp0

set BuildTargetName=ui-desktop_1
dotnet run --project ../src/Omnius.Xeus.Ui.Desktop/ -- "./%BuildTargetName%/state" "./%BuildTargetName%/logs"
