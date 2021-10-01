#!/bin/bash
cd $(dirname $0)

export BuildTargetName=ui-desktop_2
dotnet run --project ../src/Omnius.Xeus.Ui.Desktop/ -- "./$BuildTargetName/state" "./$BuildTargetName/logs"
