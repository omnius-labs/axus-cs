setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set BIN_DIR=%cd%\bin\tools\win
set BUILD_ARCHITECTURE=win-x64

mkdir %BIN_DIR%\Omnix.Serialization.OmniPack.CodeGenerator

dotnet publish %cd%\refs\omnix\src\Omnix.Serialization.OmniPack.CodeGenerator\Omnix.Serialization.OmniPack.CodeGenerator.csproj --configuration Release --output "%BIN_DIR%\Omnix.Serialization.OmniPack.CodeGenerator" --runtime %BUILD_ARCHITECTURE%
