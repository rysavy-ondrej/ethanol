foreach ($filePath in $args) 
{
    $filePathRootWithoutExt=$filePath.Split(".")[0]
    $InputFile=$filePath
    $OutputFile=$filePathRootWithoutExt + ".ctx.json"
    ../Source/Ethanol.ContextBuilder/bin/Debug/net7.0/Ethanol.ContextBuilder.exe Build-Context -r "FlowmonJson:{file=$InputFile}" -c config.yaml -w "JsonWriter:{file=$OutputFile}"
}