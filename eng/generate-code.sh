#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

BIN_DIR=$PWD/bin/tools/linux
TOOL_PATH=$BIN_DIR/Omnius.Core.Serialization.RocketPack.DefinitionCompiler/Omnius.Core.Serialization.RocketPack.DefinitionCompiler
INCLUDE="-i $PWD/fmt $PWD/refs/core/fmt"

"$TOOL_PATH" $PWD/fmt/Omnius.Xeus.Engine.rpd $INCLUDE -o $PWD/src/Omnius.Xeus.Engine/_RocketPack/Messages.generated.cs
"$TOOL_PATH" $PWD/fmt/Omnius.Xeus.Engine.Implements.Internal.rpd $INCLUDE -o $PWD/src/Omnius.Xeus.Engine.Implements/Internal/_RocketPack/Messages.generated.cs
