setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

if %PROCESSOR_ARCHITECTURE% == x86 (
    set TOOLS_DIR=%cd%\tools\win-x86\
    set BUILD_ARCHITECTURE=win-x86
) 

if %PROCESSOR_ARCHITECTURE% == AMD64 (
    set TOOLS_DIR=%cd%\tools\win-x64\
    set BUILD_ARCHITECTURE=win-x64
)

mkdir %TOOLS_DIR%Omnix.Serialization.RocketPack.CodeGenerator

dotnet publish %cd%\refs\Omnix\src\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.csproj --configuration Release --output "%TOOLS_DIR%Omnix.Serialization.RocketPack.CodeGenerator" --runtime %BUILD_ARCHITECTURE%
