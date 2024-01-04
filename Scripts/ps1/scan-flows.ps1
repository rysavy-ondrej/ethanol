<#
.SYNOPSIS
    This script malware scans the flow data from flow files.

.DESCRIPTION
    The script first computes the context data from a given flow file and 
    then performs malware scan to test the context data for malware using a specified malware profile.
    The report is then written in the specified format to the output folder.

.PARAMETER InputFolder
    Specifies the path to the folder containing the input flow files to be scanned.

.PARAMETER MalwareProfile
    Specifies the name of the malware profile to be used for testing the flow data.

.PARAMETER Threshold
    Specifies the threshold value for the malware scan. It should be a floating-point number.

.PARAMETER FlowFormat
    Specifies the format of the input flow files. It can be either "ipfixcol-json" or "flowmon-json".

.PARAMETER OutputFolder
    Specifies the path to the folder where the output reports will be stored. If the folder does not exist, it will be created.

.PARAMETER OutputFormat
    Specifies the format of the output reports. It can be either "json", "csv", or "markdown".

.EXAMPLE
    .\scan-flows.ps1 -InputFolder "C:\FlowData" -MalwareProfile "Profile1.mal" -Treshold 0.8 -FlowFormat "ipfixcol-json" -OutputFolder "C:\Reports" -OutputFormat "json"
    This example scans the flow data from the "C:\FlowData" folder using the "Profile1" malware profile.
    The threshold value is set to 0.8, and the input flow files are in the "ipfixcol-json" format.
    The resulting reports will be stored in the "C:\Reports" folder in JSON format.

.NOTES
    - Make sure to provide the correct paths for the input folder and output folder.
    - The malware profile should exist and be compatible with the flow data format.
    - Adjust the threshold value according to your needs.
    - Ensure that the required dependencies, such as the "ethanol.exe" tool, are available in the system.
#>

param (
    [Parameter(Mandatory=$true, HelpMessage="The name of the source file to use for testing.")]
    [string]$InputFolder,

    [Parameter(Mandatory=$true, HelpMessage="Name of the malware profile to use for testing.")]
    [string]$MalwareProfile,

    [Parameter(Mandatory=$true, HelpMessage="Name of the malware profile to use for testing.")]
    [float]$Threshold,
    
    [Parameter(Mandatory=$true, HelpMessage="The input flow format. It can be either ipfixcol-json or flowmon-json.")]
    [string]$FlowFormat,

    [Parameter(Mandatory=$true, HelpMessage="The output folder to store the reports. If the folder does not exist, it will be created.")]
    [string]$OutputFolder,

    [Parameter(Mandatory=$true, HelpMessage="The output report format. It can be either json, csv, or markdown.")]
    [string]$OutputFormat
)  

# get all input files in InputFolder:
$files = Get-ChildItem -Path $InputFolder -Filter *.json

# output foilder not exist create it:
if (!(Test-Path $OutputFolder)) {
    New-Item -ItemType Directory -Force -Path $OutputFolder
}

# for each file:
foreach ($file in $files) {

    $outfile = "$OutputFolder\$($file.Name).$($OutputFormat)"

    Get-Content $file | ethanol.exe builder exec -f $FlowFormat | ethanol.exe malware pscan -p $MalwareProfile -t $Threshold -f $OutputFormat > $outfile
}
