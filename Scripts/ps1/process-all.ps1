<#
.SYNOPSIS
    Randomly generates pairs of normal and malware traffic pcap files, combines them, and computes IPFIX flows.

.DESCRIPTION
    This script takes a set of normal traffic pcap files and a set of malware pcap files, randomly pairs them, and generates a specified number of combined pcap files. Each combined file is an "infected" pcap that includes both normal and malware traffic. The script then processes these infected pcap files to compute IPFIX flows using the Flowmon exporter.

.PARAMETER NormalPcapsFolder
    The path to the folder containing normal traffic pcap files. This parameter is mandatory.

.PARAMETER MalwareFamiliesFolder
    The path to the folder containing different malware family pcap files. This parameter is mandatory.

.PARAMETER OutputCaptureFolderPath
    The path to the folder where the output infected pcap files will be saved. This parameter is mandatory.

.PARAMETER SampleCount
    The number of infected pcap samples to generate. This parameter is optional with a default value of 10.

.EXAMPLE
    .\process-all.ps1 -NormalPcapsFolder "path\to\normal\pcaps" -MalwareFamiliesFolder "path\to\malware\pcaps" -OutputCaptureFolderPath "path\to\output\folder"

    This command will process the pcap files in the specified folders and generate 10 infected pcap samples in the output folder.

.EXAMPLE
    .\process-all.ps1 -NormalPcapsFolder "path\to\normal\pcaps" -MalwareFamiliesFolder "path\to\malware\pcaps" -OutputCaptureFolderPath "path\to\output\folder" -SampleCount 20

    This command will generate 20 infected pcap samples using the pcap files from the specified normal and malware folders and save them in the output folder.

#>

param (
    [Parameter(Mandatory=$true)]
    [string] $NormalPcapsFolder,
    
    [Parameter(Mandatory=$true)]
    [string] $MalwareFamiliesFolder,

    [Parameter(Mandatory=$true)]
    [string] $OutputCaptureFolderPath,

    [Parameter(Mandatory=$false)]
    [int] $SampleCount = 10
)

$normalPcaps = Get-Item -Path $NormalPcapsFolder/*.pcap
$malwareFamilies = Get-Item -Path $MalwareFamiliesFolder/*

$samples = 1..$SampleCount | ForEach-Object {

    $malwareFamily = $malwareFamilies | Get-Random
    $malwareSample = Get-Item -Path $malwareFamily/*.pcap | Get-Random
    $malwareFamilyName = $malwareFamily.Name
    $malwareSampleName = $malwareSample.Name
    $malwareSamplePath = $malwareSample.FullName

    $normalPcap = $normalPcaps | Get-Random
    $normalPcapName = $normalPcap.Name
    $normalPcapPath = $normalPcap.FullName

    $id = $_.ToString("D4")

    $normalPcapNameNoExtension =  Split-Path -Path $normalPcapPath -LeafBase
    $malwareSampleNameNoExtension =  Split-Path -Path $malwareSamplePath -LeafBase
    $outputCaptureName = "$id-$normalPcapNameNoExtension-$malwareFamilyName.pcap"
    $outputCapturePath = Join-Path -Path $OutputCaptureFolderPath -ChildPath $outputCaptureName

    $obj = [PSCustomObject]@{
        MalwareSamplePath = $malwareSamplePath
        NormalPcapPath = $normalPcapPath
        OutputCaptureName = $outputCapturePath
    }
    Write-Output $obj
}

$samples | Foreach-Object -ThrottleLimit 10 -Parallel {
    Write-Host "$_"
    ./bin/insert-infection.ps1 -BackgroudCapturePath $_.NormalPcapPath -MalwareCapturePath $_.MalwareSamplePath -OutputCapturePath $_.OutputCaptureName

    ./bin/export-flows.ps1 flowmon.rysavy.fit.vutbr.cz $_.OutputCaptureName
}


