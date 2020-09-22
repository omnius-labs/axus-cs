#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

BIN_DIR="$PWD/bin/tools/linux"
TOOL_PATH="$BIN_DIR/Omnius.Core.RocketPack.DefinitionCompiler/Omnius.Core.RocketPack.DefinitionCompiler"
INCLUDE_1="$PWD/refs/core/fmt/**/*.rpd"
INCLUDE_2="$PWD/fmt/**/*.rpd"

"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Components/Omnius.Xeus.Components.Models.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Components/Models/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Components/Omnius.Xeus.Components.Connectors.Internal.Models.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Components/Connectors/Internal/Models/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Components/Omnius.Xeus.Components.Storages.Internal.Models.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Components/Storages/Internal/Models/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Components/Omnius.Xeus.Components.Engines.Internal.Models.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Components/Engines/Internal/Models/_RocketPack/_Generated.cs"

"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Service/Omnius.Xeus.Service.Models.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Service/Models/_RocketPack/_Generated.cs"
"$TOOL_PATH" compile -s "$PWD/fmt/Omnius.Xeus.Service/Omnius.Xeus.Service.rpd" -i "$INCLUDE_1" -i "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Service/_RocketPack/_Generated.cs"
