#!/usr/env bash
set -euo pipefail

rm -rf ./pub/linux-x64/*

export NativeDepsPlatform=linux
export PlatformTarget=x64

dotnet publish ./src/Omnius.Axus.Daemon -p:PublishSingleFile=true --runtime linux-x64 --configuration Release --self-contained true --output ./pub/linux-x64/bin/daemon
dotnet publish ./src/Omnius.Axus.Ui.Desktop -p:PublishSingleFile=true --runtime linux-x64 --configuration Release --self-contained true --output ./pub/linux-x64/bin/ui-desktop
dotnet publish ./src/Omnius.Axus.Launcher -p:PublishSingleFile=true -p:PublishTrimmed=true --runtime linux-x64 --configuration Release --self-contained true --output ./pub/linux-x64/
