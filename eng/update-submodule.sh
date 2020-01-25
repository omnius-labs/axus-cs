#!/bin/bash

branch=$(git symbolic-ref --short HEAD)

if [ $branch = "master" ]; then
  branch=master
else
  branch=develop
fi

(cd refs/core && git checkout $branch && git pull)
