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

The Ethanol.ContextBuilder tool offers a suite of commands designed to facilitate context creation, manage flow and host tags, and interface with various data modules. To use the tool, you need to run the Ethanol.ContextBuilder.exe executable followed by the desired command.

Here's a brief overview of the available commands:

- __Build-Context:__ Generates the context for flows, enabling a deeper analysis of network data.

- __Insert-FlowTags:__ Allows for the insertion of flow tags from a given JSON file into an SQL table. This can be useful for categorizing and understanding the nature of specific flows.

- __Insert-HostTags:__ Inserts host-related tags from a JSON file into an SQL table, facilitating a richer understanding of the hosts in your network environment.

- __Insert-Netify:__ An efficient command to update Netify tags in an SQL table. It first removes existing tags and then inserts new ones using data from the provided CSV files.

- __List-Modules:__ Offers a list of available modules within the tool. This is useful for understanding the different functionalities and extensions present.

- __help:__ If you ever need assistance on a specific command or just a general overview, this command will display helpful information.

- __version:__ Use this to quickly check the version of the Ethanol.ContextBuilder tool you're running.

### Build-Context Command

The `Build-Context` command is designed to construct a context for network flows. By creating this context, users can derive a deeper understanding of their network data, identify patterns, and facilitate more informed network-related decisions.

Here's how to use the command and its options:

#### Syntax:

```bash
./Ethanol.ContextBuilder Build-Context -r <input-reader> -c <configuration-file> -w <output-writer>
```

#### Options:

- **-r, --input-reader** `<String>`: Specifies the reader module used for processing the input stream. 


- **-c, --configuration-file** `<String>`: Designates the configuration file that dictates the settings for the processing phase. This file contains various parameters that influence how the data is processed and the context is built.


- **-w, --output-writer** `<String>`: Indicates the writer module employed for generating the output. Once the context for flows is constructed, this module is responsible for formatting and exporting the results.


For successful execution, ensure all three required parameters are provided.

#### Examples:

__Json files I/O__
The following command is invoking the tool to build a context for network flows. It reads input from the webuser.flows.json file using the FlowmonJson reader module, uses the configuration settings from config-postgres.yaml, and then writes the constructed context to the webuser.ctx.json file using the JsonWriter module.

```bash
./Ethanol.ContextBuilder Build-Context -r FlowmonJson:{file=flows.json} -c config-postgres.yaml -w JsonWriter:{file=context.json}
```

Note that parameter `{file=flows.json}` indicates that the input will be read from a file named `flows.json` using the FlowmonJson reader module.
Similarly, parameter means the constructed context will be written to a file named `context.json` using the JsonWriter module.

__Std I/O pipeline__

The following command uses the FlowmonJson module without file argument which means that the module reads input data from the standard input. After processing, the resulting context is output in JSON format directly to the standard output, using the JsonWriter module as no file argument specified. Given the use of standard input and output, this command is versatile and can be used in conjunction with other tools in a pipeline to feed data in or route the resulting output elsewhere.

```bash
./Ethanol.ContextBuilder Build-Context -r FlowmonJson -c config-postgres.yaml -w JsonWriter
```

__Tcp I/O__

This command sets up the tool to listen for incoming FlowmonJson formatted data over TCP on port 1600, process it according to the parameters in the config-postgres.yaml file, and then output the results as JSON to the console.

```bash
./Ethanol.ContextBuilder Build-Context -r FlowmonJson:{tcp=0.0.0.0:1600} -c config-postgres.yaml -w JsonWriter
```

__Direct SQL output__

This command configures the Ethanol.ContextBuilder to capture FlowmonJson formatted data from TCP port 1600, process this data according to the rules in config-postgres.yaml, and store the results in the host_context table of a PostgreSQL database located on localhost.

```bash
./Ethanol.ContextBuilder Build-Context -r FlowmonJson:{tcp=0.0.0.0:1600} -c config-postgres.yaml -w PostgresWriter:{server=localhost,port=5432,database=ethanol,user=postgress,password=postgress,tableName=host_context}
```

## Context Builder Functionality

TODO: describe the way the context is compute in detail.

## Credits

Ethanol is a collaborative project made possible by the following contributors:

- **Authors:**
  - Ondrej Rysavy, Brno University of Technology
  - Martin Holkovic, Flowmon Networks

This project was developed as part of the research initiative titled [Context-based Encrypted Traffic Analysis Using Flow Data](https://www.fit.vut.cz/research/project/1445/.en). This initiative aims to leverage flow data to enhance the analysis of encrypted network traffic, thereby improving the ability to identify and mitigate security threats in encrypted communications.

## License
TBD