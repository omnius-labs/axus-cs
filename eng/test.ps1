dotnet tool restore

$Env:ContinuousIntegrationBuild = "true"

ForEach ($folder in (Get-ChildItem -Path "test" -Directory)) {
    $path = $folder.FullName;
    $name = $folder.Name;
    $output = "../../tmp/test/win/${name}.opencover.xml";

    dotnet test "$path" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="$output" /p:Exclude="[xunit*]*%2c[*.Tests]*";

	if (!$?) {
        exit 1
    }
}

dotnet tool run reportgenerator "--reports:tmp/test/win/*.opencover.xml" "--targetdir:pub/code-coverage/win"
