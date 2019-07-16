dotnet tool install --global coverlet.console
dotnet tool update --global coverlet.console

ForEach ($folder in (Get-ChildItem -Path "tests" -Directory)) 
{
    $path = $folder.FullName;
    $name = $folder.Name;
    $output = "../../tmp/tests/win/${name}.opencover.xml";

    dotnet test "$path" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="$output" /p:Exclude="[xunit*]*%2c[*.Tests]*" -v:n;
   
	if (!$?) {
        exit 1
    }
}

dotnet tool install dotnet-reportgenerator-globaltool --tool-path tools/win/
.\tools\win\reportgenerator.exe "--reports:tmp/tests/win/*.opencover.xml" "--targetdir:publish/code-coverage/win"
