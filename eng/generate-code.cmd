setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set BIN_DIR=%cd%\bin\tools\win
set TOOL_PATH=%BIN_DIR%\Omnix.Serialization.OmniPack.CodeGenerator\Omnix.Serialization.OmniPack.CodeGenerator.exe
set INCLUDE=-i %cd%\fmt %cd%\refs\omnix\fmt

"%TOOL_PATH%" %cd%\fmt\Xeus.Core.rpf %INCLUDE% -o %cd%\src\Xeus.Core\_OmniPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Xeus.Core.Connectors.Internal.rpf %INCLUDE% -o %cd%\src\Xeus.Core.Connectors\Internal\_OmniPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Xeus.Core.Storages.Internal.rpf %INCLUDE% -o %cd%\src\Xeus.Core.Storages\Internal\_OmniPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Xeus.Core.Repositories.Internal.rpf %INCLUDE% -o %cd%\src\Xeus.Core.Repositories\Internal\_OmniPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Xeus.Core.Explorers.Internal.rpf %INCLUDE% -o %cd%\src\Xeus.Core.Explorers\Internal\_OmniPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Xeus.Core.Negotiators.Internal.rpf %INCLUDE% -o %cd%\src\Xeus.Core.Negotiators\Internal\_OmniPack\Messages.generated.cs
