
<#
.SYNOPSIS

Collects and dumps information about current TCP connections.

.DESCRIPTION

The Dump-TcpConnection script regulalry checks the open connections
and stores this information to dump CSV files. It creates a new 
file on the defined timeout.  

.PARAMETER LocalAddress
Specifies the local IP address for connections to be monitored and collected.

.PARAMETER DumpTimeout
Specifies the number of minutes between creation of dump files.

.PARAMETER FetchInterval
Specifies the interval as number of seconds between querying the system about active TCP connections.

.PARAMETER Prefix
The prefix used to create dump files. 

.INPUTS

None. You cannot pipe objects to the script.

.OUTPUTS

None. The script does not generate any output.

.EXAMPLE

PS> .\Dump-TcpConnections.ps1 -LocalAddress 192.168.111.11 -Prefix desktop

.EXAMPLE

PS> .\Dump-TcpConnections.ps1 -LocalAddress 192.168.111.11 -Prefix desktop -DumpInterval 10

#>
param (
    [Parameter(Mandatory=$true)]
    [string] $LocalAddress,
    [int] $DumpTimeout = 5,
    [int] $FetchInterval = 10,
    [Parameter(Mandatory=$true)]
    [string] $Prefix
)


class Helper {
    static [int] $Interval = 5;
    static [string] $Prefix = "tcpcon";
    static [string] GetCurrent () {
        $current = Get-Date
        $curMinutes = [int][Math]::Floor($current.Minute / [Helper]::Interval) * [Helper]::Interval
        $currentName = [string]::Format("{0}.{1:0000}{2:00}{3:00}{4:00}{5:00}.csv",[Helper]::Prefix, $current.Year, $current.Month, $current.Day, $current.Hour,$curMinutes);
        return $currentName
    }
    static [string]GetFlow([string]$localAddress, [int]$localPort, [string]$remoteAddress, [int] $remotePort)
    {
        $flowKey = [string]::Format("TCP@{0}:{1}-{2}:{3}", $localAddress, $localPort, $remoteAddress, $remotePort);
        return $flowKey
    }
}

[Helper]::Interval = $DumpTimeout 
[Helper]::Prefix = $Prefix
$recordCount = 0;
$connections = @()
$currentOutputFile = $lastOutputFile = [Helper]::GetCurrent()
for(;;)
{  
    $activeConnections = Get-NetTCPConnection -AppliedSetting Internet -LocalAddress $LocalAddress 
    | Select-Object -Property @{name='FlowKey';expression={[Helper]::GetFlow($_.LocalAddress,$_.LocalPort, $_.RemoteAddress, $_.RemotePort)}}, LocalAddress, LocalPort,RemoteAddress, RemotePort, State,@{name='ProcessName';expression={(Get-Process -Id $_.OwningProcess). Path}},CreationTime,@{name='CurrentTime';expression={Get-Date}}         
        
    $connections += $activeConnections
    if (!$currentOutputFile.Equals($lastOutputFile))
    {
        $uniqueConnections = $connections | Sort-Object -Property FlowKey -Unique
        $connectionsCount = $uniqueConnections.Count
        Write-Progress -Activity "Writing dump" -Status "Writing $connectionsCount connections to dump: $currentOutputFile"
        $lastOutputFile = $currentOutputFile
        $uniqueConnections | Export-Csv -Path $currentOutputFile -NoTypeInformation 
        $connections = @()
    }
    $activeConnectionsCount = $activeConnections.Count
    Write-Progress -Activity "Fetching TCP connections" -Status "Active connections count: $activeConnectionsCount, current dump: $currentOutputFile"
    $currentOutputFile = [Helper]::GetCurrent()
    Start-Sleep $FetchInterval
}