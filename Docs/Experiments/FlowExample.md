
An example of a rich flow context is as follows:

```yaml
event: TCP@192.168.111.32:60379-104.26.6.39:443
validTime:
  start: 2021-10-16T17:15:00.0000000
  end: 2021-10-16T17:20:00.0000000
payload:
  flowKey:
    proto: TCP
    srcIp: 192.168.111.32
    srcPt: 60379
    dstIp: 104.26.6.39
    dstPt: 443
  context:
    tlsRecord:
      flow:
        proto: TCP
        srcIp: 192.168.111.32
        srcPt: 60379
        dstIp: 104.26.6.39
        dstPt: 443
      meters:
        packets: 122
        octets: 21960
        duration: 00:01:41.8530000
      tlsJa3: cd08e31494f9531f560d64c695473da9
      tlsServerName: prebid.smilewanted.com
      tlsServerCommonName: N/A
      processName: 
    domains:
      key:
        srcIp: 192.168.111.32
        dstIp: 104.26.6.39
      flows:
      - flow:
          proto: UDP
          srcIp: 192.168.111.1
          srcPt: 53
          dstIp: 192.168.111.32
          dstPt: 61992
        meters:
          packets: 1
          octets: 116
          duration: 00:00:00
        queryName: prebid.smilewanted.com
        responseData: 104.26.6.39
      - flow:
          proto: UDP
          srcIp: 192.168.111.1
          srcPt: 53
          dstIp: 192.168.111.32
          dstPt: 55275
        meters:
          packets: 1
          octets: 116
          duration: 00:00:00
        queryName: static.smilewanted.com
        responseData: 104.26.6.39
    tlsClientFlows:
      key:
        srcIp: 192.168.111.32
        ja3Fingerprint: cd08e31494f9531f560d64c695473da9
      flows:
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 60379
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 122
          octets: 21960
          duration: 00:01:41.8530000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 60378
          dstIp: 135.125.160.77
          dstPt: 443
        meters:
          packets: 12
          octets: 2772
          duration: 00:00:05.2050000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: c.eu1.dyntrk.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 60378
          dstIp: 198.54.12.127
          dstPt: 443
        meters:
          packets: 13
          octets: 5381
          duration: 00:00:02.2370000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sync.search.spotxchange.com
        tlsServerCommonName: '*.search.spotxchange.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 60533
          dstIp: 142.250.182.163
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:01.2569999
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: csi.gstatic.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 60682
          dstIp: 54.209.16.83
          dstPt: 443
        meters:
          packets: 11
          octets: 1928
          duration: 00:00:06.1880000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sync.srv.stackadapt.com
        tlsServerCommonName: '*.srv.stackadapt.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59936
          dstIp: 66.155.71.149
          dstPt: 443
        meters:
          packets: 11
          octets: 1792
          duration: 00:00:05.1110000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: pixel-sync.sitescout.com
        tlsServerCommonName: '*.sitescout.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 60855
          dstIp: 198.54.12.127
          dstPt: 443
        meters:
          packets: 9
          octets: 2593
          duration: 00:00:00.9540000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sync.search.spotxchange.com
        tlsServerCommonName: '*.search.spotxchange.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59837
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0260000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59804
          dstIp: 104.64.166.98
          dstPt: 443
        meters:
          packets: 9
          octets: 969
          duration: 00:00:10.4330000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: docs.microsoft.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59787
          dstIp: 76.223.111.131
          dstPt: 443
        meters:
          packets: 34
          octets: 4862
          duration: 00:00:35.1710000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: match.adsrvr.org
        tlsServerCommonName: '*.adsrvr.org'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59715
          dstIp: 70.42.32.127
          dstPt: 443
        meters:
          packets: 10
          octets: 1935
          duration: 00:00:24.6070000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: b1h.zemanta.com
        tlsServerCommonName: '*.zemanta.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 61049
          dstIp: 51.75.146.200
          dstPt: 443
        meters:
          packets: 11
          octets: 1992
          duration: 00:00:25.2040000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: id5-sync.com
        tlsServerCommonName: '*.id5-sync.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59676
          dstIp: 104.21.73.110
          dstPt: 443
        meters:
          packets: 10
          octets: 1617
          duration: 00:00:00.1019999
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: basher.ezodn.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 61121
          dstIp: 18.156.147.57
          dstPt: 443
        meters:
          packets: 32
          octets: 5218
          duration: 00:00:38.6510000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: pixel.advertising.com
        tlsServerCommonName: pixel.advertising.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59583
          dstIp: 87.248.118.22
          dstPt: 443
        meters:
          packets: 12
          octets: 1792
          duration: 00:00:00.1150000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ads.yahoo.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 61319
          dstIp: 2.21.172.96
          dstPt: 443
        meters:
          packets: 2442
          octets: 147603
          duration: 00:00:27.6320000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: assets.msn.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 61358
          dstIp: 216.58.201.66
          dstPt: 443
        meters:
          packets: 9
          octets: 1076
          duration: 00:00:00.0890000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: www.googletagservices.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59335
          dstIp: 104.208.16.90
          dstPt: 443
        meters:
          packets: 7
          octets: 967
          duration: 00:00:00.5800000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: browser.events.data.msn.com
        tlsServerCommonName: '*.events.data.microsoft.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59240
          dstIp: 69.173.144.143
          dstPt: 443
        meters:
          packets: 9
          octets: 3285
          duration: 00:00:00.1580000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: fastlane.rubiconproject.com
        tlsServerCommonName: '*.rubiconproject.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59197
          dstIp: 65.9.94.127
          dstPt: 443
        meters:
          packets: 12
          octets: 1868
          duration: 00:00:01.5270000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: d.agkn.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 61588
          dstIp: 216.58.201.66
          dstPt: 443
        meters:
          packets: 6
          octets: 833
          duration: 00:00:00.0740000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: adservice.google.cz
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59120
          dstIp: 173.194.150.231
          dstPt: 443
        meters:
          packets: 9
          octets: 2015
          duration: 00:00:00.1350000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: r1---sn-2gb7sn7s.c.2mdn.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59119
          dstIp: 35.186.238.175
          dstPt: 443
        meters:
          packets: 17
          octets: 2348
          duration: 00:00:00.5639999
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: cz-gmtdmp.mookie1.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59003
          dstIp: 173.194.150.232
          dstPt: 443
        meters:
          packets: 9
          octets: 2015
          duration: 00:00:00.1230000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: r2---sn-2gb7sn7s.c.2mdn.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 61771
          dstIp: 185.86.137.110
          dstPt: 443
        meters:
          packets: 16
          octets: 2073
          duration: 00:00:44.7170000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: rtb-csync.smartadserver.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58928
          dstIp: 13.107.6.158
          dstPt: 443
        meters:
          packets: 14
          octets: 5151
          duration: 00:00:00.1060000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ent-api.msn.com
        tlsServerCommonName: '*.msn.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58858
          dstIp: 69.173.144.143
          dstPt: 443
        meters:
          packets: 47
          octets: 26062
          duration: 00:01:41.7540000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: fastlane.rubiconproject.com
        tlsServerCommonName: '*.rubiconproject.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58819
          dstIp: 198.54.12.127
          dstPt: 443
        meters:
          packets: 5
          octets: 780
          duration: 00:00:00.2670000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sync.search.spotxchange.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 61973
          dstIp: 23.47.212.25
          dstPt: 443
        meters:
          packets: 8
          octets: 1745
          duration: 00:00:00.0930000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: cs.media.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 62092
          dstIp: 104.18.12.5
          dstPt: 443
        meters:
          packets: 14
          octets: 2785
          duration: 00:00:05.6920000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: a.tribalfusion.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58612
          dstIp: 172.217.23.206
          dstPt: 443
        meters:
          packets: 12
          octets: 1926
          duration: 00:00:32.1049999
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: gcdn.2mdn.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58544
          dstIp: 65.9.94.128
          dstPt: 443
        meters:
          packets: 81
          octets: 28213
          duration: 00:01:41.9800000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: pb-server.ezoic.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 62263
          dstIp: 18.184.29.12
          dstPt: 443
        meters:
          packets: 13
          octets: 2048
          duration: 00:00:00.1350000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ice.360yield.com
        tlsServerCommonName: '*.360yield.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 62285
          dstIp: 178.250.2.146
          dstPt: 443
        meters:
          packets: 11
          octets: 1826
          duration: 00:00:00.1900000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: gum.criteo.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 62403
          dstIp: 35.156.135.60
          dstPt: 443
        meters:
          packets: 9
          octets: 2893
          duration: 00:00:00.1710000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: pm.w55c.net
        tlsServerCommonName: '*.w55c.net'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58336
          dstIp: 204.79.197.200
          dstPt: 443
        meters:
          packets: 7
          octets: 967
          duration: 00:00:00.0250000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: r.bing.com
        tlsServerCommonName: www.bing.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58309
          dstIp: 193.232.148.141
          dstPt: 443
        meters:
          packets: 22
          octets: 3670
          duration: 00:00:06.9120000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: px.adhigh.net
        tlsServerCommonName: ltmse.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 62452
          dstIp: 172.217.23.225
          dstPt: 443
        meters:
          packets: 8
          octets: 913
          duration: 00:00:00.0730000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: cdn.ampproject.org
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58282
          dstIp: 52.111.236.6
          dstPt: 443
        meters:
          packets: 16
          octets: 3867
          duration: 00:00:00.3250000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: webshell.suite.office.com
        tlsServerCommonName: webshell.suite.office.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 62497
          dstIp: 2.21.172.96
          dstPt: 443
        meters:
          packets: 20
          octets: 7236
          duration: 00:00:23.6200000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: assets.msn.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 62648
          dstIp: 35.158.18.139
          dstPt: 443
        meters:
          packets: 34
          octets: 6678
          duration: 00:00:03.2120000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: btlr.sharethrough.com
        tlsServerCommonName: '*.sharethrough.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58093
          dstIp: 54.250.62.115
          dstPt: 443
        meters:
          packets: 8
          octets: 1113
          duration: 00:00:01.5650000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: cc.adingo.jp
        tlsServerCommonName: '*.adingo.jp'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58031
          dstIp: 195.113.232.74
          dstPt: 443
        meters:
          packets: 170
          octets: 8070
          duration: 00:00:20.0360000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: img-prod-cms-rt-microsoft-com.akamaized.net
        tlsServerCommonName: a248.e.akamai.net
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 58015
          dstIp: 23.47.208.212
          dstPt: 443
        meters:
          packets: 16
          octets: 2315
          duration: 00:00:20.3100000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ads.pubmatic.com
        tlsServerCommonName: '*.pubmatic.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 62888
          dstIp: 35.186.193.173
          dstPt: 443
        meters:
          packets: 16
          octets: 2731
          duration: 00:00:03.4340000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: gcm.ctnsnet.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 57828
          dstIp: 74.125.206.156
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0990000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: stats.g.doubleclick.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 57811
          dstIp: 185.33.220.244
          dstPt: 443
        meters:
          packets: 15
          octets: 4259
          duration: 00:00:27.9700000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ib.adnxs.com
        tlsServerCommonName: '*.adnxs.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 62967
          dstIp: 66.155.40.24
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.5290000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: gmpg.org
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 57749
          dstIp: 198.54.12.127
          dstPt: 443
        meters:
          packets: 5
          octets: 780
          duration: 00:00:00.2690000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sync.search.spotxchange.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 57729
          dstIp: 172.217.23.226
          dstPt: 443
        meters:
          packets: 7
          octets: 883
          duration: 00:00:00.0700000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: securepubads.g.doubleclick.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 57718
          dstIp: 104.21.62.192
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0480000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: shellgeek.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 63139
          dstIp: 104.21.62.192
          dstPt: 443
        meters:
          packets: 32
          octets: 2524
          duration: 00:00:00.6580000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: shellgeek.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 63169
          dstIp: 18.185.143.19
          dstPt: 443
        meters:
          packets: 12
          octets: 3735
          duration: 00:00:02.9620000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: x.bidswitch.net
        tlsServerCommonName: '*.bidswitch.net'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 57582
          dstIp: 172.217.23.237
          dstPt: 443
        meters:
          packets: 13
          octets: 2400
          duration: 00:00:00.1380000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: accounts.google.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 63222
          dstIp: 66.155.71.149
          dstPt: 443
        meters:
          packets: 19
          octets: 2903
          duration: 00:00:07.7660000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: pixel-sync.sitescout.com
        tlsServerCommonName: '*.sitescout.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 63333
          dstIp: 35.190.0.66
          dstPt: 443
        meters:
          packets: 21
          octets: 3427
          duration: 00:00:02.5370000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ads.travelaudience.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 63370
          dstIp: 185.33.220.100
          dstPt: 443
        meters:
          packets: 12
          octets: 2793
          duration: 00:00:24.6930000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: secure.adnxs.com
        tlsServerCommonName: '*.adnxs.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 57318
          dstIp: 172.217.23.228
          dstPt: 443
        meters:
          packets: 13
          octets: 1113
          duration: 00:00:46.1599999
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: www.google.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 63484
          dstIp: 185.183.112.155
          dstPt: 443
        meters:
          packets: 8
          octets: 1831
          duration: 00:00:00.1900000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sync.adotmob.com
        tlsServerCommonName: sync.adotmob.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 57227
          dstIp: 69.173.144.143
          dstPt: 443
        meters:
          packets: 9
          octets: 3285
          duration: 00:00:00.1650000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: fastlane.rubiconproject.com
        tlsServerCommonName: '*.rubiconproject.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 63563
          dstIp: 192.0.77.48
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0560000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: s.w.org
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 57070
          dstIp: 20.42.73.24
          dstPt: 443
        meters:
          packets: 52
          octets: 53240
          duration: 00:00:06.8540000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: browser.pipe.aria.microsoft.com
        tlsServerCommonName: '*.events.data.microsoft.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 63833
          dstIp: 204.79.197.200
          dstPt: 443
        meters:
          packets: 7
          octets: 967
          duration: 00:00:00.0280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: r.bing.com
        tlsServerCommonName: www.bing.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56868
          dstIp: 65.9.94.70
          dstPt: 443
        meters:
          packets: 10
          octets: 1757
          duration: 00:00:00.0900000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sb.scorecardresearch.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56662
          dstIp: 195.113.232.72
          dstPt: 443
        meters:
          packets: 9
          octets: 1015
          duration: 00:00:12.1420000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: img-s-msn-com.akamaized.net
        tlsServerCommonName: a248.e.akamai.net
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 64111
          dstIp: 37.157.4.39
          dstPt: 443
        meters:
          packets: 30
          octets: 4618
          duration: 00:00:38.4360000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: c1.adform.net
        tlsServerCommonName: track.adform.net
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56552
          dstIp: 104.208.16.90
          dstPt: 443
        meters:
          packets: 62
          octets: 57913
          duration: 00:00:13.6700000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: browser.events.data.msn.com
        tlsServerCommonName: '*.events.data.microsoft.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56494
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0260000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56324
          dstIp: 172.217.23.226
          dstPt: 443
        meters:
          packets: 6
          octets: 843
          duration: 00:00:00.0760000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: securepubads.g.doubleclick.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 64658
          dstIp: 54.250.62.115
          dstPt: 443
        meters:
          packets: 16
          octets: 2658
          duration: 00:00:31.6500000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: cc.adingo.jp
        tlsServerCommonName: '*.adingo.jp'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 64676
          dstIp: 147.75.38.124
          dstPt: 443
        meters:
          packets: 72
          octets: 29685
          duration: 00:01:41.9100000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.a-mo.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 64687
          dstIp: 69.173.144.139
          dstPt: 443
        meters:
          packets: 12
          octets: 3562
          duration: 00:00:15.6450000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: pixel.rubiconproject.com
        tlsServerCommonName: '*.rubiconproject.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 64705
          dstIp: 92.123.9.160
          dstPt: 443
        meters:
          packets: 11
          octets: 2435
          duration: 00:00:00.1840000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: eus.rubiconproject.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56014
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0260000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 64885
          dstIp: 185.86.137.133
          dstPt: 443
        meters:
          packets: 17
          octets: 2986
          duration: 00:00:43.9500000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sync.smartadserver.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 55853
          dstIp: 69.173.144.139
          dstPt: 443
        meters:
          packets: 10
          octets: 2587
          duration: 00:00:17.4280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: pixel-eu.rubiconproject.com
        tlsServerCommonName: '*.rubiconproject.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 64934
          dstIp: 216.58.201.65
          dstPt: 443
        meters:
          packets: 13
          octets: 1833
          duration: 00:00:00.0810000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: a8ca48555c2705d637170f10c35e7b49.safeframe.googlesyndication.co
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 55807
          dstIp: 23.47.213.137
          dstPt: 443
        meters:
          packets: 7
          octets: 1668
          duration: 00:00:00.0740000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ads.stickyadstv.com
        tlsServerCommonName: ads.stickyadstv.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 55705
          dstIp: 65.9.98.231
          dstPt: 443
        meters:
          packets: 10
          octets: 1558
          duration: 00:00:00.0160000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: d27xxe7juh1us6.cloudfront.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 65169
          dstIp: 65.9.94.98
          dstPt: 443
        meters:
          packets: 10
          octets: 1555
          duration: 00:00:00.0390000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: go.ezoic.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 55515
          dstIp: 18.159.85.44
          dstPt: 443
        meters:
          packets: 22
          octets: 10937
          duration: 00:00:03.5100000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: pm.w55c.net
        tlsServerCommonName: '*.w55c.net'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 65299
          dstIp: 65.9.94.115
          dstPt: 443
        meters:
          packets: 17
          octets: 1801
          duration: 00:00:00.0290000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sf.ezoiccdn.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 55360
          dstIp: 172.217.23.234
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0690000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: fonts.googleapis.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 55303
          dstIp: 69.173.144.143
          dstPt: 443
        meters:
          packets: 7
          octets: 2166
          duration: 00:00:00.1660000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: fastlane.rubiconproject.com
        tlsServerCommonName: '*.rubiconproject.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 55228
          dstIp: 13.107.6.158
          dstPt: 443
        meters:
          packets: 62
          octets: 39857
          duration: 00:00:10.0800000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: business.bing.com
        tlsServerCommonName: www.bing.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 54982
          dstIp: 91.228.74.226
          dstPt: 443
        meters:
          packets: 14
          octets: 2384
          duration: 00:00:02.8280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: cms.quantserve.com
        tlsServerCommonName: '*.quantserve.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 54974
          dstIp: 69.173.144.143
          dstPt: 443
        meters:
          packets: 11
          octets: 4568
          duration: 00:00:30.7770000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: fastlane.rubiconproject.com
        tlsServerCommonName: '*.rubiconproject.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 54521
          dstIp: 65.9.94.98
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0780000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: go.ezoic.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 54219
          dstIp: 20.82.209.183
          dstPt: 443
        meters:
          packets: 15
          octets: 4009
          duration: 00:00:01.7200000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: arc.msn.com
        tlsServerCommonName: arc.msn.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 54178
          dstIp: 172.67.154.71
          dstPt: 443
        meters:
          packets: 10
          octets: 1675
          duration: 00:00:00.0930000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: id.a-mx.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 54126
          dstIp: 185.255.84.150
          dstPt: 443
        meters:
          packets: 15
          octets: 4123
          duration: 00:00:20.1560000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: hb-api.omnitagjs.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 54113
          dstIp: 172.67.161.209
          dstPt: 443
        meters:
          packets: 77
          octets: 4454
          duration: 00:00:00.4890000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: go.ezodn.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 53654
          dstIp: 185.29.134.244
          dstPt: 443
        meters:
          packets: 9
          octets: 2402
          duration: 00:00:03.0280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sync.mathtag.com
        tlsServerCommonName: '*.mathtag.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 53409
          dstIp: 204.79.197.203
          dstPt: 443
        meters:
          packets: 7
          octets: 967
          duration: 00:00:00.0230000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: api.msn.com
        tlsServerCommonName: '*.msn.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 53337
          dstIp: 198.54.12.127
          dstPt: 443
        meters:
          packets: 10
          octets: 3355
          duration: 00:00:00.9760000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: sync.search.spotxchange.com
        tlsServerCommonName: '*.search.spotxchange.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 53321
          dstIp: 216.58.201.70
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0800000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: s0.2mdn.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 53318
          dstIp: 185.64.189.114
          dstPt: 443
        meters:
          packets: 10
          octets: 1841
          duration: 00:00:00.0740000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: simage4.pubmatic.com
        tlsServerCommonName: '*.pubmatic.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 52779
          dstIp: 157.240.30.8
          dstPt: 443
        meters:
          packets: 16
          octets: 2194
          duration: 00:00:00.5750000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ad.atdmt.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 52659
          dstIp: 172.217.23.206
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0720000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: www.google-analytics.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 52099
          dstIp: 52.142.114.2
          dstPt: 443
        meters:
          packets: 14
          octets: 3783
          duration: 00:00:00.2330000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: c.msn.com
        tlsServerCommonName: c.msn.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51931
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51823
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51557
          dstIp: 172.217.23.194
          dstPt: 443
        meters:
          packets: 7
          octets: 883
          duration: 00:00:00.0750000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ade.googlesyndication.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51431
          dstIp: 23.47.209.6
          dstPt: 443
        meters:
          packets: 10
          octets: 2761
          duration: 00:00:00.1610000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ssum-sec.casalemedia.com
        tlsServerCommonName: san.casalemedia.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51304
          dstIp: 51.89.9.254
          dstPt: 443
        meters:
          packets: 26
          octets: 3307
          duration: 00:00:33.2550000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: onetag-sys.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51286
          dstIp: 54.250.62.115
          dstPt: 443
        meters:
          packets: 5
          octets: 729
          duration: 00:00:00.5430000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: cc.adingo.jp
        tlsServerCommonName: '*.adingo.jp'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51226
          dstIp: 20.42.73.24
          dstPt: 443
        meters:
          packets: 10
          octets: 3782
          duration: 00:00:10.2870000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: browser.pipe.aria.microsoft.com
        tlsServerCommonName: '*.events.data.microsoft.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51141
          dstIp: 185.64.189.115
          dstPt: 443
        meters:
          packets: 39
          octets: 11313
          duration: 00:01:06.5970000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: image6.pubmatic.com
        tlsServerCommonName: '*.pubmatic.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51137
          dstIp: 69.173.144.143
          dstPt: 443
        meters:
          packets: 9
          octets: 3285
          duration: 00:00:00.1590000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: fastlane.rubiconproject.com
        tlsServerCommonName: '*.rubiconproject.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51007
          dstIp: 142.250.182.163
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:01.2689999
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: csi.gstatic.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 50806
          dstIp: 172.217.23.228
          dstPt: 443
        meters:
          packets: 13
          octets: 1113
          duration: 00:00:51.1580000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: www.google.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 50759
          dstIp: 23.47.213.137
          dstPt: 443
        meters:
          packets: 7
          octets: 1603
          duration: 00:00:00.0780000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ads.stickyadstv.com
        tlsServerCommonName: ads.stickyadstv.com
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 50661
          dstIp: 34.96.105.8
          dstPt: 443
        meters:
          packets: 15
          octets: 2439
          duration: 00:00:02.7759999
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: tr.blismedia.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 50554
          dstIp: 172.217.23.226
          dstPt: 443
        meters:
          packets: 13
          octets: 2136
          duration: 00:00:00.0859999
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: adservice.google.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 50443
          dstIp: 65.9.94.128
          dstPt: 443
        meters:
          packets: 9
          octets: 953
          duration: 00:00:00.0460000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: pb-server.ezoic.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 50265
          dstIp: 185.86.137.110
          dstPt: 443
        meters:
          packets: 16
          octets: 1974
          duration: 00:00:44.7170000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: rtb-csync.smartadserver.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 49992
          dstIp: 204.79.197.203
          dstPt: 443
        meters:
          packets: 54
          octets: 7337
          duration: 00:00:02.5960000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ntp.msn.com
        tlsServerCommonName: '*.msn.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 49676
          dstIp: 52.49.74.33
          dstPt: 443
        meters:
          packets: 51
          octets: 19981
          duration: 00:01:41.7120000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ads.yieldmo.com
        tlsServerCommonName: '*.yieldmo.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 49612
          dstIp: 172.217.23.194
          dstPt: 443
        meters:
          packets: 7
          octets: 883
          duration: 00:00:00.0709999
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ade.googlesyndication.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 49594
          dstIp: 173.194.150.232
          dstPt: 443
        meters:
          packets: 9
          octets: 2015
          duration: 00:00:00.1270000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: r2---sn-2gb7sn7s.c.2mdn.net
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 49480
          dstIp: 174.137.133.49
          dstPt: 443
        meters:
          packets: 14
          octets: 4507
          duration: 00:00:03.4460000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: dsp.adkernel.com
        tlsServerCommonName: '*.adkernel.com'
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 49418
          dstIp: 18.156.0.31
          dstPt: 443
        meters:
          packets: 12
          octets: 2783
          duration: 00:00:02.9190000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: ups.analytics.yahoo.com
        tlsServerCommonName: N/A
        processName: 
    bagOfFlows:
      key:
        dstIp: 104.26.6.39
        dstPort: 443
        protocol: TCP
      flows:
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56494
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0260000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59837
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0260000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51823
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56014
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0260000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51931
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 60379
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 122
          octets: 21960
          duration: 00:01:41.8530000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
    flowBurst:
      key:
        srcIp: 192.168.111.32
        dstIp: 104.26.6.39
        dstPort: 443
        protocol: TCP
      flows:
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56494
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0260000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 59837
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0260000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51823
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 56014
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0260000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 51931
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 7
          octets: 873
          duration: 00:00:00.0280000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
      - flow:
          proto: TCP
          srcIp: 192.168.111.32
          srcPt: 60379
          dstIp: 104.26.6.39
          dstPt: 443
        meters:
          packets: 122
          octets: 21960
          duration: 00:01:41.8530000
        tlsJa3: cd08e31494f9531f560d64c695473da9
        tlsServerName: prebid.smilewanted.com
        tlsServerCommonName: N/A
        processName: 
    plainHttpFlows:
      key: 
      flows: []
```