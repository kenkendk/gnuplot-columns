#!/bin/bash

#Use this file if you have installed columns to /opt/local/lib/columns

export LD_LIBRARY_PATH="/opt/local/lib/columns${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
export MONO_PATH=$MONO_PATH:/opt/local/lib/columns

EXE_FILE=/opt/local/lib/columns/columns.exe
APP_NAME=columns

exec -a "$APP_NAME" mono "$EXE_FILE" "$@"
