# Ethanol CLI

Ethanol CLI is a command line interface designed to streamline the running of applications and services associated with Ethanol projects. It enables users to easily start applications, access context objects through an API, and manage configurations via a configuration file.

## Installation

[Include detailed installation instructions here]

## Usage

### General

To use the Ethanol CLI, open a terminal window and navigate to the directory where `ethanol.exe` is located. The general usage pattern is as follows:

```bash
.\ethanol.exe <Command>
```

### Run Builder

The `run-builder` command initializes and starts the application with the necessary configurations provided by the user.

```bash
.\ethanol.exe run-builder [options...]
```

#### Options:

- `-c, --configuration-file <String>`: Specifies the path to the configuration file that will be used to configure the processing. This option is required.

#### Examples:

To start the application with a specific configuration file:

```bash
.\ethanol.exe run-builder -c "C:\path\to\your\config.json"
```

This will launch the builder using settings defined in `config.json`. 
The configuration file for Ethanol CLI is structured in JSON format, which makes it easy to read and write. See the description of [configuration file structure](doc/BuilderConfig.md).
See also an [example](config/context-builder.config.json) of the configuration file.

### Start Service

The `start-service` command spins up the API service, allowing access to context objects, as defined in the provided configuration file.

```bash
.\ethanol.exe start-service [options...]
```

#### Options:

- `-c, --configuration-file <String>`: Specifies the path to the configuration file that will be used to configure the service. This option is required.

#### Examples:

To start the API service with a designated configuration file:

```bash
.\ethanol.exe start-service -c "C:\path\to\service-config.json"
```

This will start the service and the API will be configured according to `service-config.json`. The configuration file has [well-defined structure](doc/BuilderConfig.md). See also an [example](config/context-builder.config.json) of the configuration file.


## Other Commands

### Help

Displays help information about the Ethanol CLI or any specific command. To see a list of available commands:

```bash
.\ethanol.exe help
```

For help with a specific command:

```bash
.\ethanol.exe <Command> help
```

### Version

Shows the current version of Ethanol CLI installed on your machine.

```bash
.\ethanol.exe version
```

## License

Ethanol CLI is released under the [LICENSE](LICENSE) file in the root directory of this source tree.