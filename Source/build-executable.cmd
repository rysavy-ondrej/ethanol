@ECHO Building executables for win-x64...

cd Ethanol.Cli
dotnet publish -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -c Release
copy .\bin\Release\net7.0\win-x64\publish\ethanol.exe ..\..\Deploy\Bin\

cd ..\Ethanol.StressTest
dotnet publish -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -c Release
copy .\bin\Release\net7.0\win-x64\publish\ethanol-test.exe ..\..\Deploy\Bin\

@ECHO Done!
pause

 