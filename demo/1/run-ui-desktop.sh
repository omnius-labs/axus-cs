#!/usr/env bash
set -euo pipefail

cd $(dirname $0)

export BuildTargetName=ui-desktop-1
dotnet run --project ../../src/Omnius.Axus.Ui.Desktop/ -- -s ./storage/ui-desktop -l "tcp(ip4(127.0.0.1),43201)" -v true
