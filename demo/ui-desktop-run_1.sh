#!/bin/bash
cd $(dirname $0)

export BuildTargetName=ui-desktop_1
dotnet run --project ../src/Omnius.Xeus.Ui.Desktop/ -- --config "./$BuildTargetName/config.yml" --storage "./$BuildTargetName/storage" --logs "./$BuildTargetName/logs" -v
