# Ipfixcol2 JSON Flow Source

```bash
ipfixcol2 -c startup.xml
```

The content of startup.xml file is as follows:

```xml
<ipfixcol2>
<inputPlugins>
<input>
    <name>UDP input</name>
    <plugin>udp</plugin>
    <params>
        <localPort>4739</localPort>
        <localIPAddress></localIPAddress>
        <!-- Optional parameters -->
        <connectionTimeout>600</connectionTimeout>
        <templateLifeTime>1800</templateLifeTime>
        <optionsTemplateLifeTime>1800</optionsTemplateLifeTime>
    </params>
</input>
</inputPlugins>
<outputPlugins>
<output>
    <name>JSON output</name>
    <plugin>json</plugin>
    <params>
        <tcpFlags>formatted</tcpFlags>
        <timestamp>formatted</timestamp>
        <protocol>formatted</protocol>
        <ignoreUnknown>true</ignoreUnknown>
        <ignoreOptions>true</ignoreOptions>
        <nonPrintableChar>true</nonPrintableChar>
        <octetArrayAsUint>true</octetArrayAsUint>
        <numericNames>false</numericNames>
        <splitBiflow>false</splitBiflow>
        <detailedInfo>false</detailedInfo>
        <templateInfo>false</templateInfo>
        <outputs>
            <!-- Choose one or more of the following outputs -->
                <!--
            <send>
                <name>Send to my server</name>
                <ip>127.0.0.1</ip>
                <port>8000</port>
                <protocol>tcp</protocol>
                <blocking>no</blocking>
            </send>
                -->
            <print>
                <name>Printer to standard output</name>
            </print>

            <file>
                <name>Store to files</name>
                <path>./flow/%Y/%m/%d/</path>
                <prefix>json.</prefix>
                <timeWindow>300</timeWindow>
                <timeAlignment>yes</timeAlignment>
                <compression>none</compression>
            </file>
        </outputs>
    </params>
</output>
</outputPlugins>
</ipfixcol2>
```

## Examples of various JSON records:

### Generic Flow

```json
{
   "@type":"ipfix.entry",
   "iana:octetDeltaCount":156,
   "iana:packetDeltaCount":3,
   "iana:protocolIdentifier":"TCP",
   "iana:ipClassOfService":0,
   "iana:tcpControlBits":"....S.",
   "iana:sourceTransportPort":60392,
   "iana:sourceIPv4Address":"192.168.66.2",
   "iana:ingressInterface":8,
   "iana:destinationTransportPort":80,
   "iana:destinationIPv4Address":"107.150.18.214",
   "iana:egressInterface":0,
   "iana:bgpSourceAsNumber":0,
   "iana:bgpDestinationAsNumber":8100,
   "iana:samplingInterval":0,
   "iana:samplingAlgorithm":0,
   "iana:sourceMacAddress":"00:0C:29:D2:9E:CE",
   "iana:postDestinationMacAddress":"48:8F:5A:19:A4:CD",
   "iana:ipVersion":4,
   "iana:flowStartMilliseconds":"2023-12-19T15:18:14.033Z",
   "iana:flowEndMilliseconds":"2023-12-19T15:18:17.035Z"
}
```

### DNS

```json
{
   "@type":"ipfix.entry",
   "iana:octetDeltaCount":124,
   "iana:packetDeltaCount":1,
   "iana:protocolIdentifier":"UDP",
   "iana:ipClassOfService":0,
   "iana:sourceTransportPort":53,
   "iana:sourceIPv4Address":"8.8.8.8",
   "iana:ingressInterface":8,
   "iana:destinationTransportPort":58509,
   "iana:destinationIPv4Address":"192.168.66.2",
   "iana:egressInterface":0,
   "iana:bgpSourceAsNumber":15169,
   "iana:bgpDestinationAsNumber":0,
   "iana:samplingInterval":0,
   "iana:samplingAlgorithm":0,
   "iana:sourceMacAddress":"48:8F:5A:19:A4:CD",
   "iana:postDestinationMacAddress":"00:0C:29:D2:9E:CE",
   "iana:ipVersion":4,
   "iana:flowStartMilliseconds":"2023-12-19T15:18:18.016Z",
   "iana:flowEndMilliseconds":"2023-12-19T15:18:18.016Z",
   "iana:applicationId":50331701,
   "flowmon:dnsId":46618,
   "flowmon:dnsFlagsCodes":33155,
   "flowmon:dnsQuestionCount":1,
   "flowmon:dnsAnswrecCount":0,
   "flowmon:dnsAuthrecCount":1,
   "flowmon:dnsAddtrecCount":0,
   "flowmon:dnsCrrName":"\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:dnsCrrType":65535,
   "flowmon:dnsCrrClass":65535,
   "flowmon:dnsCrrTtl":4294967295,
   "flowmon:dnsCrrRdata":"0x00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
   "flowmon:dnsCrrRdataLen":0,
   "flowmon:dnsQname":"wpad.redmoon.cz\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:dnsQtype":1,
   "flowmon:dnsQclass":1
}
```

