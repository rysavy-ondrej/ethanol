param (
    [Parameter(Mandatory=$true, HelpMessage="The input json file with flowmonexp5 flows.")]
    [string]$FlowInput,

    [Parameter(Mandatory=$true, HelpMessage="The input json file with tcp info from monitored host.")]
    [string]$TcpInfo,

    [Parameter(Mandatory=$true, HelpMessage="The output file name of the context file.")]
    [string]$ContextOutput = "json"
)

$configFile = New-TemporaryFile

$configContent = @"
window-size: 00:05:00
window-hop: 00:05:00
host-tag-enricher:
    postgres:
        server: localhost
        port: 5432
        database: ethanol
        user: postgres
        password: ethanol-password
        tableName: smartads
flow-tag-enricher:
    jsonfile:
        filename: $TcpInfo
        collection: flows
"@

$configFileName = $configFile.FullName
Set-Content $configFileName $configContent

& ../Source/Ethanol.ContextBuilder/bin/Debug/net7.0/Ethanol.ContextBuilder.exe Build-Context -r FlowmonJson:"{file=$FlowInput}" -c $configFileName -w JsonWriter:"{file=$ContextOutput}"