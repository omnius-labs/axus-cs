#!/bin/bash
cd $(dirname $0)

dotnet run --project ../src/Omnius.Xeus.Daemon.Implementations/ -- "./daemon_1/state" "./daemon_1/logs"
