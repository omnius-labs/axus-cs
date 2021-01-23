#!/bin/bash

for name in $(dotnet tool list | awk 'NR>2{print $1}'); do
    dotnet tool update $name
done

dotnet tool restore
dotnet tool run dotnet-format
dotnet tool run nukeeper update
