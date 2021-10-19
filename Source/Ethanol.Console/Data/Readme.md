# Data source acquisition

Data acquisition is done by using `nfdump` tool to export CSV files from binary netflow records. 

FLOW
```
/usr/local/bin/nfdump -M /data/nfsen/profiles-data/'live'/'127-0-0-1_p3000' -R $INPUTFILE -o csv 'ipv4' > flow.csv
```

DNS
```
/usr/local/bin/nfdump -M /data/nfsen/profiles-data/'live'/'127-0-0-1_p3000' -R $INPUTFILE -o csv 'proto udp and port 53' > dns.csv
```

HTTP
```
/usr/local/bin/nfdump -M /data/nfsen/profiles-data/'live'/'127-0-0-1_p3000' -R $INPUTFILE -o csv 'proto tcp and port 80' > http.csv
```

TLS
```
/usr/local/bin/nfdump -M /data/nfsen/profiles-data/'live'/'127-0-0-1_p3000' -R $INPUTFILE -o csv 'not tls-cver "N/A"' > tls.csv
```

Example of input:

```
INPUTFILE='2021/09/07/nfcapd.202109070000:2021/09/07/nfcapd.202109071155'
```

