setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set TOOL_PATH=%cd%\tools\win\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.exe
set INCLUDE=-i %cd%\formats %cd%\refs\omnix\formats

"%TOOL_PATH%" %cd%\formats\Xeus.Messages.rpf %INCLUDE% -o %cd%\src\Xeus.Messages\_RocketPack\Messages.generated.cs

"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Common.rpf %INCLUDE% -o %cd%\src\Xeus.Core\Internal\Common\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Connection.rpf %INCLUDE% -o %cd%\src\Xeus.Core\Internal\Connection\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Storage.rpf %INCLUDE% -o %cd%\src\Xeus.Core\Internal\Storage\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Search.rpf %INCLUDE% -o %cd%\src\Xeus.Core\Internal\Search\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Swap.rpf %INCLUDE% -o %cd%\src\Xeus.Core\Internal\Swap\_RocketPack\Messages.generated.cs
