#!/bin/bash

# This script can be called from your project to copy dependencies to your build
# folder and run the necessary import commands. In Xamarin Studio or MonoDevelop,
# add a custom AfterBuildCommand:
#
# /path/to/Gamestack/scripts/desktop.sh [content-folder] ${TargetDir} [PROPERTY=VALUE]...
#
# Content will be processed and written to ${TargetDir}/assets.
BIN=`dirname $0`
CONTENT=$1
TGT=$2
EXT=so
shift 2

if [ ! -f "$BIN"/import.exe ]; then
    [ -f "$BIN"/../bin/import.exe ] && BIN="$BIN/../bin"
fi

[ `uname` == 'Darwin' ] && EXT=dylib;

mkdir -p "$TGT"/assets
cp -f "$BIN"/desktop/*.{dll,dll.mdb,dll.config,$EXT} "$TGT"
cd "$BIN"
mono import.exe "$CONTENT" "$TGT"/assets "$@" 
