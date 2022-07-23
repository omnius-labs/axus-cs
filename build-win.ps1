Remove-Item ./pub/win-x64/* -Recurse

$Env:NativeDepsPlatform="Windows"
$Env:PlatformTarget="x64"

dotnet publish ./src/Omnius.Axus.Daemon -p:PublishSingleFile=true --runtime win-x64 --configuration Release --self-contained true --output ./pub/win-x64/bin/daemon
dotnet publish ./src/Omnius.Axus.Ui.Desktop -p:PublishSingleFile=true --runtime win-x64 --configuration Release --self-contained true --output ./pub/win-x64/bin/ui-desktop
dotnet publish ./src/Omnius.Axus.Launcher -p:PublishSingleFile=true -p:PublishTrimmed=true --runtime win-x64 --configuration Release --self-contained true --output ./pub/win-x64/
