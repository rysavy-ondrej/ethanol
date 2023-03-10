param (
    [Parameter(Mandatory=$true, HelpMessage="IP address or hostname of the remote host to deploy.")]
    [string]$RemoteHost
)

$sourceFiles = @( "collect-tcp.ps1" )
$destination = "\\$RemoteHost\c$\Applications\Ethanol\"

foreach ($source in $sourceFiles) {
    Copy-Item $source $destination -Recurse -Force
}