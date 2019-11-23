setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set BIN_DIR=%cd%\bin\tools\win
set BUILD_ARCHITECTURE=win-x64

mkdir %BIN_DIR%\Omnius.Core.Serialization.RocketPack.DefinitionCompiler

dotnet publish %cd%\refs\core\src\Omnius.Core.Serialization.RocketPack.DefinitionCompiler\Omnius.Core.Serialization.RocketPack.DefinitionCompiler.csproj --configuration Release --output "%BIN_DIR%\Omnius.Core.Serialization.RocketPack.DefinitionCompiler" --runtime %BUILD_ARCHITECTURE%
