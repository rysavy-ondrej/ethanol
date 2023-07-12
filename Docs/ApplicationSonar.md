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
    "Applications":[

    ],
    "Services":[

    ],
    "Connections":[

    ]
  }
}
```
