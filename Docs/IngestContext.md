# Ingest facts

The facts are ingested to the context by executing temporal queries on source streams.
Facts are included in the context if it has a defined relation with the flow. 
There are different types of relations as listed in this section. For each relation
the temporal query is continuously executed over the source stream and observed results are then ingested to the flow's context.

## Reverse Flow

```csharp
var reverseFlowStream = source.Multicast(
    src => src.Join(
        src, 
        f => new { SA = f.SrcIp, SP = f.SrcPt, DA = f.DstIp, DP = f.DstPt }, 
        f => new { SA = f.DstIp, SP = f.DstPt, DA = f.SrcIp, DP = f.SrcPt }, 
        (k,v) => new { KeyFlow = k, ReverseFlow = v }));
```

## Domain Names

```csharp
var domainFlowStream = source.Join(
    dnsStream.SessionTimeoutWindow(TimeSpan.FromMinutes(5).Ticks, TimeSpan.FromMinutes(10).Ticks), 
    f => new { SA = f.SrcIp, DA = f.DstIp }, 
    f => new { SA = f.DstIp, DA = f.DnsResponseData }, 
    (k,v) => new { KeyFlow = k, DomainFlow = v });
```

## HTTP Request 

Provides information on HTTP requests from the related plain HTTP flow (if any). This 
may be useful to get more information on HTTPS flows as sometimes there is plain HTTP
related to the session.

```csharp
var webFlowStream = tcpStream.Join(
    httpStream.SessionTimeoutWindow(TimeSpan.FromSeconds(5).Ticks, TimeSpan.FromSeconds(10).Ticks), 
    f => new { SA = f.SrcIp, DA = f.DstIp, DP_HTTP = 80, DP_HTTPS = f.DstPt }, 
    f => new { SA = f.SrcIp, DA = f.DstIp, DP_HTTP = f.DstPt, DP_HTTPS = 443 }, 
    (k,v) => new {KeyFlow= k, WebFlow = v});
var webFlowList = GetList(webFlowStream, e => e.IsData);
display(webFlowList);
```


#