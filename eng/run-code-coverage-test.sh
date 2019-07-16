dotnet tool install --global coverlet.console
dotnet tool update --global coverlet.console

for path in `find "tests" -maxdepth 2 -type f -name "*.csproj"`
do
    name=$(basename ${path%.*})
    output="../../tmp/tests/linux/${name}.opencover.xml"
    dotnet test "$path" -p:CollectCoverage=true -p:CoverletOutputFormat=opencover -p:CoverletOutput="$output" -p:Exclude="[xunit*]*%2c[*.Tests]*" -v:n;

    ret=$?

    if [ $ret -gt 0 ]; then
        exit $ret
    fi
done

dotnet tool install dotnet-reportgenerator-globaltool --tool-path tools/linux/
./tools/linux/reportgenerator "--reports:tmp/tests/linux/*.opencover.xml" "--targetdir:publish/code-coverage/linux"
