# ethanol

An experimental environment for context-based flow artifact analysis. It implements two phases:

* building a context for the specified object using the set of predefined queries
* performing various analysis by custom analytical procedures that can combine rules, ML models and other methods.

A context is organized as a key,value collection. The key is a string label. The value is an artifact. 
The context is build by querying the source data store (currently represented by Flowmon REST API)
using the query specified with the builder.
The context is built from: 

* environment -  general system wide information, which is available for all analyzed artifacts. 
* local context - information related to the specific artifact type, this context is created by builders.

The artifact data model is typed. It means that each artifact has associated type, which 
defines its operations, fields and also the builders that contain queries executed to get the initial local context. 

For example, consider the `flow` artifacts, it contains set of operations, fields and builder for enriching the flow
with domain information:

```
artifact flow:
    operations:
        - start
        - end
    fields:
        - application_protocol
        - content
        - dest_hostname
        - dest_ip
        - dest_port
        - end_time
        - in_bytes
        - network_direction
        - out_bytes
        - packet_count
        - proto_info
        - protocol
        - src_ip
        - src_port
        - start_time
        - tcp_flags
        - transport_protocol
    builders:
        dest_fqdn: dns.reply as d where d.dst_ip = flow.src_ip and d.a = flow.dst_ip when last_preceding(d,flow) 
```
In this example, there is a single builder that creates a local context with label `domain` associated 
with a value, which is an artifact of type `dns` with `reply` action. `where` block specifies conditions
on fields and `when` expression is a temporal condition. 

Builders serve to initialize local context of the artifacts. The context can be modified by executing the rules. 
Rules enable to infer other information from the existing context and the artifact fields. 
Rules are executed using the initial context and the environment as inputs and they generate the final artifact context for further analysis. 
Note: NRules engine is used to express the rules and compute the final context.

Question: is it ok to do it this way? Can we precompute the context that will be useful for most of analysis tasks?
Note: For temporal operators see Trill, for instance.

## Environment and Packages 

## Usage

## Why?

## Documentation

The system is used to analyse the data source, e.g., NetFlow collection that provides an input data. For the selected object, its context 
is created and then analyzed. Using this approach it is possible to either provide a rich information for each object or to perfom advanced
security threats identification or network troubleshooting. 


### Context

A context is an ordered sequence of properties that define an environment for the artifacts.
Each context property holds the name/value pair of the property name and the object representing the property of a context.

### Context buidling

A context is built iterativelly:

* Initial step: Collect objects by query source database and using target artifact builders
* Iterative step: Apply rules (NRules) to produce context information for the target

## Install

## Acknowledgments

This project was developed in the frame of research initiative [Context-based Encrypted Traffic Analysis Using Flow Data](https://www.fit.vut.cz/research/project/1445/.en).

## See Also

### Suitable datasets

* [LITNET 2020 Dataset](https://dataset.litnet.lt/index.php)


## License

Not decided yet.
