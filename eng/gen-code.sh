#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

BIN_DIR="$PWD/bin/tools/linux"
TOOL_PATH="$BIN_DIR/Omnius.Core.Serialization.RocketPack.DefinitionCompiler/Omnius.Core.Serialization.RocketPack.DefinitionCompiler"
INCLUDE_1="$PWD/refs/core/fmt"
INCLUDE_2="$PWD/fmt"

"$TOOL_PATH" -f "$PWD/fmt/Omnius.Xeus.Service/Omnius.Xeus.Service.Models.rpd" -i "$INCLUDE_1" "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Service/Models/_RocketPack/Messages.generated.cs"
"$TOOL_PATH" -f "$PWD/fmt/Omnius.Xeus.Service.Impls/Omnius.Xeus.Service.Connectors.Internal.rpd" -i "$INCLUDE_1" "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Service.Impls/Connectors/Internal/_RocketPack/Messages.generated.cs"
"$TOOL_PATH" -f "$PWD/fmt/Omnius.Xeus.Service.Impls/Omnius.Xeus.Service.Engines.Internal.rpd" -i "$INCLUDE_1" "$INCLUDE_2" -o "$PWD/src/Omnius.Xeus.Service.Impls/Engines/Internal/_RocketPack/Messages.generated.cs"
