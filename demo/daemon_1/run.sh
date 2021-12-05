#!/bin/bash
cd $(dirname $0)

export BuildTargetName=daemon_1
dotnet run --project ../../src/Omnius.Xeus.Daemon/ -- --config "./config.yml"
