# Ethanol

An experimental environment for context-based flow artifact analysis. 
The system can be used to analyse the data source, e.g., NetFlow collection that provides an input data. 
For the selected object, its context is created and then analyzed. Using this approach it is possible to either provide 
a rich information for each object or to perfom advanced security threats identification or network troubleshooting. 

It implements the following tools:

* [ContexBuilder](Source/Ethanol.ContextBuilder/Readme.md) - building a context for the specified object using the set of predefined queries
* [MalwareSonar](Source/Ethanol.MalwareSonar/Readme.md) - performing various analysis by custom analytical procedures that can combine rules, ML models and other methods.

## Environment and Packages

Tho run the tool .NET SDK or .NET runtime version 5.0 and greater is required. 

## Documentation

TBD

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
