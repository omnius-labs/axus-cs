#!/bin/bash
cd $(dirname $0)

export BuildTargetName=ui-desktop_3
dotnet run --project ../../src/Omnius.Xeus.Ui.Desktop/ -- --config "./config.yml"
