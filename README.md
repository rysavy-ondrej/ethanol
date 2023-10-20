# Ethanol: Context-Based Flow Artifact Analysis

Ethanol is an experimental platform engineered to research context-based flow artifact analysis. At its core, Ethanol delves into data sources, like NetFlow collections, to ingest and evaluate input data. For every chosen object within this data, Ethanol intricately builds its context and then subjects it to rigorous analysis. This methodology doesn't merely stop at generating enriched information about each object. It also broadens the horizons by enabling the identification of sophisticated security threats and facilitating advanced network troubleshooting.


## Core Features and Tools

Ethanol brings a suite of specialized tools designed for diverse functionalities:

* [ContexBuilder](Source/Ethanol.ContextBuilder/Readme.md) is a tool designed specifically for processing and analyzing IPFIX records presented in JSON format. For every distinct IP endpoint identified, the tool computes its context. This context provides a systematic record of connections either initiated or accepted by hosts, along with specific details on TLS, DNS, and HTTP activities extracted from IPFIX data. An essential feature of ContextBuilder is its ability to augment these contexts with data from various external sources, ensuring a consistent format for integrating supplementary information. The end product of its processing is a comprehensive JSON output that represents this enhanced context. 

* [ApplicationSonar](Source/Ethanol.ApplicationSonar/Readme.md) is a specialized tool developed to further analyze the context JSON produced by ContextBuilder. Leveraging the details provided in the context, ApplicationSonar aims to pinpoint both internet services and local processes associated with the enumerated connections. It delves into the intricacies of the context information to identify communication patterns, offering a refined view of the network activity. ApplicationSonar serves as asset for network administrators and security professionals, allowing them to present a comprehensive communication profile for specific endpoints and users.

* [MalwareSonar](Source/Ethanol.MalwareSonar/Readme.md) stands out as a unique tool tailored to harness the power of context-driven information for malware detection rather than using traditional methods like ML-based algorithms or signature models. Taking the generated context as its input, MalwareSonar seeks out indicative patterns of communication that might suggest malware activity. MalwareSonar translates identified Indicators of Compromise (IoC) of the malware samples analyzed in controlled Sandbox environment into discernible patterns within the context, effectively flagging potential malware activities. As such, MalwareSonar presents a novel approach to malware detection from network communication, leveraging the richness of context information to pinpoint suspicious communication behaviors that mirror known malware traits.

## Environment and Packages

Tho run the tool .NET SDK or .NET runtime version 7.0 is required.

## Usage

The easiest way is to use pre-built Docker Compose to set up an application environment consisting of  Ethanol for flow processing and context building, Fluent-Bit as a stream data router, and PostgreSql for context and host tag data storage.
In addition, the socat tool is used within the infrastructure to glue together the pipeline running on distributed nodes.

For a detailed walkthrough, refer to the step-by-step [guidelines](Publish/Docker/Readme.md) .

## Acknowledgments

This project was developed in the frame of research initiative [Context-based Encrypted Traffic Analysis Using Flow Data](https://www.fit.vut.cz/research/project/1445/.en).

The project uses [Pine Cone](https://icons8.com/icon/MrEybNsoqQoH/pine-cone") and [Center of Gravity](https://icons8.com/icon/JnSQSAhuEi7B/center-of-gravity) icons by [Icons8](https://icons8.com).
