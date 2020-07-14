#!/bin/bash

git diff --diff-filter=AMCR HEAD | gzip -c | base64 > patch.txt
