
param (
    [Parameter(Mandatory=$true, HelpMessage="Host or IP address of Flowmon Appliance.")]
    [string]$FlowmonHost,

    [Parameter(Mandatory=$true, ValueFromPipeline=$true, HelpMessage="Input pcap file to process.")]
    [string]$InputFile,

    [Parameter(Mandatory=$false, HelpMessage="Maximum wait time between actions.")]
    [bool]$DeleteAfterDownload = $true
 )  

begin {
    ssh flowmon@$FlowmonHost "mkdir /home/flowmon/ethanol"
    $securePassword = Read-Host "Enter password for flowmon user" -AsSecureString
    $password = ConvertFrom-SecureString $securePassword
}
process {
    $filePath = $InputFile
    # Check if file exists
    if (Test-Path $filePath) {
        $filePathRoot = Split-Path $filePath -Parent
        $fileNameWithExtension = Split-Path $filePath -Leaf
        $fileNameWithoutExtension = $fileNameWithExtension.Split(".")[0]
        # Process file here
        Write-Progress -Activity "Processing file" -Status "Uploading file: $filePath" 
        scp $filePath flowmon@flowmon.rysavy.fit.vutbr.cz:/home/flowmon/ethanol/$fileNameWithExtension

        Write-Progress -Activity "Processing file" -Status  "Exporting flows from pcap file: $fileNameWithExtension at the remote host...this may take a while."
        ssh flowmon@flowmon.rysavy.fit.vutbr.cz "echo $password | sudo -S flowmonexp5 -I pcap-replay:file=/home/flowmon/ethanol/$fileNameWithExtension,speed=1 -P nbar2 -P tls:fields=MAIN#JA3#CLIENT#CERT -P dns -P http -E json > /home/flowmon/ethanol/$fileNameWithoutExtension.flows.json"

        Write-Progress -Activity "Processing file" -Status "Downloading result file: $fileNameWithoutExtension.flows.json"
        scp flowmon@flowmon.rysavy.fit.vutbr.cz:/home/flowmon/ethanol/$fileNameWithoutExtension.flows.json $filePathRoot

        if ($true -eq $DeleteAfterDownload)
        {
            ssh flowmon@$FlowmonHost "rm /home/flowmon/ethanol/$fileNameWithoutExtension.*"
        }
    }
    else {
        Write-Error "File not found: $filePath"
    }
}






