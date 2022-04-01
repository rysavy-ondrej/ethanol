

```
var dnsResolved = 1 - MinMaxScale(arg.Context.Domains.Flows.Count(), 0, 3, 0, 1);
var tlsServerName = arg.Context.TlsClientFlows.Flows.Select(fact => MinMaxScale(Statistics.ComputeDnsEntropy(fact.TlsServerCommonName).Max(), 0, 4, 0, 1)).Average();
var serverEntropy = arg.Context.TlsClientFlows.Flows.Select(fact => MinMaxScale(Statistics.ComputeDnsEntropy(fact.TlsServerName).Max(), 0, 4, 0, 1)).Average();
var destPort = arg.Context.TlsClientFlows.Flows.Select(fact => MinMaxScale(fact.Flow.DstPt, 0,ushort.MaxValue, 0,1)).Average();
return dnsResolved * 0.4 + tlsServerName * 0.2 + serverEntropy * 0.2 + destPort * 0.2;
```

```
dns = 1 - MINMAXSCALE(count(context.domains.flows), 0, 3, 0, 1)
sname =  AVERAGE(f : context.tlsClientFlows.Flows =>  MINMAXSCALE(ENTROPY(f.tlsServerCommonName), 0, 4, 0, 1))
server = AVERAGE(f : context.tlsClientFlows.Flows =>  MINMAXSCALE(ENTROPY(f.TlsServerName), 0, 4, 0, 1))
dport =  AVERAGE(f : context.tlsClientFlows.Flows =>  MINMAXSCALE(f.flow.dstPort), 0, 65535, 0, 1))

score = dns * 0.4 + sname * 0.2 + server * 0.2 + dport * 0.2;

```