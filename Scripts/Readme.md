# Scripts Description

This folder contains various scripts for managing data and operations in Ethanol experiments and deployment. The scripts are organized by the scripting language used.

## Bash Scripts

| Script Name       | Description                                                                                                 | Usage                                             |
|-------------------|-------------------------------------------------------------------------------------------------------------|---------------------------------------------------|
| ConvertTo-Csv     | Converts nfdump files to CSV. Reads all NFDUMP source files in the source folder and generates corresponding CSV files in the destination path. Overwrites existing files. | `ConvertTo-Csv [-r <SourceFolder>] [-w <DestinationFolder>]` |

## C# Scripts

| Script Name         | Description                                      | Usage                                                                                                         |
|---------------------|--------------------------------------------------|---------------------------------------------------------------------------------------------------------------|
| Insert-Flowtags     | Inserts flowtags into the PostgreSQL database.   | `dotnet-script insert-flowtags.csx -- -c "Server=localhost;Port=1605;..." -i "E:\Ethanol\webuser2\webuser.tcp.json"` |
| Replace-IPAddress   | Replaces IP address in a pcap file.              | `dotnet-script shift-time.csx -- -r 10.27.0.169:192.168.111.14 -i input.pcap -o output.pcap`                   |
| Shift-Time          | Shifts time in a pcap file.                     | `dotnet-script shift-time.csx -- -s 2024-01-01T00:00:00 -i input.pcap -o output.pcap`                          |

## PowerShell Scripts

| Script Name         | Description                                                                          | Examples                                                                         |
|---------------------|-----------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------|
| Dump-Connections.ps1| Collects and dumps information about current TCP connections to CSV files.                    | `.\dump-connections.ps1 -LocalAddress 192.168.111.11 -Prefix desktop` |
| Export-Flows.ps1    | Uploads a capture file to Flowmon appliance, exports flows, and downloads the results.        | `.\export-flows.ps1 -FlowmonHost "192.168.1.100" -InputFile "capture.pcap" -DeleteAfterDownload $true` |
| Scan-Context.ps1    | Scans context data from an Ethanol Database for malware.                                      | `.\scan-context.ps1 -ApiAddress "ethanol.fit.vutbr.cz:1610" -MalwareProfile "Default.mal" -StartTime (Get-Date).AddDays(-1) -EndTime (Get-Date)` |
