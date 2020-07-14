#!/bin/bash

cat patch.txt | base64 -d | gzip -d | git apply
