#!/bin/bash
SCRIPT_DIR=$(cd $(dirname $0); pwd)

dotnet run --project ../src/Omnius.Xeus.Ui.Desktop/ -- "./demo/state/ui-desktop" ".demo/temp"
