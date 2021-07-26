# ethanol
An experimental environment for context-based flow artifact analysis. It implements two phases:

* building a context for the specified object using the set of predefined queries
* performing various analysis by custom analytical procedures that can combine rules, ML models and other methods.

The context is defined as a collection of artifacts related to the artifact in question.
The context is build by querying the source data store (currently represented by Flowmon REST API). 
The context is built from: 

* environment -  general system wide information 
* local context - information related to artifact type

The artifact data model is typed. It means that each artifact has associated type, which 
defines its operations, fields and also the builders for the local contaxt. 

For example, consider the `flow` artifacts, it contains a builder for enriching the flow
with domain information:

```
input flow
select dns.reply where dns.dst_ip = flow.src_ip and dns.a = flow.dst_ip when last_preceding(dns,flow) 
```




## Environment and Packages 

## Usage

## Why?

## Documentation
The system is used to analyse the data source, e.g., NetFlow collection that provides an input data. For the selected object, its context 
is created and then analyzed. Using this approach it is possible to either provide a rich information for each object or to perfom advanced
security threats identification or network troubleshooting. 

## Install

## Acknowledgments
This project was developed in the frame of research initiative [Context-based Encrypted Traffic Analysis Using Flow Data](https://www.fit.vut.cz/research/project/1445/.en).
## See Also

## License
Not decided yet.
