# Ethanol CLI Documentation

Ethanol CLI is a command-line interface that simplifies the operation of applications and services associated with Ethanol projects. It allows users to easily start applications, access context objects through APIs, and manage configurations.

## Installation

Ethanol CLI is distributed as standalone executable files for different platforms, available in the [Deploy folder](../../Deploy/Bin). Use the appropriate build script for your target OS:

| Operating System | Build Script |
|------------------|--------------|
| Windows OS       | [build-executable.cmd](../build-executable.cmd) |
| Linux OS         | [build-executable.sh](../build-executable.sh)   |

## Usage Instructions

### General Usage

Navigate to the directory containing `ethanol.exe` and execute commands using the following pattern:

```bash
ethanol <Command> [options...]

Commands:
  builder exec                          Executes the context builder that reads data on stdin and produces contexts to stdout.
  builder run                           Runs the context builder command according to the configuration file.
  malware infect                        Modify the input context by 'infecting' it with the malware from malware reports in the given folder.
  malware learn                         Learn malware models from specified malware reports.
  malware scan                          Tests the specified context for the presence of malware indicators.
  service start                         Starts the API for accesing the context objects.
  tags insert-netify                    Inserts netify tags from the provided CSV files into the specified table in an SQL database.
  help                                  Display help.
  version                               Display version.
```

### Context Builder Commands

#### Run Builder

Initializes and starts the application with user-provided configurations.

| Option | Description |
|--------|-------------|
| `-c, --configuration-file <String>` | Path to the configuration file (Required) |

**Example:** Start the application with a specific configuration file:

```bash
ethanol builder run -c "C:\path\to\your\config.json"
```

#### Exec Builder

Executes the context builder, reading input NDJSON with flow records and producing the context output.

**Example:** Execute with a specific configuration file, processing standard input:

```bash
cat INPUT-FLOWS.ndjson | ethanol builder exec > OUTPUT-CTX.ndjson
```

### Context Provider Commands

#### Start Service

Launches the API service, enabling access to context objects.

| Option | Description |
|--------|-------------|
| `-c, --configuration-file <String>` | Path to the configuration file (Required) |

**Example:** Start the API service with a designated configuration file:

```bash
ethanol service start -c "C:\path\to\service-config.json"
```

### Malware Sonar Commands

#### Scan Malware

Analyzes the specified context for the presence of malware indicators.

| Option | Description |
|--------|-------------|
| `-p, --path-malware-profile-file <String>` | Path to the malware profile file (Required) |
| `-i, --input-path <String>` | Path to the input folder or file (Required) |
| `-o, --output-file-path <String>` | Path for the output JSON report (Required) |
| `-t, --threshold-score <Double>` | Threshold score for malware presence (Optional, Default: 1.0) |

**Example:** Execute with a specific configuration file, processing standard input:

```bash
ethanol malware scan -p .\Models\lightblue.mal -i .\ContextData\ctx-bening1.json -o .\TestResults\ctx-bening1.tested.json
```

#### Learn Malware

Learns malware models from specified malware reports.

| Option | Description |
|--------|-------------|
| `-r, --root-report-folder <String>` | Path to the root folder with malware reports (Required) |
| `-o, --output-profile-path <String>` | Path for the output malware profiles (Required) |

**Example:** Execute with a specific configuration file, processing standard input:

```bash
ethanol malware learn -r Train -o Models/profile.mal
```

### Other Commands

#### Help

Displays help information about CLI commands and specific commands.

**Example:** Display general help or help for a specific command:

```bash
ethanol help
ethanol <Command> help
```

#### Version

Shows the current version of the Ethanol CLI.

**Example:** Check the installed version of Ethanol CLI:

```bash
ethanol version
```

## Licensing

Ethanol CLI is released under the [LICENSE](LICENSE) included in the source tree's root directory.