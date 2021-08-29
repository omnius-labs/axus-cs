#!/bin/bash
cd $(dirname $0)

dotnet run --project ../src/Omnius.Xeus.Service.Daemon.Implementations/ -- "./daemon_3/state" "./daemon_3/logs"
