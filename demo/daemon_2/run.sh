#!/bin/bash
cd $(dirname $0)

export BuildTargetName=daemon_2
dotnet run --project ../../src/Omnius.Xeus.Daemon/ -- --config "./config.yml"
