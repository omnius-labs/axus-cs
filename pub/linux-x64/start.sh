#!/usr/bin/env bash

BASEDIR=$(dirname "$0")
WORKING_DIRECTORY=$BASEDIR"/storage/launcher"

mkdir -p $WORKING_DIRECTORY
cd $WORKING_DIRECTORY

../../bin/launcher/Omnius.Axis.Launcher --config config.yml
