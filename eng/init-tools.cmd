setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set TOOLS_DIR=%cd%\tools\win\
set BUILD_ARCHITECTURE=win-x64

mkdir %TOOLS_DIR%Omnix.Serialization.RocketPack.CodeGenerator

dotnet publish %cd%\refs\omnix\src\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.csproj --configuration Release --output "%TOOLS_DIR%Omnix.Serialization.RocketPack.CodeGenerator" --runtime %BUILD_ARCHITECTURE%
