#!/bin/bash

dotnet run --project ./src/Omnius.Xeus.Daemon/ -- run -c ./demo/.config/daemon -s ./demo/.state/daemon
