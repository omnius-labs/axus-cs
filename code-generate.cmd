setlocal

set BAT_DIR=%~dp0

if %PROCESSOR_ARCHITECTURE% == x86 (
    set TOOL_PATH=%BAT_DIR%tools\win-x86\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.exe
)

if %PROCESSOR_ARCHITECTURE% == AMD64 (
    set TOOL_PATH=%BAT_DIR%tools\win-x64\Omnix.Serialization.RocketPack.CodeGenerator\Omnix.Serialization.RocketPack.CodeGenerator.exe
)

"%TOOL_PATH%" %BAT_DIR%formats\Xeus.Messages.rpf %BAT_DIR%src\Xeus.Messages\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %BAT_DIR%formats\Xeus.Messages.Reports.rpf %BAT_DIR%src\Xeus.Messages\Reports\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %BAT_DIR%formats\Xeus.Messages.Options.rpf %BAT_DIR%src\Xeus.Messages\Options\_RocketPack\Messages.generated.cs

"%TOOL_PATH%" %BAT_DIR%formats\Xeus.Core.Internal.rpf %BAT_DIR%src\Xeus.Core\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %BAT_DIR%formats\Xeus.Core.Contents.Internal.rpf %BAT_DIR%src\Xeus.Core\Contents\Internal\_RocketPack\Messages.generated.cs
"%TOOL_PATH%" %BAT_DIR%formats\Xeus.Core.Exchange.Internal.rpf %BAT_DIR%src\Xeus.Core\Exchange\Internal\_RocketPack\Messages.generated.cs
