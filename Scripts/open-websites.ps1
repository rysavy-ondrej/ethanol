<#
.SYNOPSIS
    This script opens random web pages in different web browsers.

.DESCRIPTION
    The script reads a list of URLs from a file and randomly opens each URL in a different web browser.
    The script supports the following parameters:

    -UrlFile <string>
        Name of file with URL list. Each URL is on a separate line.
        Default value is "top-1m.txt".
        
    -MinWait <int>
        Minimum wait time between actions, in seconds.
        Default value is 3.
        
    -MaxWait <int>
        Maximum wait time between actions, in seconds.
        Default value is 10.

.PARAMETER UrlFile
    Name of file with URL list. Each URL is on a separate line.

.PARAMETER MinWait
    Minimum wait time between actions, in seconds.

.PARAMETER MaxWait
    Maximum wait time between actions, in seconds.

.EXAMPLE
    .\Open-WebPages.ps1 -UrlFile "urls.txt" -MinWait 5 -MaxWait 15
    This command reads a list of URLs from the "urls.txt" file and opens each URL in a different web browser.
    The script waits between 5 and 15 seconds before opening each URL.

.INPUTS
    None.

.OUTPUTS
    None.

.NOTES
    Author: Ondrej Rysavy
    Date: 2023-03-19
#>
param (
    [Parameter(Mandatory=$true, HelpMessage="Name of file with URL list. Each URL is on a separate line.")]
    [string]$UrlFile = "top-1m.txt",
    [Parameter(Mandatory=$true, HelpMessage="Minimum wait time between actions.")]
    [int]$MinWait = 3,
    [Parameter(Mandatory=$true, HelpMessage="Maximum wait time between actions.")]
    [int]$MaxWait = 10
 )   

$websites = Get-Content -Path $UrlFile -TotalCount 1000
$chrome = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
$firefox = 'C:\Program Files\Mozilla Firefox\firefox.exe' 
$edge = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
$browsers = @($chrome,$firefox,$edge)
$execs=@('chrome', 'firefox','msedge')

while($true)
{
    $action = @("open","open","open", "close") | Get-Random

    if ($action -eq "open")
    {

        $website = $websites | Get-Random
        $browser = $browsers | Get-Random
        $delay = $delays | Get-Random
        Start-Process $browser $website
        Write-Progress -Activity "Open website" -Status "URL=$website in $browser" 
    }
    else
    {
        $process = $execs | Get-Random
        Write-Progress -Activity "Stop process" -Status "$($process)"
        Stop-Process -Name $process -ErrorAction 'SilentlyContinue'
    } 
     
    $delay = Get-Random -Minimum $MinWait -Maximum $MaxWait
    for ($i = 0; $i -lt $delay; $i++) {
        Start-Sleep -Seconds 1
        Write-Progress -Activity "Idle" -Status "Waiting to the next run" -SecondsRemaining ($delay-$i) -PercentComplete ($i * 100/$delay)
    }
} 
