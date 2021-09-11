cd /d %~dp0

set BuildTargetName=ui-desktop_2
dotnet run --project ../src/Omnius.Xeus.Ui.Desktop/ -- "./%BuildTargetName%/state" "./%BuildTargetName%/logs"
