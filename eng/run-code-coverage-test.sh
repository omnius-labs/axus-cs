dotnet tool install --global coverlet.console
dotnet tool update --global coverlet.console

for path in `find "test" -maxdepth 2 -type f -name "*.csproj"`
do
    name=$(basename ${path%.*})
    output="../../tmp/test/linux/${name}.opencover.xml"
    dotnet test "$path" -p:CollectCoverage=true -p:CoverletOutputFormat=opencover -p:CoverletOutput="$output" -p:Exclude="[xunit*]*%2c[*.Tests]*" -v:n;

    ret=$?

    if [ $ret -gt 0 ]; then
        exit $ret
    fi
done

dotnet tool install dotnet-reportgenerator-globaltool --tool-path tmp\tools/linux/
./tmp/tools/linux/reportgenerator "--reports:tmp/test/linux/*.opencover.xml" "--targetdir:publish/code-coverage/linux"
