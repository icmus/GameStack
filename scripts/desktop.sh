#!/bin/bash

# This script can be called from your project to copy dependencies to your build
# folder and run the necessary import commands. In Xamarin Studio or MonoDevelop,
# add a custom AfterBuildCommand:
#
# /path/to/Gamestack/scripts/desktop.sh ${TargetDir} [content-folder] [PROPERTY=VALUE]...
#
# Content will be processed and written to ${TargetDir}/assets.
SRC=`dirname $0`
CONTENT=$1
TGT=$2
shift 2

if [ -z "$TGT" ]; then exit; fi
cp -f "$SRC"/../lib/liblodepng.* "$SRC"/../lib/*.config "$TGT"
mkdir -p "$TGT"/Assets
mono "$SRC"/../bin/import.exe "$CONTENT" "$TGT"/Assets "$@"
