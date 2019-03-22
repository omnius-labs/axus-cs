setlocal

set BAT_DIR=%~dp0
cd %BAT_DIR%

powershell -File ./run-test.ps1