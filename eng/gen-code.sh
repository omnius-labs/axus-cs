#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

BIN_DIR="$PWD/bin/tools/linux"
TOOL_PATH="$BIN_DIR/Omnius.Core.RocketPack.DefinitionCompiler/Omnius.Core.RocketPack.DefinitionCompiler"
INCLUDE_1="$PWD/refs/core/fmt/**/*.rpd"
INCLUDE_2="$PWD/fmt/**/*.rpd"

"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Rpc/Omnius.Xeus.Rpc.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Rpc/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Components/Omnius.Xeus.Components.Models.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Components/Models/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Components.Impls/Omnius.Xeus.Components.Connectors.Internal.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Components.Impls/Connectors/Internal/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Components.Impls/Omnius.Xeus.Components.Storages.Internal.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Components.Impls/Storages/Internal/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Components.Impls/Omnius.Xeus.Components.Engines.Internal.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Components.Impls/Engines/Internal/_RocketPack/_Generated.cs"
