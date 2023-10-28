#!/bin/bash
set -euo pipefail

git diff --diff-filter=AMCRD HEAD | gzip -c | base64 > patch.txt
