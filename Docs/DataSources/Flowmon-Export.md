# Export flow data from Flowmon


## nfdump

## flowmonexp5

To use extract JSON representation of flows in the source pcap file `INPUT` to produce `OUTPUT` file execute the following command:

```bash
sudo flowmonexp5 -I pcap-replay:file=INPUT.pcap,speed=1 -P nbar2 -P tls:fields=MAIN#JA3#CLIENT#CERT -P dns -P http -E json > OUTPUT.json
```

## References

* https://www.flowmon.com/en/blog/creating-custom-logs-from-netflow
