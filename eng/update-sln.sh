#!/usr/env bash
set -euo pipefail

dotnet new sln --force -n axis
dotnet sln axis.sln add ./refs/core/src/**/*.csproj
dotnet sln axis.sln add ./refs/core/test/**/*.csproj
dotnet sln axis.sln add ./src/**/*.csproj
dotnet sln axis.sln add ./test/**/*.csproj
