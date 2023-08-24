# TODO

## Filter on target hosts

Enable to define target host filter for context builder. Add it as an program argument.

## IPFIX Input

Consider IPFIX as a possible input of the tool. This can be implemented in the pipeline as a preprocessor for input IPFIX data via integrating one of the following ipfix to json tools: 

* https://github.com/calmh/ipfixcat
* https://tools.netsa.cert.org/fixbuf/ipfix2json.html

## Smart ADS / Postgres

Fix the problem with inserting Smart ADS to database and using this data. 

Add to context record a new "column" that will contain enriched data from Smart ADS sources. This content of this column will be an array of key-value attributes:

```json
[
    { "source" : ADS-SOURCE , "value" : VALUE, "reliability" : RELIABILITY },
    ...
]
```

for example:

```json
[
    { "source" : "" , "value" : }, 


]
```

## ApplicationSonar

Demonstrate how to use context for determining local application / internet service for host's traffic.

```
HOST --- APP --- SERVICE --- FLOW
      |       |           |- FLOW
      |       |           |- FLOW
      |       |- SERVICE --- FLOW
      |                   |- FLOW
      |                   |- FLOW
      |- APP --- SERVICE --- FLOW
      |       |           |- FLOW
      |       |           |- FLOW
      |       |- SERVICE --- FLOW
      |                   |- FLOW
      |                   |- FLOW      
```