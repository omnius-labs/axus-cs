setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set TOOL_PATH=%cd%\tools\win\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.exe

"%TOOL_PATH%" %cd%\formats\Xeus.Messages.rpf %cd%\src\Xeus.Messages\_RocketPack\Messages.generated.cs

"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.rpf %cd%\src\Xeus.Core\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Connections.rpf %cd%\src\Xeus.Core\Internal\Connections\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Contents.rpf %cd%\src\Xeus.Core\Internal\Contents\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Exchange.rpf %cd%\src\Xeus.Core\Internal\Exchange\_RocketPack\Messages.generated.cs
