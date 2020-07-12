#!/bin/bash

DOTNET_CLI_TELEMETRY_OPTOUT=1

BIN_DIR="$PWD/bin/tools/linux"
BUILD_ARCHITECTURE=linux-x64

mkdir "$BIN_DIR/Omnius.Core.Serialization.RocketPack.DefinitionCompiler"
dotnet publish "$PWD/refs/core/src/Omnius.Core.Serialization.RocketPack.DefinitionCompiler/Omnius.Core.Serialization.RocketPack.DefinitionCompiler.csproj" --configuration Release --output "$BIN_DIR/Omnius.Core.Serialization.RocketPack.DefinitionCompiler" --runtime $BUILD_ARCHITECTURE
