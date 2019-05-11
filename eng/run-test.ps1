dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool

ForEach ($folder in (Get-ChildItem -Path "tests" -Directory)) 
{
    dotnet test $folder.FullName /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="../TestResults/$folder-opencover.xml" /p:Exclude="[xunit*]*%2c[*.Tests]*" -v:n;
}

reportgenerator.exe "--reports:tests/TestResults/*-opencover.xml" "--targetdir:tests/TestResults/html"
