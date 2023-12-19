<#
.SYNOPSIS
This PowerShell script uploads a capture file to Flowmon appliance, exports flows using flowmonexp5, and downloads the results.

.DESCRIPTION
The script takes an input pcap file, uploads it to the Flowmon appliance, exports flows using flowmonexp5, and downloads the resulting flow file. The downloaded flow file can then be processed further or used for analysis. The script also provides an option to delete the downloaded file after it has been processed.

.PARAMETER FlowmonHost
The host or IP address of the Flowmon Appliance.

.PARAMETER InputFile
The input pcap file to process.

.PARAMETER DeleteAfterDownload
An optional boolean parameter that specifies whether to delete the downloaded file after it has been processed. The default value is $true.

.EXAMPLE
PS> .\Export-Flows.ps1 -FlowmonHost "192.168.1.100" -InputFile "capture.pcap" -DeleteAfterDownload $true

This command uploads the "capture.pcap" file to the Flowmon appliance with IP address "192.168.1.100", exports the resulting flows using flowmonexp5, and downloads the resulting flow file. The downloaded file will be deleted after it has been processed.

.NOTES
This script requires flowmonexp5 to be installed on the remote machine.

#>
param (
    [Parameter(Mandatory=$true, HelpMessage="Host or IP address of Flowmon Appliance.")]
    [string]$FlowmonHost,

    [Parameter(Mandatory=$true, ValueFromPipeline=$true, HelpMessage="Input pcap file to process.")]
    [string]$InputFile,

    [Parameter(Mandatory=$false, HelpMessage="An optional boolean parameter that specifies whether to delete the downloaded file after it has been processed.")]
    [bool]$DeleteAfterDownload = $true
 )  

begin {
    
    $rootFolder = "/data/tmp/"
    # get random string:
    $tempFolder = Split-Path -Path $([System.IO.Path]::GetTempFileName()) -LeafBase  
    
    $workingFolder = $rootFolder + $tempFolder

    # create temp folder on remote host:
    ssh flowmon@$FlowmonHost "mkdir $workingFolder"
}
process {
    $filePath = $InputFile
    # Check if file exists
    if (Test-Path $filePath) {
        $filePathRoot = Split-Path $filePath -Parent
        $fileNameWithExtension = Split-Path $filePath -Leaf
        $fileNameWithoutExtension = $fileNameWithExtension.Split(".")[0]

        Write-Host "Uploading file to: $workingFolder/$fileNameWithExtension"
        # Process file here
        Write-Progress -Activity "Processing file" -Status "Uploading file: $filePath" 
        scp $filePath flowmon@flowmon.rysavy.fit.vutbr.cz:$workingFolder/$fileNameWithExtension

        Write-Progress -Activity "Processing file" -Status  "Exporting flows from pcap file: $fileNameWithExtension at the remote host...this may take a while."
        ssh flowmon@flowmon.rysavy.fit.vutbr.cz "echo $password | sudo -S flowmonexp5 -I pcap-replay:file=$workingFolder/$fileNameWithExtension,speed=1 -P nbar2 -P tls:fields=MAIN#JA3#CLIENT#CERT -P dns -P http -E json > $workingFolder/$fileNameWithoutExtension.flows.json"

        Write-Progress -Activity "Processing file" -Status "Downloading result file: $fileNameWithoutExtension.flows.json"
        scp flowmon@flowmon.rysavy.fit.vutbr.cz:$workingFolder/$fileNameWithoutExtension.flows.json $filePathRoot

        Write-Host "Done. File written to $filePathRoot/$fileNameWithoutExtension.flows.json"

        if ($true -eq $DeleteAfterDownload)
        {
            ssh flowmon@$FlowmonHost "rm $workingFolder/*; rmdir $workingFolder"
        }
        Write-Progress -Activity "Processing file" -Status "Done"
    }
    else {
        Write-Error "File not found: $filePath"
    }
}






