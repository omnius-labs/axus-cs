#!/bin/bash
cd $(dirname $0)

export BuildTargetName=daemon_2
dotnet run --project ../src/Omnius.Xeus.Service.Daemon/ -- --config "./$BuildTargetName/config.yml" --storage "./$BuildTargetName/storage" --logs "./$BuildTargetName/logs" -v
