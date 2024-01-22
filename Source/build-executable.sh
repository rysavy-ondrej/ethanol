#!/bin/bash
mkdir ../../Deploy/Bin/
cd Ethanol.Cli
dotnet publish -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -c Release
cp ./bin/Release/net7.0/linux-x64/publish/ethanol ../../Deploy/Bin/