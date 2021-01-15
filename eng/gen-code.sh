#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

BIN_DIR="$PWD/bin/tools/linux"
TOOL_PATH="$BIN_DIR/Omnius.Core.RocketPack.DefinitionCompiler/Omnius.Core.RocketPack.DefinitionCompiler"
INCLUDE_1="$PWD/refs/core/rpfs/**/*.rpf"
INCLUDE_2="$PWD/rpfs/**/*.rpf"

"$TOOL_PATH" compile -s "$PWD/rpfs/Omnius.Xeus.Engines/Omnius.Xeus.Engines.Models.rpf" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Engines/Models/_RocketPack/_Generated.cs"

"$TOOL_PATH" compile -s "$PWD/rpfs/Omnius.Xeus.Engines.Implementations/Omnius.Xeus.Engines.Connectors.Internal.Models.rpf" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Engines.Implementations/Connectors/Internal/Models/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/rpfs/Omnius.Xeus.Engines.Implementations/Omnius.Xeus.Engines.Storages.Internal.Models.rpf" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Engines.Implementations/Storages/Internal/Models/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/rpfs/Omnius.Xeus.Engines.Implementations/Omnius.Xeus.Engines.Mediators.Internal.Models.rpf" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Engines.Implementations/Mediators/Internal/Models/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/rpfs/Omnius.Xeus.Engines.Implementations/Omnius.Xeus.Engines.Exchangers.Internal.Models.rpf" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Engines.Implementations/Exchangers/Internal/Models/_RocketPack/_Generated.cs"

"$TOOL_PATH" compile -s "$PWD/rpfs/Omnius.Xeus.Api/Omnius.Xeus.Api.rpf" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Api/_RocketPack/_Generated.cs"

"$TOOL_PATH" compile -s "$PWD/rpfs/Omnius.Xeus.Interactors/Omnius.Xeus.Interactors.Models.rpf" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Interactors/Models/_RocketPack/_Generated.cs"
