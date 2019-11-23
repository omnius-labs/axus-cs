setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set BIN_DIR=%cd%\bin\tools\win
set TOOL_PATH=%BIN_DIR%\Omnius.Core.Serialization.RocketPack.DefinitionCompiler\Omnius.Core.Serialization.RocketPack.DefinitionCompiler.exe
set INCLUDE=-i %cd%\fmt %cd%\refs\core\fmt

"%TOOL_PATH%" %cd%\fmt\Omnius.Xeus.Engine.rpd %INCLUDE% -o %cd%\src\Omnius.Xeus.Engine\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Omnius.Xeus.Engine.Connectors.Internal.rpd %INCLUDE% -o %cd%\src\Omnius.Xeus.Engine.Connectors\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Omnius.Xeus.Engine.Storages.Internal.rpd %INCLUDE% -o %cd%\src\Omnius.Xeus.Engine.Storages\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Omnius.Xeus.Engine.Explorers.Internal.rpd %INCLUDE% -o %cd%\src\Omnius.Xeus.Engine.Explorers\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Omnius.Xeus.Engine.Negotiators.Internal.rpd %INCLUDE% -o %cd%\src\Omnius.Xeus.Engine.Negotiators\Internal\_RocketPack\Messages.generated.cs
