dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool

ForEach ($folder in (Get-ChildItem -Path "tests" -Directory)) 
{
    dotnet test $folder.FullName /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="../TestResults/$folder-opencover.xml";
}

reportgenerator "--reports:tests/TestResults/*-opencover.xml" "--targetdir:tests/TestResults/html"
