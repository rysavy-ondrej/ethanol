# Loop through all input files
ssh flowmon@flowmon.rysavy.fit.vutbr.cz "mkdir /home/flowmon/flowexp" > $null
$securePassword = Read-Host "Enter password for flowmon user" -AsSecureString
$password = ConvertFrom-SecureString $securePassword
foreach ($filePath in $args) {
    # Check if file exists
    if (Test-Path $filePath) {
        $filePathRoot = Split-Path $filePath -Parent
        $fileNameWithExtension = Split-Path $filePath -Leaf
        $fileNameWithoutExtension = $fileNameWithExtension.Split(".")[0]
        # Process file here
        Write-Host "Uploading file: $filePath"
        scp $filePath flowmon@flowmon.rysavy.fit.vutbr.cz:/home/flowmon/flowexp/$fileNameWithExtension
        # Do some action with the file, such as reading, writing, or manipulating data
        Write-Host "Exporting flows from pcap file: $fileNameWithExtension at the remote host...this may take a while."
        ssh flowmon@flowmon.rysavy.fit.vutbr.cz "echo $password | sudo -S flowmonexp5 -I pcap-replay:file=/home/flowmon/flowexp/$fileNameWithExtension,speed=1 -P nbar2 -P tls:fields=MAIN#JA3#CLIENT#CERT -P dns -P http -E json > /home/flowmon/flowexp/$fileNameWithoutExtension.flows.json"
        Write-Host "Downloading result file: $fileNameWithoutExtension.flows.json"
        scp flowmon@flowmon.rysavy.fit.vutbr.cz:/home/flowmon/flowexp/$fileNameWithoutExtension.flows.json $filePathRoot
    }
    else {
        Write-Host "File not found: $filePath"
    }
}






