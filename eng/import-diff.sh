#!/usr/env bash
set -euo pipefail

cat patch.txt | base64 -d | gzip -d | git apply
