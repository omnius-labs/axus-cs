#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

# *.tt
dotnet tool restore
dotnet tool run t4 ./src/Omnius.Xeus.Ui.Desktop/Resources/Models/UiSettings.tt -o ./src/Omnius.Xeus.Ui.Desktop/Resources/Models/UiSettings.generated.cs

# *.rpf
RPFC_PATH="$PWD/refs/core/src/Omnius.Core.RocketPack.DefinitionCompiler"
dotnet run -p $RPFC_PATH -- -c "$PWD/rpfs/config.yml"
