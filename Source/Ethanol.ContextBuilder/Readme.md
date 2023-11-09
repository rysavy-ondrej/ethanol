# Ethanol.ContextBuilder

ContextBuilder is a tool designed specifically for processing and analyzing IPFIX records presented in JSON format. For every distinct IP endpoint identified, the tool computes its context. This context provides a systematic record of connections either initiated or accepted by hosts, along with specific details on TLS, DNS, and HTTP activities extracted from IPFIX data. An essential feature of ContextBuilder is its ability to augment these contexts with data from various external sources, ensuring a consistent format for integrating supplementary information. The end product of its processing is a comprehensive JSON output that represents this enhanced context. 

## Getting Started

This section provides guidelines for compiling, installing, and running the tool on various platforms: Windows, Linux, and MacOS X.

### Prerequisites

Before you can successfully compile and utilize the application, ensure that you have met the following prerequisites:

__dotnet 7 SDK:__ The application is built on the .NET 7 framework. Ensure that you have the Software Development Kit (SDK) for .NET 7 installed. This is necessary for building, running, and managing .NET projects. Download the SDK from the official .NET website.

__NuGet Packages:__ When you build the application, the necessary packages will be automatically fetched and installed from NuGet:

- AutoMapper: A convention-based object-object mapper. AutoMapper uses a fluent configuration API to define an object-object mapping strategy. It can be used to transform input objects into the desired output shape.

- ConsoleAppFramework: A micro-framework for creating console-based applications. It simplifies tasks like argument parsing and provides a more structured approach to building command-line tools.

- CsvHelper: A library that aids in reading and writing CSV data. It ensures seamless CSV data manipulation, supporting both reading from and writing to CSV files.

- JsonFlatFileDataStore: A simple library that allows you to store and retrieve .NET objects into and from JSON formatted flat files. It can be useful for applications that require lightweight persistence without the overhead of a full database system.

- Npgsql: The .NET data provider for PostgreSQL. It allows any application built on .NET to connect to a PostgreSQL database and perform CRUD operations.

- YamlDotNet: A .NET library for YAML processing. It can be used to deserialize objects from YAML formatted strings/files or serialize objects to YAML.

- ZLogger: A high-performance, zero allocation logging library. It provides structured logging capabilities with a focus on performance.

### Compilation

To compile the tool into a standalone executable application, follow these steps:

1. Ensure you have the .NET SDK 7.0 installed. You can download it from the official [Microsoft website](https://dotnet.microsoft.com/download/dotnet/7.0).

2. Open your terminal or command prompt.

3. Navigate to the directory where the project file (`Ethanol.ContextBuilder.csproj`) resides.

4. Run the following command to compile your project into a standalone executable:

   ```bash
   dotnet build -c Release -r [runtime] --self-contained 
   ```

   Replace `[runtime]` with the appropriate runtime identifier depending on your target platform:
   - `win-x64` for Windows
   - `linux-x64` for Linux
   - `osx-x64` for macOS X

   For example, to create a standalone executable for Windows, use:

   ```bash
   dotnet build -c Release -r win-x64 --self-contained 
   ```

After compilation, the binary executable and associated files can be found in the `bin\Release\net7.0\[runtime]\` directory.

### Running the Application

The application can be execute via [ethanol command line interface](..\Ethanol.Cli\Readme.md).

Dpending on the configuration the application reads/writes data from PostgreSQL database `ethanol`. See [Scripts](./Scripts/Readme.md) for instructions on the initialization of the 
database.


## Context Builder: Deep Dive into Functionality

The Context Builder serves as a tool designed to curate a rich contextual representation of network activities, specifically IPFIX flow records. Let's delve into its functionality to understand its workflow:

1. **Data Ingestion**: 
   The Context Builder begins its process by ingesting input data in the form of JSON objects. Each of these JSON objects represents a distinct IPFIX flow record, encapsulating all the vital details of that flow.

2. **Deserialization**: 
   Depending on the nature of the flow, the JSON object is systematically deserialized into appropriate objects. For instance, standard flows translate into 'IpFlow' objects, whereas flows related to the Domain Name System (DNS) become 'DnsFlow' objects. This approach ensures that no crucial IPFIX information gets overlooked.

3. **Temporal Grouping**: 
   To offer a structured analysis, the flows are organized using a time window mechanism that operates on a hopping principle. Both the window size and the hop duration are configurable, providing flexibility in how the data is temporally segmented.

4. **Spatial Grouping**: 
   Within these designated time windows, flows are further categorized based on their source and destination IP addresses. By doing so, the Context Builder can pinpoint individual endpoint hosts and collate the relevant flows for them, representing both outgoing (initiated) and incoming (accepted) connections.

5. **Context Construction**: 
   Upon identifying each host, a unique context is constructed for it. The foundation of this context is the host's IP address, which serves as a primary identifier. This is accompanied by a list of all connections associated with that host.

6. **Enrichment Process**: 
The basic context is first refined through an enrichment stage using information from an enrichment repository. This repository serves as a cache of various data elements, with examples ranging from Netify's catalog of Internet applications to derived tags based on host information, among others. Such enrichment adds complexity and dimension to the base context.

The data introduced by the Content Enricher must be accessible from some part of the system, with common sources including

Computation: The content enricher may compute the required data from information available in the context.

Environment: Additional data can be obtained from the system environment itself. 

Other system: Often, the Content Enricher must retrieve the required data from a separate system, which may be a database, file, etc.

7. **Context Refinement**:
   The enriched host-based context is then streamlined to derive a more concise output structure. This resultant structure comprises the following elements:
   - **Host Key**: The unique identifier of the host (typically its IP address).
   - **Initiated Connections**: Connections initiated by the host.
   - **Accepted Connections**: Connections accepted by the host.
   - **Tags**: Relevant tags associated with the host.
   - **HTTP Requests**: List of HTTP requests made by the host.
   - **DNS Resolutions**: Details of domain names resolved by the host.
   - **TLS Information**: Selective data derived from the TLS handshake.

8. **Output Relay**: 
   Finally, these structured rich context data are dispatched to the tool's output. They can be further processed and analysed by subsequent tools in the analytical pipeline.

## Credits

Ethanol is a collaborative project made possible by the following contributors:

- **Authors:**
  - Ondrej Rysavy, Brno University of Technology
  - Martin Holkovic, Flowmon Networks

This project was developed as part of the research initiative titled [Context-based Encrypted Traffic Analysis Using Flow Data](https://www.fit.vut.cz/research/project/1445/.en). This initiative aims to leverage flow data to enhance the analysis of encrypted network traffic, thereby improving the ability to identify and mitigate security threats in encrypted communications.

## License
TBD
