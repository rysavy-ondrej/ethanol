# Application Sonar

Application Sonar is a sophisticated tool designed to identify known client applications and internet services. It achieves this by utilizing:

- **Fingerprint of Client Application Communication**: Analyzing the unique patterns and characteristics of client application network traffic.
- **Indicators of Services**: Utilizing a comprehensive list of indicators such as IP addresses and domain names associated with known internet applications, as provided by the Netify dataset.

## Data Processing and Output

Application Sonar processes host-based context data and outputs the results in a structured JSON format.

### Output Structure

The JSON output is structured as follows:

```json
{
  "StartTime": "2023-03-10T11:30:00",
  "EndTime": "2023-03-10T11:35:00",
  "HostAddress": "192.168.111.19",
  "OperatingSystem": "Windows 11",
  "Applications": {
    "edge": ["flow1", "flow2", "flow3"],
    "zoom": ["flow4", "flow5"]
  },
  "Services": {
    "gdrive": ["flow2", "flow3"]
  },
  "Connections": {
    "flow1": {
      "Key": {"SrcIp": "192.168.111.19", "SrcPt": 34562, "DstIp": "23.54.35.211", "DstPt": 443},
      "Value": {"OctetsSent": 24546, "OctetsRecv": 35465, "PacketsSent": 435, "PacketsRecv": 450}
    },
    "flow2": { ... },
    "flow3": { ... },
    "flow4": { ... },
    "flow5": { ... }
  }
}
```

### Key Components

- **StartTime and EndTime**: Specify the time range for the captured data.
- **HostAddress**: The IP address of the host machine.
- **OperatingSystem**: The operating system of the host.
- **Applications**: A dictionary of applications detected, with each application linked to its corresponding network flows.
- **Services**: Identified internet services with associated network flows.
- **Connections**: Detailed information about each network flow, including source and destination IPs, ports, and transmitted data metrics.

This structured JSON output enables users to comprehensively understand the network behavior of client applications and services within the specified time range. For further details on utilizing Application Sonar, refer to additional documentation or user guides.