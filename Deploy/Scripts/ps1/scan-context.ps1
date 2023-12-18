<#
.SYNOPSIS
    This script scans the context data from a Ethanol Database accessed via REST API for malware.

.DESCRIPTION
    The script retrieves context data from a Ethanol Database within a specified time range and saves it in an NDJSON file.
    It then uses the 'ethanol.exe' tool to test the context data for malware using a specified malware profile.

.PARAMETER ApiAddress
    The host or IP address of the Ethanol REST API.

.PARAMETER MalwareProfile
    The name of the malware profile to use for testing.

.PARAMETER StartTime
    The start time of the context data to retrieve.

.PARAMETER EndTime
    The end time of the context data to retrieve.

.EXAMPLE
    .\scan-context.ps1 -ApiAddress "ethanol.fit.vutbr.cz:1610" -MalwareProfile "Default.mal" -StartTime (Get-Date).AddDays(-1) -EndTime (Get-Date)

.NOTES
    - The script requires the 'ethanol.exe' tool to be available in the current directory.
    - The context data is saved in an NDJSON file named 'sandbox.{StartTime}.ctx.ndjson'.
    - The output of the malware testing is saved in an NDJSON file named '{StartTime}.out.ndjson'.
#>

param (
    [Parameter(Mandatory=$true, HelpMessage="Host or IP address of Ethanol API, e.g., 'ethanol.fit.vutbr.cz:1610'.")]
    [string]$ApiAddress,

    [Parameter(Mandatory=$true, HelpMessage="Name of the malware profile to use for testing.")]
    [string]$MalwareProfile,
    
    [Parameter(Mandatory=$true, HelpMessage="Start time of the context data to retrieve.")]
    [DateTime]$StartTime,

    [Parameter(Mandatory=$true, HelpMessage="End time of the context data to retrieve.")]
    [DateTime]$EndTime
)  

$startDateTime = $StartTime.ToString("yyyy-MM-ddTHH:mm:ss")
$endDateTime = $EndTime.ToString("yyyy-MM-ddTHH:mm:ss")

$filePrefix = ${startDateTime}.Replace(":", "").Replace("-", "")

$ndjsonPath = "${filePrefix}.ctx.ndjson"

$response = Invoke-RestMethod -Uri "http://${ApiAddress}/api/v1/host-context/contexts?start=${startDateTime}&end=${endDateTime}" -Method Get

if (Test-Path $ndjsonPath) {
    Clear-Content $ndjsonPath
}
else
{
	New-Item -Path $ndjsonPath -ItemType File
}

foreach ($item in $response) {
    $jsonLine = $item | ConvertTo-Json -Compress
    Add-Content -Path $ndjsonPath -Value $jsonLine
}

ethanol.exe test-malware -p ${MalwareProfile} -i .\${filePrefix}.ctx.ndjson -o .\${filePrefix}.out.ndjson