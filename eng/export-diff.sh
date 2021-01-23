#!/bin/bash

git diff --diff-filter=AMCRD HEAD | gzip -c | base64 > patch.txt
