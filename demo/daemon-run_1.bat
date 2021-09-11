cd /d %~dp0

set BuildTargetName=daemon_1
dotnet run --project ../src/Omnius.Xeus.Service.Daemon/ -- "./%BuildTargetName%/state" "./%BuildTargetName%/logs"
