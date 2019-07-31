setlocal

setx DOTNET_CLI_TELEMETRY_OPTOUT 1

set TOOL_PATH=%cd%\tools\win\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.exe

"%TOOL_PATH%" %cd%\formats\Xeus.Messages.rpf %cd%\src\Xeus.Messages\_RocketPack\Messages.generated.cs

"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.rpf %cd%\src\Xeus.Core\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Connection.rpf %cd%\src\Xeus.Core\Internal\Connection\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Content.rpf %cd%\src\Xeus.Core\Internal\Content\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %cd%\formats\Xeus.Core.Internal.Exchange.rpf %cd%\src\Xeus.Core\Internal\Exchange\_RocketPack\Messages.generated.cs
