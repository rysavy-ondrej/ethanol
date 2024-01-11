$providerPid = Start-Job -ScriptBlock { & ../../Source/Ethanol.Cli/bin/Debug/net7.0/ethanol service start -c context-provider.config.json > provider.stdout.log 2> provider.stderr.log }

$streamerPid =  Start-Job -ScriptBlock { while ($true){ curl "http://localhost:5190/api/v1/host-context/context-stream" | Out-Null } }

$lastWinPid = Start-Job -ScriptBlock { while ($true){ curl http://localhost:5190/api/v1/host-context/windows/last }}

Write-Host "Press any key to continue..."
[Console]::ReadKey($true)

Stop-Job $streamerPid
Stop-Job $lastWinPid
Stop-Job $providerPid
