#!/bin/sh

if [ -z "$1" ]; then
    TARGET="Release"
else
    TARGET=$1    
fi

dotnet test src/SoundFingerprinting.Solr.Tests/SoundFingerprinting.Solr.Tests.csproj -c $TARGET /p:TargetFrameworks=netcoreapp2.1 -v n

if [ "$?" != "0" ]; then
	echo Tests failed. Check logs for details.
	exit 1
fi

dotnet pack src/SoundFingerprinting.Solr/SoundFingerprinting.Solr.csproj -c $TARGET -o ../../build /p:TargetFrameworks=netstandard2.0