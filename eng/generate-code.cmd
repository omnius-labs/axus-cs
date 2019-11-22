setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set BIN_DIR=%cd%\bin\tools\win
set TOOL_PATH=%BIN_DIR%\Omnix.Serialization.RocketPack.DefinitionCompiler\Omnix.Serialization.RocketPack.DefinitionCompiler.exe
set INCLUDE=-i %cd%\fmt %cd%\refs\omnix\fmt

"%TOOL_PATH%" %cd%\fmt\Xeus.Engine.rpd %INCLUDE% -o %cd%\src\Xeus.Engine\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Xeus.Engine.Connectors.Internal.rpd %INCLUDE% -o %cd%\src\Xeus.Engine.Connectors\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Xeus.Engine.Storages.Internal.rpd %INCLUDE% -o %cd%\src\Xeus.Engine.Storages\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Xeus.Engine.Explorers.Internal.rpd %INCLUDE% -o %cd%\src\Xeus.Engine.Explorers\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\fmt\Xeus.Engine.Negotiators.Internal.rpd %INCLUDE% -o %cd%\src\Xeus.Engine.Negotiators\Internal\_RocketPack\Messages.generated.cs
