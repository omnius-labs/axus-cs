#!/usr/env bash
set -euo pipefail

dotnet new sln --force -n axus
dotnet sln axus.sln add ./refs/core/src/**/*.csproj
dotnet sln axus.sln add ./refs/core/test/**/*.csproj
dotnet sln axus.sln add ./src/**/*.csproj
dotnet sln axus.sln add ./test/**/*.csproj
