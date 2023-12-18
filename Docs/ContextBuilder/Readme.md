# Context Builder v1.0 Documentation

Context Builder is an experimental tool that computes a rich information context for hosts observed in the input NetFLow data.
It can be used as a standalone tool or integrated in the monitoring infrastructure via [Fluent Bit](https://docs.fluentbit.io/manual/).

## Output

The final output of the context builder is NDJSON (in the following example formatted for clarity):

```json
{   
    "id":1131,
    "key":"192.168.111.32",
    "start":"2023-12-16T00:00:00",
    "end":"2023-12-16T00:05:00",
    "connections":[{"remoteHostAddress":"192.168.66.2","remoteHostName":null,"remotePort":7680,"applicationProcessName":null,"internetServices":null,"flows":8,"packetsSent":40,"octetsSent":2296,"packetsRecv":24,"octetsRecv":1056}],
    "resolvedDomains":[],
    "webUrls":[],
    "tlsHandshakes":[],
    "tags":{"ip_dependency_client":{},"ip_dependency_server":{},"open_ports":[],"tags_by_services":[],"hostml_label":[],"in_flow_tags":[],"tls_os_version":[],"activity_all":{"flows":0,"bytes":0},"activity_global":{"flows":0,"bytes":0}}
}
```
