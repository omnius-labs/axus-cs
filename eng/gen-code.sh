#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

BIN_DIR=$PWD/bin/tools/linux
TOOL_PATH=$BIN_DIR/Omnius.Core.Serialization.RocketPack.DefinitionCompiler/Omnius.Core.Serialization.RocketPack.DefinitionCompiler
INCLUDE="-i $PWD/fmt $PWD/refs/core/fmt"

"$TOOL_PATH" $PWD/fmt/Omnius.Xeus.Service/Omnius.Xeus.Service.Drivers.rpd $INCLUDE -o $PWD/src/Omnius.Xeus.Service/Drivers/_RocketPack/Messages.generated.cs
"$TOOL_PATH" $PWD/fmt/Omnius.Xeus.Service/Omnius.Xeus.Service.Engines.rpd $INCLUDE -o $PWD/src/Omnius.Xeus.Service/Engines/_RocketPack/Messages.generated.cs
"$TOOL_PATH" $PWD/fmt/Omnius.Xeus.Service.Implements/Omnius.Xeus.Service.Drivers.Internal.rpd $INCLUDE -o $PWD/src/Omnius.Xeus.Service.Implements/Drivers/Internal/_RocketPack/Messages.generated.cs
"$TOOL_PATH" $PWD/fmt/Omnius.Xeus.Service.Implements/Omnius.Xeus.Service.Engines.Internal.rpd $INCLUDE -o $PWD/src/Omnius.Xeus.Service.Implements/Engines/Internal/_RocketPack/Messages.generated.cs
