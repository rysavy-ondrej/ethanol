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


## Running the Infrastructure

Execute the following commands on the respective hosts:

```bash
flowmon$ sudo flowmonexp5 probe-ethanol.json | while(true); do nc --send-only FLOW-SONAR-IP 5170; done
```

```bash
flowsonar$ /opt/fluent-bit/bin/fluent-bit -c ~/Applications/Fluent-Bit/etc/fluent-bit-input.conf | ~/Applications/Ethanol/bin/Ethanol.ContextBuilder Build-Context -r FlowmonJson -c ~/Applications/Ethanol/etc/context-builder.conf -w JsonWriter | /opt/fluent-bit/bin/fluent-bit -c ~/Applications/Fluent-Bit/etc/fluent-bit-output.conf
```