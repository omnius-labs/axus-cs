#!/usr/env bash
set -euo pipefail

cd $(dirname $0)

export BuildTargetName=daemon-3
dotnet run --project ../../src/Omnius.Axus.Daemon/ -- -s ./storage/daemon -l "tcp(ip4(127.0.0.1),43203)" -v true
