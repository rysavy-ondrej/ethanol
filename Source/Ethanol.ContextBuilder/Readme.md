# Ethanol.ContextBuilder

ContextBuilder is a tool designed specifically for processing and analyzing IPFIX records presented in JSON format. For every distinct IP endpoint identified, the tool computes its context. This context provides a systematic record of connections either initiated or accepted by hosts, along with specific details on TLS, DNS, and HTTP activities extracted from IPFIX data. An essential feature of ContextBuilder is its ability to augment these contexts with data from various external sources, ensuring a consistent format for integrating supplementary information. The end product of its processing is a comprehensive JSON output that represents this enhanced context. 

## Getting Started

This section will guide you through the process of compaling, installing and running the tool on various platforms: Windows, Linux, and macOS X.

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
   dotnet publish -c Release -r [runtime] --self-contained true -p:PublishSingleFile=true
   ```

   Replace `[runtime]` with the appropriate runtime identifier depending on your target platform:
   - `win-x64` for Windows
   - `linux-x64` for Linux
   - `osx-x64` for macOS X

   For example, to create a standalone executable for Windows, use:

   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true
   ```

After compilation, the binary executable and associated files can be found in the `bin\Release\net7.0\[runtime]\publish\` directory.

### Running the Application

**Windows**:
1. Navigate to the directory where your compiled executable resides.
2. Double-click on the executable or run it from the command prompt.

**Linux**:
1. Open a terminal.
2. Navigate to the directory where your compiled executable resides.
3. Give execute permissions to the executable:
 
   ```bash
   chmod +x Ethanol.ContextBuilder
   ```
4. Run the application:
   ```bash
   ./Ethanol.ContextBuilder
   ```

**macOS X**:
1. Open a terminal.
2. Navigate to the directory where your compiled executable resides.
3. Run the application:
   ```bash
   ./Ethanol.ContextBuilder
   ```

### Installation

After compiling the application to a single-file executable, you can directly run the application without any further installation. You simply need to make sure that the executable has the appropriate permissions (like the execute permission on Linux or macOS). Alternatively, manual installation of the compiled application can be done depending on the platform:

- Windows: Place the executable in an appropriate directory, and optionally create shortcuts to it. You can also add the directory to your PATH if you want to run it from any command prompt.

- Linux: Move the single-file executable to a directory like `/usr/local/bin` or `/opt/[application-name]` and ensure it has execute permissions (chmod +x [executable-name]).

- macOS: Similar to Linux, move the executable to a suitable location, such as `/usr/local/bin`, and set the execute permissions.

### Configuration

The configuration file is vital for specifying options related to flow processing and the source of data enrichment. 
The file defines the application's behavior, offering both adaptability and precision tailored to specific requirements. 

Here's the structure of the configuration file:

```yaml
window-size: <time duration>
window-hop: <time duration>
target-prefix: <CIDR notation address>
tag-enricher:
    postgres:
        server: <server address>
        port: <port number>
        database: <database name>
        user: <username>
        password: <password>
        tableName: <table name>
```

### Field Descriptions:

- **window-size**: Specifies the duration of the processing data window. It's formatted as HH:MM:SS, representing hours, minutes, and seconds respectively.

- **window-hop**: Defines the hop or step size between consecutive windows. It shares the same format as `window-size`.

- **target-prefix**: Indicates the network prefix or subnet of interest to the application. The value is provided in CIDR notation.

- **tag-enricher**: This section houses details for tag enrichment. The enrichment details can be sourced from various systems, and each system has its subsection.

- **postgres**: Contains settings related to postrgres enrichment source:
      - **server**: The server's address or hostname.
      - **port**: The listening port number of the server.
      - **database**: The name of the database for connection.
      - **user**: Username for database authentication.
      - **password**: Password for the provided username.
      - **tableName**: Designates the name of the database table containing enrichment data.

### Example Configuration:

Here's a practical example to illustrate the configuration:

```yaml
window-size: 00:05:00
window-hop: 00:05:00
target-prefix: 192.168.111.0/24
tag-enricher:
    postgres:
        server: localhost
        port: 1605
        database: ethanol
        user: postgres
        password: postgres
        tableName: enrichment_data
```

In this example, the window size and hop are set to 5 minutes, and the target network prefix is "192.168.111.0/24". The tag enrichment source specified is a PostgreSQL database with the provided details.

## Usage

The tool provides several commands:

```
Usage: Ethanol.ContextBuilder <Command>

Commands:
  Build-Context    Builds the context for flows.
  List-Modules     Provides information on available modules.
  help             Display help.
  version          Display version.
```

Context is build with `Build-Context` command:

```
> Ethanol.ContextBuilder.exe Build-Context --help
Usage: Ethanol.ContextBuilder Build-Context [options...]

Builds the context for flows.

Options:
  -r, --input-reader <String>          The reader module for processing input stream. (Required)
  -c, --configuration-file <String>    The configuration file used to configure the processing. (Required)
  -w, --output-writer <String>         The writer module for producing the output. (Required)
```

To use the tool, three modules need to be specified:

* Reader necessary to process correctly the input files or stream.
* Builder performing the main task - building the context according to the given parameters.
* Writer used to produce the output in the required format and target.

The list of available modules is obtained by the following command:

```
> Ethanol.ContextBuilder.exe List-Modules
READERS:
  NfdumpCsv    Reads CSV file produced by nfdump.
BUILDERS:
  FlowContext    Builds the context for TLS flows in the source IPFIX stream.
  HostContext    Builds the context for IP hosts identified in the source IPFIX stream.
  IpHostContext    Builds the context for IP hosts identified in the source IPFIX observable.
ENRICHERS:
  IpHostContextEnricher    Enriches the context for IP hosts from the provided data.
  VoidContextEnricher    Does not enrich the context. Used to fill the space in the processing pipeline.
WRITERS:
  JsonWriter    Writes NDJSON formatted file for computed context.
  YamlWriter    Writes YAML formatted file for computed context.
```

The builder is configured by providing a [configuration file](Configuration-file.md).


## Building Host Context

To building host context use the following command:

```
> Ethanol.ContextBuilder.exe Build-Context -r FlowmonJson:{file=webuser.flows.json} -c config.yaml -w JsonWriter:{file=webuser.ctx.json}
```

where the content of configuration file `config.yaml` is:

```yaml
window-size: 00:05:00
window-hop: 00:05:00
target-prefix: 192.168.111.0/24
flow-tag-enricher:
    jsonfile:
        filename: webuser.tcp.json
        collection: flows
netify-tag-enricher:
    jsonfile:
        filename: netify.json
        collection: apps,ips
host-tag-enricher:
    csvfile:
        filename: webuser.smartads.csv
```


## Contributing
Larger projects often have sections on contributing to their project, in which contribution instructions are outlined. Sometimes, this is a separate file. If you have specific contribution preferences, explain them so that other developers know how to best contribute to your work. To learn more about how to help others contribute, check out the guide for setting guidelines for repository contributors.

### Setup
These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Running the tests
Explain how to run the automated tests for this system. CI/CD Details, ...

### Logging
How is logging configured and what is the location of the log files

### Versioning
We use SemVer for versioning. For the versions available, see the tags on this repository.

### Credits
Include a section for credits in order to highlight and link to the authors of your project.

### License
Finally, include a section for the license of your project. For more information on choosing a license, check out GitHub’s licensing guide!