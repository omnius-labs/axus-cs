setx DOTNET_CLI_TELEMETRY_OPTOUT 1

setlocal

set BAT_DIR=%~dp0

if %PROCESSOR_ARCHITECTURE% == x86 (
    set TOOLS_DIR=%BAT_DIR%tools\win-x86\
    set BUILD_ARCHITECTURE=win-x86
) 

if %PROCESSOR_ARCHITECTURE% == AMD64 (
    set TOOLS_DIR=%BAT_DIR%tools\win-x64\
    set BUILD_ARCHITECTURE=win-x64
)

mkdir %TOOLS_DIR%Omnix.Serialization.RocketPack.CodeGenerator

dotnet publish %BAT_DIR%refs\Omnix\src\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.csproj --configuration Release --output "%TOOLS_DIR%Omnix.Serialization.RocketPack.CodeGenerator" --runtime %BUILD_ARCHITECTURE%
