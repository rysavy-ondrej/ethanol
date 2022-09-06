# How to create datasets

## Ground Truth

In order to design and test classifiers (either manual) or ML-based we need a rich datasets with ground truth information available.
Fortunately, there are some simple method to get such information automatically.

## Windows PowerShell

Windows operating system since version 10 offers a rich collection of POwerShell modules that can be used to inspect 
various OS internals. Cmdlet `Get-NetTCPConnection` is alternative `netstat` enabling to obtain information about active TCP connections
(see http://woshub.com/get-nettcpconnection-windows-powershell/).


```ps
Get-NetTCPConnection -AppliedSetting Internet -State Established |Select-Object -Property LocalAddress, LocalPort,RemoteAddress, RemotePort, State,@{name='ProcessName';expression={(Get-Process -Id $_.OwningProcess). Path}},OffloadState,CreationTime |ft
```