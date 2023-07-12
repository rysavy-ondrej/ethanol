# Application Sonar

The application sonar is a tool that identifies known client applications and internet services based on:

* fingerprint of client application communication
* list of indicators of services (IP address, domain names, etc) for known Internet applications as provided by Netify dataset

The application sonar consumes host-based context and outputs the JSON data in the following structure:

```json
{ "StartTime":"2023-03-10T11:30:00","EndTime":"2023-03-10T11:35:00",
  "Payload":{
    "HostAddress":"192.168.111.19",
    "OperatingSystem":"Windows 11",
    "Applications": {
      "edge" : [ "flow1", "flow2", "flow3"],
      "zoom" : [ "flow4", "flow5"]
    },
    "Services": {
      "gdrive" : [ "flow2", "flow3" ]
    },
    "Connections":[
       "flow1": { "Key" : {"SrcIp": "192.168.111.19", "SrcPt": 34562, "DstIp" : "23.54.35.211", "DstPt" : 443}, "Value" : { "OctetsSent" : 24546, "OctetsRecv" : 35465, "PacketsSent" : 435, "PacketsRecv" : 450 } },
       "flow2": { ... },
       "flow3": { ... },
       "flow4": { ... },
       "flow5": { ... }
    ]
  }
}
```