```json
{
   "@type":"ipfix.entry",
   "iana:octetDeltaCount":289,
   "iana:packetDeltaCount":1,
   "iana:protocolIdentifier":"UDP",
   "iana:ipClassOfService":0,
   "iana:sourceTransportPort":53,
   "iana:sourceIPv4Address":"8.8.8.8",
   "iana:ingressInterface":8,
   "iana:destinationTransportPort":59549,
   "iana:destinationIPv4Address":"192.168.66.2",
   "iana:egressInterface":0,
   "iana:bgpSourceAsNumber":15169,
   "iana:bgpDestinationAsNumber":0,
   "iana:samplingInterval":0,
   "iana:samplingAlgorithm":0,
   "iana:sourceMacAddress":"48:8F:5A:19:A4:CD",
   "iana:postDestinationMacAddress":"00:0C:29:D2:9E:CE",
   "iana:ipVersion":4,
   "iana:flowStartMilliseconds":"2023-12-19T15:28:43.500Z",
   "iana:flowEndMilliseconds":"2023-12-19T15:28:43.500Z",
   "iana:applicationId":50331701,
   "flowmon:dnsId":20921,
   "flowmon:dnsFlagsCodes":33152,
   "flowmon:dnsQuestionCount":1,
   "flowmon:dnsAnswrecCount":4,
   "flowmon:dnsAuthrecCount":0,
   "flowmon:dnsAddtrecCount":0,
   "flowmon:dnsCrrName":"eus2c-displaycatalog.frontdoor.bigcatalog.commerce.microsoft.com",
   "flowmon:dnsCrrType":1,
   "flowmon:dnsCrrClass":1,
   "flowmon:dnsCrrTtl":19,
   "flowmon:dnsCrrRdata":"0x1452E409000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
   "flowmon:dnsCrrRdataLen":4,
   "flowmon:dnsQname":"displaycatalog.mp.microsoft.com\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:dnsQtype":1,
   "flowmon:dnsQclass":1
}
```

### TLS

