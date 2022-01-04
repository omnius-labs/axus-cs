#!/usr/env bash
set -euo pipefail

DOTNET_CLI_TELEMETRY_OPTOUT=1

# *.rpf
RPFC_PATH="$PWD/refs/core/src/Omnius.Core.RocketPack.DefinitionCompiler"
dotnet run --project $RPFC_PATH -- -c "$PWD/rpfs/config.yml"
