#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

BIN_DIR="$PWD/bin/tools/linux"
TOOL_PATH="$BIN_DIR/Omnius.Core.Serialization.RocketPack.DefinitionCompiler/Omnius.Core.Serialization.RocketPack.DefinitionCompiler"
INCLUDE_1="$PWD/refs/core/fmt"
INCLUDE_2="$PWD/fmt"

"$TOOL_PATH" -f "$PWD/fmt/Omnius.Xeus.Service/Omnius.Xeus.Service.Drivers.rpd" -i "$INCLUDE_1" "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Service/Drivers/_RocketPack/Messages.generated.cs"
"$TOOL_PATH" -f "$PWD/fmt/Omnius.Xeus.Service/Omnius.Xeus.Service.Engines.rpd" -i "$INCLUDE_1" "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Service/Engines/_RocketPack/Messages.generated.cs"
"$TOOL_PATH" -f "$PWD/fmt/Omnius.Xeus.Service.Impls/Omnius.Xeus.Service.Drivers.Internal.rpd" -i "$INCLUDE_1" "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Service.Impls/Drivers/Internal/_RocketPack/Messages.generated.cs"
"$TOOL_PATH" -f "$PWD/fmt/Omnius.Xeus.Service.Impls/Omnius.Xeus.Service.Engines.Internal.rpd" -i "$INCLUDE_1" "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Service.Impls/Engines/Internal/_RocketPack/Messages.generated.cs"
