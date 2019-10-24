setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set BIN_DIR=%cd%\bin\tools\win
set BUILD_ARCHITECTURE=win-x64

mkdir %BIN_DIR%\Omnix.Serialization.RocketPack.DefinitionCompiler

dotnet publish %cd%\refs\omnix\src\Omnix.Serialization.RocketPack.DefinitionCompiler\Omnix.Serialization.RocketPack.DefinitionCompiler.csproj --configuration Release --output "%BIN_DIR%\Omnix.Serialization.RocketPack.DefinitionCompiler" --runtime %BUILD_ARCHITECTURE%
