# If executable is not build run the following:

if [ ! -e ../../Publish/Macos/Ethanol.ContextBuilder ]; then
  echo "Executable file does not exist. Compiling project and publishing..."
  dotnet publish -c Release -o ../../Publish/Macos -r osx.10.11-x64 --self-contained true /p:PublishReadyToRun=true /p:PublishSingleFile=true ../../Source/Ethanol.ContextBuilder/
fi

# run the experiments
../../Publish/Macos/Ethanol.ContextBuilder Build-Context -r FlowmonJson:file=webuser.flows.json -c ./config.yaml -w JsonWriter > ./webuser.ctx.json
