<#
.SYNOPSIS
    This script runs an application for building host-based communication context.

.DESCRIPTION
    The script runs an application that builds a host-based communication context using data from two input JSON files:
    
    - The first input file contains flow data from FlowmonExp5.
    - The second input file contains TCP information from a monitored host.
    
    The script generates an output file containing the communication context data in JSON format.

.PARAMETER FlowInput
    The input JSON file containing flow data from FlowmonExp5.

.PARAMETER TcpInfo
    The input JSON file containing TCP information from a monitored host.

.PARAMETER ContextOutput
    The output file name for the communication context data in JSON format.
    Default value is "json".

.EXAMPLE
    .\Build-CommunicationContext.ps1 -FlowInput "flows.json" -TcpInfo "tcp.json" -ContextOutput "context.json"
    This command runs the application for building a host-based communication context using the flow data in "flows.json"
    and the TCP information in "tcp.json". The resulting context data is saved to a file named "context.json".

.INPUTS
    The script takes two input JSON files: one containing flow data from FlowmonExp5, and one containing TCP information from a monitored host.

.OUTPUTS
    The script generates an output file containing the communication context data in JSON format.

.NOTES
    Author:Ondrej Rysavy
    Date: 2023-03-10

.LINK
    https://github.com/your-repository/your-project
#>

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