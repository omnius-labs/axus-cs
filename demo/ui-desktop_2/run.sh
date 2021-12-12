#!/bin/bash
cd $(dirname $0)

export BuildTargetName=ui-desktop_2
dotnet run --project ../../src/Omnius.Axis.Ui.Desktop/ -- --config "./config.yml"
