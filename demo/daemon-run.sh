#!/bin/bash
SCRIPT_DIR=$(cd $(dirname $0); pwd)

dotnet run --project ../src/Omnius.Xeus.Daemon/ -- run -s "./demo/state/daemon"
