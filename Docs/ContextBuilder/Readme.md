# Context Builder v1.0 Documentation

Context Builder is an experimental tool that computes a rich information context for hosts observed in the input NetFLow data.
It can be used as a standalone tool or integrated in the monitoring infrastructure via [Fluent Bit](https://docs.fluentbit.io/manual/).


## Integration via FLuent Bit

It is possible to exploit advantages of Fluent Bit to easily integrate the Context Builder to the monitoring infrastructure. 

Fluent Bit provides a Data Pipeline that represents a flow of data that goes through the inputs (sources), filers, and output (sinks).
The Context Builder can be a part of this pipeline.

To integrate with FLuent Bit, we need o run the Fluent Bit and Context Builder and link them together. In this scenario 
the Fluent Bit handles all input/output operations and it forwards prepared NetFlow data to Context Builder that emits the output back to
Fluent Bit for further routing.

TODO: Describe this case...