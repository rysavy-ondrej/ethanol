<#
.SYNOPSIS
    Merges a normal pcap file with a malware pcap file, adjusting host addresses and timestamps.

.DESCRIPTION
    `insert-infection.ps1` is designed to combine a normal pcap file with a malware pcap file into a single pcap file. It adjusts the host addresses and timestamps in the malware pcap file to create a seamless integration with the normal traffic capture. This script is suitable for creating combined pcap datasets for network analysis, security training, and testing.

.PARAMETER BackgroudCapturePath
    Specifies the path to the pcap file containing normal background network traffic. This parameter is mandatory.

.PARAMETER MalwareCapturePath
    Specifies the path to the pcap file containing malware traffic. This parameter is mandatory.

.PARAMETER OutputCapturePath
    Specifies the path where the output pcap file, containing the merged traffic, will be saved. This parameter is mandatory.

.PARAMETER InsertPoint
    Specifies the insertion point of the malware traffic within the normal traffic as a decimal fraction. For example, a value of 0.5 indicates that the malware traffic will be inserted halfway through the normal traffic timeline. This parameter is optional with a default value of 0.5.

.EXAMPLE
    .\insert-infection.ps1 -BackgroudCapturePath "path\to\normal_traffic.pcap" -MalwareCapturePath "path\to\malware_traffic.pcap" -OutputCapturePath "path\to\output.pcap"

    This command merges 'normal_traffic.pcap' with 'malware_traffic.pcap' and outputs the merged file to 'output.pcap'.

.EXAMPLE
    .\insert-infection.ps1 -BackgroudCapturePath "path\to\normal_traffic.pcap" -MalwareCapturePath "path\to\malware_traffic.pcap" -OutputCapturePath "path\to\output.pcap" -InsertPoint 0.3

    This command merges the pcap files with the malware traffic inserted at 30% into the timeline of the normal traffic.

#>

param (
    [Parameter(Mandatory=$true)]
    [string] $BackgroudCapturePath,
    [Parameter(Mandatory=$true)]
    [string] $MalwareCapturePath,
    [Parameter(Mandatory=$true)]
    [string] $OutputCapturePath,
    [Parameter(Mandatory=$false)]
    [double] $InsertPoint = 0.5
    
)

# Define the malware host prefix used by Tria.ge. 
# This is used to identify the malware host in the background capture.
$malwareHostPrefix = '^10.127.\d{1,3}.\d{2,3}$'

Write-Progress -Activity "Processing input" -Status "Getting information on Malware Capture file: $MalwareCapturePath"

# read capinfo and retrieve information on the first and last packet:
$packetInfo = capinfos.exe -a -e -c -s $MalwareCapturePath
$firstPacket = ($packetInfo | Select-String -Pattern "First packet time") -split ":   " | Select-Object -Last 1 
$lastPacket = ($packetInfo | Select-String -Pattern "Last packet time") -split ":    " | Select-Object -Last 1

$malwareCaptureDuration =  (Get-Date -Date $lastPacket) - (Get-Date -Date $firstPacket)

Write-Host "Malware Capture: First packet=$firstPacket"
Write-Host "Malware Capture: Last packet=$lastPacket"

$ipHosts = & tshark -r  $MalwareCapturePath -T fields -e ip.src -Y 'tcp.flags.syn == 1 and tcp.flags.ack == 0' | Where-Object { $_ -match $malwareHostPrefix } | Sort-Object | Get-Unique
$infectedHostIp = $ipHosts | Select-Object -First 1

Write-Host "Malware Capture: Found $($ipHosts.Count) unique IP addresses in the Malware Capture file, infected host=$infectedHostIp."

## now analyze the background capture file:
Write-Progress -Activity "Processing input" -Status "Analyzing Background Capture file: $BackgroudCapturePath"

# read capinfo and retrieve information on the first and last packet:
$backgroundPacketInfo = capinfos.exe -a -e -c -s $BackgroudCapturePath
$backgroundFirstPacket = ($backgroundPacketInfo | Select-String -Pattern "First packet time") -split ":   " | Select-Object -Last 1
$backgroundLastPacket = ($backgroundPacketInfo | Select-String -Pattern "Last packet time") -split ":    " | Select-Object -Last 1

Write-Host "Background Capture: First packet=$backgroundFirstPacket"
Write-Host "Background Capture: Last packet=$backgroundLastPacket"

$insertTime = (Get-Date -Date $backgroundFirstPacket) + (((Get-Date -Date $backgroundLastPacket) - (Get-Date -Date $backgroundFirstPacket)) * $InsertPoint)

Write-Host "Background Capture: Calculated insert time=$insertTime  (insert point=$InsertPoint)"

# Extract IP addresses using tshark and store them in a variable
$backgroundIps = & tshark -r $BackgroudCapturePath -T fields -e ip.src -Y 'tcp.flags.syn == 1 and tcp.flags.ack == 0' | Where-Object { $_ -match '^\d{1,3}(\.\d{1,3}){3}$' } 
           
# Count occurrences of each IP address
$backgroundIpCount = $backgroundIps | Group-Object | Select-Object Name,Count  | Sort-Object Count -Descending

# Get the IP address with the most occurrences
$mostFrequentIp = $backgroundIpCount | Select-Object -First 1
$principalHostIp = $mostFrequentIp.Name

Write-Host "Background Capture: Principal host=$principalHostIp"

## now we have everythin we need to infect the background capture file:
# 1. Adjust the time in the malware file
Write-Progress -Activity "Generating output" -Status "Adjusting malware capture file time to match background capture file time"

$tempFile1 =  [System.IO.Path]::GetTempFileName()
$tempFile2 =  [System.IO.Path]::GetTempFileName()

$formattedDateTime = $insertTime.ToString("yyyy-MM-ddTHH:mm:ss")

dotnet-script $PSScriptRoot/shift-time.csx -- -s $formattedDateTime -i $MalwareCapturePath -o $tempFile1

# 2. Replace infected IP with principal host IP
Write-Progress -Activity "Generating output" -Status  "Adjusting malware capture file IP address to match background capture file IP address"

dotnet-script $PSScriptRoot/replace-address.csx -- -r ${infectedHostIp}:${principalHostIp} -i $tempFile1 -o $tempFile2

# 3. Merge pcap files to produce the infected output file
Write-Progress -Activity "Generating output" -Status "Merging background capture file and malware capture file"

mergecap -F pcap -w $OutputCapturePath $BackgroudCapturePath $tempFile2
if ($?) {
    Write-Host "Infected Capture written to $OutputCapturePath."
} else {
    $exitCode = $LASTEXITCODE
    Write-Host "Unable to merge pcaps, error: $exitCode"
}
# 4. Cleanup
Remove-Item -Path $tempFile1
Remove-Item -Path $tempFile2

@"
Infected Capture: $(Split-Path -Path $OutputCapturePath -Leaf) 
Background Capture: $(Split-Path -Path $BackgroudCapturePath  -Leaf) 
Malware Capture: $(Split-Path -Path $MalwareCapturePath  -Leaf)
Malware Host: $infectedHostIp
Infected Host: $principalHostIp
Capture Start: $backgroundFirstPacket
Capture End: $backgroundLastPacket
Infection Time: $formattedDateTime
Infection Duration: $malwareCaptureDuration
"@  | Out-File -FilePath "${OutputCapturePath}.info"

Write-Progress -Activity "Generating output" -Status "Done"

