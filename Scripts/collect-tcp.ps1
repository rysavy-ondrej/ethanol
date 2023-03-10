<#
.SYNOPSIS
Captures information on created TCP connections.
.DESCRIPTION
This script captures information on created TCP connections at a specified interval and duration and exports the data to a CSV file.
.PARAMETER OutPath
The folder path where the CSV file will be saved.
.PARAMETER ProbeInterval
The interval in seconds between each data capture. Default value is 5 seconds.
.PARAMETER Duration
The duration of the data capture in minutes. Default value is 5 minutes.
.PARAMETER OutFormat
The format of the ouput. Possible values are json, ndjson, csv.
.EXAMPLE
.\Capture-TcpConnections.ps1 -OutPath C:\Reports -ProbeInterval 00:00:10 -Duration 00:30:00 -OutFormat ndjson

Captures information on created TCP connections and exports the data to a CSV file every 10 seconds for 30 minutes, saving the file to the C:\Reports folder.

    Author: Ondrej Rysavy
    Date: 2023-03-19
#>

param (
    [Parameter(Mandatory=$true, HelpMessage="Enter the folder path where the CSV file will be saved. If stdout is used it will be outputed to the standard output instead.")]
    [string]$OutPath,

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
    $starttime = Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'
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
        $outfilename = $OutPath + "\tcpcapd.$filetime.json"  
    }
    if ($OutFormat -eq "ndjson")
    {
        $outputData = $connectionData.GetEnumerator() | Select-Object @{n='StartTime';e={$_.Value.FirstSeen}},@{n='EndTime';e={$endtime}},@{n='LocalAddress';e={$_.Value.LocalAddress}},@{n='LocalPort';e={$_.Value.LocalPort}},@{n='RemoteAddress';e={$_.Value.RemoteAddress}},@{n='RemotePort';e={$_.Value.RemotePort}},@{n='ProcessName';e={$_.Value.ProcessName}} | ForEach-Object { $_ | ConvertTo-Json -Depth 1 -Compress }
        $outfilename = $OutPath + "\tcpcapd.$filetime.ndjson"  
    }

    if ($OutFormat -eq "csv") {
        $outputData = $connectionData.GetEnumerator() | Select-Object @{n='StartTime';e={$_.Value.FirstSeen}},@{n='EndTime';e={$endtime}},@{n='LocalAddress';e={$_.Value.LocalAddress}},@{n='LocalPort';e={$_.Value.LocalPort}},@{n='RemoteAddress';e={$_.Value.RemoteAddress}},@{n='RemotePort';e={$_.Value.RemotePort}},@{n='ProcessName';e={$_.Value.ProcessName}} | ConvertTo-Csv -NoTypeInformation 
        $outfilename = $OutPath + "\tcpcapd.$filetime.csv"   

    }

    if ($OutPath -eq "stdout")
    {
        $outputData | Write-Output
    }
    else {
        $outputData | Out-File -FilePath $outfilename      
    }
    Write-Progress -Activity "Processing data" -Status "Exporting flows to $outfilename."
}


