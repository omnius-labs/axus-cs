$WorkingDirectory = $PSScriptRoot + "/storage/launcher"

New-Item $WorkingDirectory -ItemType Directory -ErrorAction SilentlyContinue
Set-Location $WorkingDirectory

Start-Process ../../bin/launcher/Omnius.Axis.Launcher.exe '--config config.yml'
