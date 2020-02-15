#!/bin/bash

if [ $# -eq 2 ]; then
    MY_DIR="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
    # export MSBUILD_TRACKSDK_PATH=$MY_DIR/MSBuild/TrackerSDK/
    # export MSBUILD_TRACKFRAMEWORK_PATH=$MY_DIR/MSBuild/TrackerFramework/
    dotnet build -v:m $MY_DIR/PTSystem_PosixAndroid.vcxproj -property:VCTargetsPath=$MY_DIR/../MSBuild/VCTargets/ -property:CLTrackerSdkPath=$MY_DIR/../MSBuild/TrackerSDK/ -property:CLTrackerFrameworkPath=$MY_DIR/../MSBuild/TrackerFramework/ -property:LinkTrackerSdkPath=$MY_DIR/../MSBuild/TrackerSDK/ -property:LinkTrackerFrameworkPath=$MY_DIR/../MSBuild/TrackerFramework/ -property:Configuration=$1 -property:Platform=$2
else
    echo Usage: PT-MSBuild.sh Configuration\(Debug\|Release\) Platform\(x86\|x64\)
fi