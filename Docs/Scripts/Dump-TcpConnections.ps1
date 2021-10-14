
<#
    Collects and dumps information about current TCP connections.

    It runs continuosly until CTRL+C is pressed.
#>
param (
    # Local IP address. This is used to filter flows that appears on the given local interface.
    [Parameter(Mandatory=$true)]
    [string] $LocalAddress,
    # The interval to dump the collection fo TCP connections to the output CSV file. Time in minutes.
    [int] $DumpTimeout = 5,
    # The interval to fetch current TCP connections from the OS. Time in seconds.
    [int] $FetchInterval = 10,
    
    # Name of the CSV file prefix. The date time information is appended to this prefix.
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