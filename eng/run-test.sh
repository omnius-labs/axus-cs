#!/bin/bash

dotnet tool restore

for path in `find "test" -maxdepth 2 -type f -name "*.csproj"`
do
    name=$(basename ${path%.*})
    output="../../tmp/test/linux/${name}.opencover.xml"
    dotnet test "$path" -p:CollectCoverage=true -p:CoverletOutputFormat=opencover -p:CoverletOutput="$output" -p:Exclude="[xunit*]*%2c[*.Tests]*";

    ret=$?

    if [ $ret -gt 0 ]; then
        exit $ret
    fi
done

dotnet tool run reportgenerator "--reports:tmp/test/linux/*.opencover.xml" "--targetdir:publish/code-coverage/linux"
