#!/bin/bash
set -euo pipefail

for name in $(dotnet tool list | awk 'NR>2{print $1}'); do
    dotnet tool update $name
done
