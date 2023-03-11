# Ethanol Experimenal Infrastructure

The experimental infrastructure consists of:

* Monitored hosts ([sandbox](sandbox/Readme.md)) are connected to monitored network whose communication is forwarded to the monitoring port of Flowmon Appliance.
* Flowmon Appliance ([flowmon](flowmon/Readme.md)) runs flowmonexp5 to export flows for traffic observed on the monitoring port.
* Application Host ([flowsonar](flowsonar/Readme.md)) runs Ethanol application and other required tools to produce data required for analysis and learning. 

## Deployment

To deploy the tools from the source host the following scripts depending on the target host are used:

### Linux Target

To copy files to a Linux server using PowerShell, you can use the scp (secure copy) command. This requires that you have SSH access to the Linux server and that the scp command is installed on the server.

```pwsh
$sourceFilePath = "C:\path\to\file.txt"
$destinationServer = "user@192.168.1.100"
$destinationFilePath = "/path/on/server/file.txt"

scp $sourceFilePath $destinationServer:$destinationFilePath
```

### Windows Target
To copy files to a Windows server using PowerShell, you can use the Copy-Item cmdlet. 

```pwsh
Copy-Item -Path "C:\local\file.txt" -Destination "\\server\share\file.txt" -Credential (Get-Credential)
```