setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set TOOL_PATH=%cd%\tools\win\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.exe
set INCLUDE=-i %cd%\formats %cd%\refs\omnix\formats

"%TOOL_PATH%" %cd%\formats\Xeus.Messages.rpf %INCLUDE% -o %cd%\src\Xeus.Messages\_RocketPack\Messages.generated.cs

"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.rpf %INCLUDE% -o %cd%\src\Xeus.Core\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Connection.rpf %INCLUDE% -o %cd%\src\Xeus.Core\Internal\Connection\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Content.rpf %INCLUDE% -o %cd%\src\Xeus.Core\Internal\Content\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Network.rpf %INCLUDE% -o %cd%\src\Xeus.Core\Internal\Network\_RocketPack\Messages.generated.cs
