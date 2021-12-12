#!/bin/bash
cd $(dirname $0)

export BuildTargetName=daemon_1
dotnet run --project ../../src/Omnius.Axis.Daemon/ -- --config "./config.yml"
