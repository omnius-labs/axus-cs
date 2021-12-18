#!/bin/bash

rm -rf ./pub/win-x64/bin
rm -rf ./pub/linux-x64/bin

dotnet publish ./src/Omnius.Axis.Daemon --configuration Release --runtime win-x64 --output ./pub/win-x64/bin/daemon
dotnet publish ./src/Omnius.Axis.Ui.Desktop --configuration Release --runtime win-x64 --output ./pub/win-x64/bin/ui-desktop
dotnet publish ./src/Omnius.Axis.Launcher --configuration Release --runtime win-x64 --output ./pub/win-x64/bin/launcher

dotnet publish ./src/Omnius.Axis.Daemon --configuration Release --runtime linux-x64 --output ./pub/linux-x64/bin/daemon
dotnet publish ./src/Omnius.Axis.Ui.Desktop --configuration Release --runtime linux-x64 --output ./pub/linux-x64/bin/ui-desktop
dotnet publish ./src/Omnius.Axis.Launcher --configuration Release --runtime linux-x64 --output ./pub/linux-x64/bin/launcher
