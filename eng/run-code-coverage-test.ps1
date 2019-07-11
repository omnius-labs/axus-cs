dotnet tool install --global coverlet.console
dotnet tool update --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet tool update --global dotnet-reportgenerator-globaltool

ForEach ($folder in (Get-ChildItem -Path "tests" -Directory)) 
{
    dotnet test $folder.FullName /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="../results/$folder-opencover.xml" /p:Exclude="[xunit*]*%2c[*.Tests]*" -v:n;
}

reportgenerator.exe "--reports:tests/results/*-opencover.xml" "--targetdir:tests/results/html"
