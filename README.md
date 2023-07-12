# Ethanol

An experimental environment for context-based flow artifact analysis. 
The system can be used to analyse the data source, e.g., NetFlow collection that provides an input data. 
For the selected object, its context is created and then analyzed. Using this approach it is possible to either provide 
a rich information for each object or to perfom advanced security threats identification or network troubleshooting. 

It implements the following tools:

* [ContexBuilder](Source/Ethanol.ContextBuilder/Readme.md) - building a context for the specified object using the set of predefined queries.
* [ApplicationSonar](Source/Ethanol.ApplicationSonar/Readme.md) - detects the communicating applications with the computed context.
* [MalwareSonar](Source/Ethanol.MalwareSonar/Readme.md) - performing various analysis by custom analytical procedures that can combine rules, ML models and other methods.

## Environment and Packages

Tho run the tool .NET SDK or .NET runtime version 7.0 is required.

## Documentation

The documentation is available for [Context Builder 1.0.](Docs/ContextBuilder/Readme.md)

## Usage

The easiest way is to use pre-built Docker Compose to set up an application environment consisting of  Ethanol for flow processing and context building, Fluent-Bit as a stream data router, and PostgreSql for context and host tag data storage.
In addition, the socat tool is used within the infrastructure to glue together the pipeline running on distributed nodes.

See [instructions](Publish/Docker/Readme.md) for the step-by-step guidelines on how to use.

## Acknowledgments

This project was developed in the frame of research initiative [Context-based Encrypted Traffic Analysis Using Flow Data](https://www.fit.vut.cz/research/project/1445/.en).

The project uses [Pine Cone](https://icons8.com/icon/MrEybNsoqQoH/pine-cone") and [Center of Gravity](https://icons8.com/icon/JnSQSAhuEi7B/center-of-gravity) icons by [Icons8](https://icons8.com).


### References

* [Temporal Database Concepts](https://www.cs.uct.ac.za/mit_notes/database/htmls/chp18.html)
* [Trill Blog](https://cloudblogs.microsoft.com/opensource/2019/03/28/trill-101-how-to-add-temporal-queries-to-your-applications/)

### Suitable datasets

* [LITNET 2020 Dataset](https://dataset.litnet.lt/index.php)

## License

TBD
