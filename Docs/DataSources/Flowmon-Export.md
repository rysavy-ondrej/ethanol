# Generate Context from a Source PCAP File

This guide outlines the process of computing context from a source PCAP file using Flowmon and Ethanol. These steps are automated in the [export-flows.ps1](../../Deploy/Scripts/export-flows.ps1) script.

## Prerequisites

Ensure the following requirements are met:

- A running Flowmon appliance, accessible via SSH/SCP.
- The Ethanol tool is deployed and available in the system path.

Set the following variables accordingly:

| Variable               | Description                                         |
|------------------------|-----------------------------------------------------|
| `$filePath`            | Path to the local PCAP file (e.g., `d:\data\webconnection.pcap`). |
| `$filePathRoot`        | Path to the parent folder (e.g., `d:\data\`).       |
| `$fileNameWithExtension` | Name of the file including its extension (e.g., `webconnection.pcap`). |

### Step 1: Upload the PCAP File to Flowmon Appliance

To upload the PCAP file to the Flowmon appliance, use the following SCP command:

```bash
scp $filePath flowmon@flowmon.rysavy.fit.vutbr.cz:/home/flowmon/data/$fileNameWithExtension
```

### Step 2: Extract IPFIX Flows

Execute the command below to convert the source PCAP file into a JSON representation of the flows:

```bash
sudo -S flowmonexp5 -I pcap-replay:file=/home/flowmon/data/$fileNameWithExtension,speed=1 -P nbar2 -P tls:fields=MAIN#JA3#CLIENT#CERT -P dns -P http -E json > /home/flowmon/data/$fileNameWithoutExtension.flows.json
```

This command extracts flows from the PCAP file and saves them in JSON format.

### Step 3: Download the IPFIX Flows

Download the generated IPFIX flows file back to your local system:

```bash
scp flowmon@flowmon.rysavy.fit.vutbr.cz:/home/flowmon/data/$fileNameWithoutExtension.flows.json $filePathRoot
```

### Step 4: Compute the Context

Use Ethanol to compute the context from the IPFIX flows:

```powershell
Get-Content $filePathRoot\$fileNameWithoutExtension.flows.json | ethanol.exe exec-builder > $filePathRoot\$fileNameWithoutExtension.ctx.json
```

This command processes the flow data and outputs the computed context in JSON format.

## References

- [Creating Custom Logs from NetFlow](https://www.flowmon.com/en/blog/creating-custom-logs-from-netflow)