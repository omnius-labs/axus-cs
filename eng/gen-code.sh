#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

# *.tt
dotnet tool restore
dotnet tool run t4 ./src/Omnius.Axis.Ui.Desktop/Configuration/UiState.tt -o ./src/Omnius.Axis.Ui.Desktop/Configuration/UiState.generated.cs

# *.rpf
RPFC_PATH="$PWD/refs/core/src/Omnius.Core.RocketPack.DefinitionCompiler"
dotnet run --project $RPFC_PATH -- -c "$PWD/rpfs/config.yml"
