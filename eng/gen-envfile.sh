#!/bin/bash
set -euo pipefail

rm -f .env

echo "LANG=en_US.UTF-8" >> .env

SCREEN=$(xrandr --listactivemonitors | awk -F " " '{ printf("%s", $4) }')
echo "AVALONIA_SCREEN_SCALE_FACTORS='$SCREEN=${SCREEN_SCALE:-1}'" >> .env
