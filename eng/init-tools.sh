#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

BIN_DIR="$PWD/bin/tools/linux"
BUILD_ARCHITECTURE=linux-x64

mkdir "$BIN_DIR/Omnius.Core.RocketPack.DefinitionCompiler"
dotnet publish "$PWD/refs/core/src/Omnius.Core.RocketPack.DefinitionCompiler/Omnius.Core.RocketPack.DefinitionCompiler.csproj" --configuration Release --output "$BIN_DIR/Omnius.Core.RocketPack.DefinitionCompiler" --runtime $BUILD_ARCHITECTURE
