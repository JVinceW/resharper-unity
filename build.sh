#!/usr/bin/env bash
set -e
set -o pipefail

# test comment

pushd rider
# Use --info and/or --stacktrace to get logging
./gradlew buildPlugin $@
