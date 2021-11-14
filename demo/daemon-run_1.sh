#!/bin/bash
cd $(dirname $0)

export BuildTargetName=daemon_1
dotnet run --project ../src/Omnius.Xeus.Daemon/ -- --config "./$BuildTargetName/config.yml" --storage "./$BuildTargetName/storage" --logs "./$BuildTargetName/logs" -v
