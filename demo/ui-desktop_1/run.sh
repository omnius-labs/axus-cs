#!/bin/bash
cd $(dirname $0)

export BuildTargetName=ui-desktop_1
dotnet run --project ../../src/Omnius.Axis.Ui.Desktop/ -- --config "./config.yml"
