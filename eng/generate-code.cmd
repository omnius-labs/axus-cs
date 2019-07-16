setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set TOOL_PATH=%cd%\tools\win\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.exe

"%TOOL_PATH%" %cd%\formats\Xeus.Messages.rpf %cd%\src\Xeus.Messages\_RocketPack\Messages.generated.cs

"%TOOL_PATH%" %cd%\formats\Xeus.Core.Connections.Internal.rpf %cd%\src\Xeus.Core\Connections\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.rpf %cd%\src\Xeus.Core\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Contents.Internal.rpf %cd%\src\Xeus.Core\Contents\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Exchange.Internal.rpf %cd%\src\Xeus.Core\Exchange\Internal\_RocketPack\Messages.generated.cs
