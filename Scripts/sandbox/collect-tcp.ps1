<#
.SYNOPSIS
Captures information on created TCP connections.
.DESCRIPTION
This script captures information on created TCP connections at a specified interval and duration and exports the data to a JSON/CSV file.
.PARAMETER OutPath
The folder path where the output file will be saved.
.PARAMETER SendTo
The connection string to be used for sending data to the TCP server.
.PARAMETER ProbeInterval
The interval in seconds between each data capture. Default value is 5 seconds.
.PARAMETER Duration
The duration of the data capture in minutes. Default value is 5 minutes.
.PARAMETER OutFormat
The format of the ouput. Possible values are json, ndjson, csv.

.EXAMPLE
.\Capture-TcpConnections.ps1 -OutPath C:\Reports -ProbeInterval 00:00:10 -Duration 00:30:00 -OutFormat ndjson

Captures information on created TCP connections and exports the data to a NDJSON file every 10 seconds for 30 minutes, saving the file to the C:\Reports folder.

.EXAMPLE
.\Capture-TcpConnections.ps1 -SendTo 192.168.111.21:5701 -ProbeInterval 00:00:10 -Duration 00:30:00 -OutFormat ndjson

Captures information on created TCP connections and exports the data in NJSON format to a TCP server running at 192.168.111.21:5701 every 30 minutes.

    Author: Ondrej Rysavy
    Date: 2023-03-19
#>

param (
    [Parameter(Mandatory=$false, HelpMessage="The folder path where the JSON/CSV file will be saved.")]
    [string]$OutPath,
    
    [Parameter(Mandatory=$false, HelpMessage="Specifies host name or address and port for TCP connection that will be used to send the data to.")]
    [string]$SendTo,

    [Parameter(Mandatory=$false, HelpMessage="Enter the interval in seconds between each data capture. Default value is 5 seconds.")]
    [timespan]$ProbeInterval = (New-TimeSpan -Seconds 5),

    [Parameter(Mandatory=$false, HelpMessage="Enter the duration of the data captures. Default value is 5 minutes.")]
    [timespan]$Duration = (New-TimeSpan -Minutes 5),

    [Parameter(Mandatory=$true, HelpMessage="The format of the ouput. Possible values are json, ndjson, csv.")]
    [ValidateSet("json", "ndjson", "csv")]
    [string]$OutFormat = "json"
)


$iterations = ($Duration.TotalSeconds / $ProbeInterval.TotalSeconds)
while($true)
{
    #$starttime = Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'
    $filetime = Get-Date -Format 'yyyyMMddHHmmss'
    # Initialize an empty array to store the connection information
    $connectionData = @{}

    # Collect the connection data for the specified duration
    for ($i = 0; $i -lt $iterations; $i++) {
        $connections = Get-NetTCPConnection -State Established | Select-Object LocalAddress,LocalPort,RemoteAddress,RemotePort,State,@{n='ProcessName';e={Get-Process -Id $_.OwningProcess | Select-Object -ExpandProperty ProcessName}}
        $currenttime = Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'
        foreach ($connection in $connections) {
            $connectionKey = "$($connection.LocalAddress)_$($connection.LocalPort)_$($connection.RemoteAddress)_$($connection.RemotePort)"
            if (($connection.RemoteAddress -ne '127.0.0.1') -and (-not $connectionData.ContainsKey($connectionKey)))
            {
                $connectionData[$connectionKey] = @{
                    'LocalAddress' = $connection.LocalAddress
                    'LocalPort' = $connection.LocalPort
                    'RemoteAddress' = $connection.RemoteAddress
                    'RemotePort' = $connection.RemotePort
                    'ProcessName' = $connection.ProcessName
                    'FirstSeen' = $currenttime
                }
            }
        }
        Start-Sleep -Seconds $ProbeInterval.TotalSeconds
        $flowCount = $connectionData.Count
        $pctComplete = $i * 100 / $iterations
        Write-Progress -Activity "Processing data" -Status "Flow count=$flowCount" -PercentComplete $pctComplete 

    }
    $endtime = Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'

    if ($OutFormat -eq "json")
    {
        $outputData = $connectionData.GetEnumerator() | Select-Object @{n='StartTime';e={$_.Value.FirstSeen}},@{n='EndTime';e={$endtime}},@{n='LocalAddress';e={$_.Value.LocalAddress}},@{n='LocalPort';e={$_.Value.LocalPort}},@{n='RemoteAddress';e={$_.Value.RemoteAddress}},@{n='RemotePort';e={$_.Value.RemotePort}},@{n='ProcessName';e={$_.Value.ProcessName}} | ConvertTo-Json 
        $outfilename = "tcpcapd.$filetime.json"  
    }
    if ($OutFormat -eq "ndjson")
    {
        $outputData = $connectionData.GetEnumerator() | Select-Object @{n='StartTime';e={$_.Value.FirstSeen}},@{n='EndTime';e={$endtime}},@{n='LocalAddress';e={$_.Value.LocalAddress}},@{n='LocalPort';e={$_.Value.LocalPort}},@{n='RemoteAddress';e={$_.Value.RemoteAddress}},@{n='RemotePort';e={$_.Value.RemotePort}},@{n='ProcessName';e={$_.Value.ProcessName}} | ForEach-Object { $_ | ConvertTo-Json -Depth 1 -Compress }
        $outfilename = "tcpcapd.$filetime.ndjson"  
    }

    if ($OutFormat -eq "csv") {
        $outputData = $connectionData.GetEnumerator() | Select-Object @{n='StartTime';e={$_.Value.FirstSeen}},@{n='EndTime';e={$endtime}},@{n='LocalAddress';e={$_.Value.LocalAddress}},@{n='LocalPort';e={$_.Value.LocalPort}},@{n='RemoteAddress';e={$_.Value.RemoteAddress}},@{n='RemotePort';e={$_.Value.RemotePort}},@{n='ProcessName';e={$_.Value.ProcessName}} | ConvertTo-Csv -NoTypeInformation 
        $outfilename = "tcpcapd.$filetime.csv"   

    }

    <# SEND OUTPUT... #>
    if ($null -ne $SendTo)
    {
        $srv, $port = $SendTo.Split(":")          
        $data = [string]$outputData
        Write-Progress -Activity "Processing data" -Status "Exporting flows to TCP server=$srv $port."
        $client = New-Object System.Net.Sockets.TcpClient($srv, $port)
        $stream = $client.GetStream()
        $data = [System.Text.Encoding]::ASCII.GetBytes($data)
        $stream.Write($data, 0, $data.Length)
        $client.Close()
    } 
    elseif($null -ne $OutPath)
    { <# send object to the stdout...#>
        $filepath = Join-Path $OutPath $outfilename
        Write-Progress -Activity "Processing data" -Status "Exporting flows to $filepath."
        $outputData | Out-File -FilePath $filepath  
    }
    else <# send object to the stdout...#> 
    {
        $outputData | Write-Output    
    }
}