```json
{
   "@type":"ipfix.entry",
   "iana:octetDeltaCount":3478,
   "iana:packetDeltaCount":22,
   "iana:protocolIdentifier":"TCP",
   "iana:ipClassOfService":0,
   "iana:tcpControlBits":".APRS.",
   "iana:sourceTransportPort":3389,
   "iana:sourceIPv4Address":"192.168.66.2",
   "iana:ingressInterface":8,
   "iana:destinationTransportPort":49332,
   "iana:destinationIPv4Address":"192.168.8.18",
   "iana:egressInterface":0,
   "iana:bgpSourceAsNumber":0,
   "iana:bgpDestinationAsNumber":0,
   "iana:samplingInterval":0,
   "iana:samplingAlgorithm":0,
   "iana:sourceMacAddress":"00:0C:29:D2:9E:CE",
   "iana:postDestinationMacAddress":"48:8F:5A:19:A4:CD",
   "iana:ipVersion":4,
   "iana:flowStartMilliseconds":"2023-12-19T15:18:01.427Z",
   "iana:flowEndMilliseconds":"2023-12-19T15:18:03.702Z",
   "iana:applicationId":218104261,
   "flowmon:httpHost":"192.168.66.2\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:httpMethodMask":512,
   "flowmon:tlsContentType":13,
   "flowmon:tlsHandshakeType":10502,
   "flowmon:tlsSetupTime":36871,
   "flowmon:tlsServerVersion":771,
   "flowmon:tlsServerRandom":"0x6581B429E33FEF5BBD4B7A241715817059FC8548BC037619B77CBBEF95335ECD",
   "flowmon:tlsServerSessionId":"0xDE050000C55FFE420E5999ED1DA6C4F487AAF5969EFCDD32D16B1C99E0E56526",
   "flowmon:tlsCipherSuite":157,
   "flowmon:tlsAlpn":"\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:tlsSni":"192.168.66.2\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:tlsSniLength":12,
   "flowmon:tlsClientVersion":772,
   "flowmon:tlsCipherSuites":"0x02130313011308C012C0160013000A00",
   "flowmon:tlsClientRandom":"0xDECBEC967953C6B013546763D99DBB35534E923A6DB34BDECE568769687E91D4",
   "flowmon:tlsClientSessionId":"0x6639252CBCE69C998D520AB601D4154CA7BAB0EDC66D532778F6B95324826DE7",
   "flowmon:tlsExtensionTypes":"0x00000B000A002300160017000D002B002D0033001500FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
   "flowmon:tlsExtensionLengths":"0x110004000C0000000000000030000900020026006500FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
   "flowmon:tlsEllipticCurves":"0x1D0017001E0019001800000000000000",
   "flowmon:tlsEcPointFormats":66303,
   "flowmon:tlsClientKeyLength":2064,
   "flowmon:tlsIssuerCn":"DESKTOP-G4F9HHE\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:tlsSubjectCn":"DESKTOP-G4F9HHE\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:tlsSubjectOn":"\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:tlsValidityNotBefore":1702376400,
   "flowmon:tlsValidityNotAfter":1718187600,
   "flowmon:tlsSignatureAlg":668,
   "flowmon:tlsPublicKeyAlg":6,
   "flowmon:tlsPublicKeyLength":2048,
   "flowmon:tlsJa3Fingerprint":"0xFD8D299752E19F596CDEDD4F9248B1E2"
}
```

### HTTP

```json
{
   "@type":"ipfix.entry",
   "iana:octetDeltaCount":776,
   "iana:packetDeltaCount":5,
   "iana:protocolIdentifier":"TCP",
   "iana:ipClassOfService":0,
   "iana:tcpControlBits":".AP.S.",
   "iana:sourceTransportPort":49941,
   "iana:sourceIPv4Address":"192.168.66.2",
   "iana:ingressInterface":8,
   "iana:destinationTransportPort":80,
   "iana:destinationIPv4Address":"95.101.75.78",
   "iana:egressInterface":0,
   "iana:bgpSourceAsNumber":0,
   "iana:bgpDestinationAsNumber":34164,
   "iana:samplingInterval":0,
   "iana:samplingAlgorithm":0,
   "iana:sourceMacAddress":"00:0C:29:D2:9E:CE",
   "iana:postDestinationMacAddress":"48:8F:5A:19:A4:CD",
   "iana:ipVersion":4,
   "iana:flowStartMilliseconds":"2023-12-19T15:27:40.030Z",
   "iana:flowEndMilliseconds":"2023-12-19T15:27:40.147Z",
   "iana:applicationId":50331728,
   "flowmon:httpHost":"ctldl.windowsupdate.com\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000",
   "flowmon:httpUrl":"/msdownload/update/v3/static/trustedr/en/authrootstl.cab?887a94\u0000",
   "flowmon:httpMethodMask":1,
   "flowmon:httpStatusCode":0,
   "flowmon:httpUaOs":65535,
   "flowmon:httpUaOsMaj":65535,
   "flowmon:httpUaOsMin":65535,
   "flowmon:httpUaOsBld":65535,
   "flowmon:httpUaApp":65535,
   "flowmon:httpUaAppMaj":65535,
   "flowmon:httpUaAppMin":65535,
   "flowmon:httpUaAppBld":65535,
   "flowmon:tcpSynSize":52,
   "flowmon:tcpSynTtl":128,
   "iana:tcpWindowSize":64240
}
```


