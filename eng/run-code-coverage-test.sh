apt-get update
apt-get install -y --no-install-recommends libc6-dev

dotnet tool install --global coverlet.console
dotnet tool update --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet tool update --global dotnet-reportgenerator-globaltool

for path in `find "tests" -maxdepth 2 -type f -name "*.csproj"`
do
    output="../results/$(basename ${path%.*})-opencover.xml"
    dotnet test "$path" -p:CollectCoverage=true -p:CoverletOutputFormat=opencover -p:CoverletOutput=$output;
done

reportgenerator "--reports:tests/results/*-opencover.xml" "--targetdir:tests/results/html"
