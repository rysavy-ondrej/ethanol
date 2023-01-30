# Ethanol.ContextBuilder

This tools builds context from IPFIX flows and other optional data sources.

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
  -r, -inputReader <String>       The reader module for processing input stream. (Required)
  -b, -contextBuilder <String>    The builder module to create a context. (Required)
  -w, -outputWriter <String>      The writer module for producing the output. (Required)
```

To use the tool, three modules need to be specified:

* Reader necessary to process conrrectly the input files or stream.
* Builder performing the main task - bulding the context according to the given parameters.
* Writer used to produce the output in the required format and target.

The list of available modules is get by the following command:

```
> Ethanol.ContextBuilder.exe List-Modules
READERS:
  FlowmonJsonReader      reads JSON file with IPFIX data produced by flowmonexp5 tool.
BUILDERS:
  TlsFlowContextBuilder  builds the context for TLS flows in the source IPFIX stream.
  IpHostContextBuilder   builds the context for Ip hosts identified in the source IPFIX stream.
WRITERS:
  JsonDataWriter         writes NDJSON formatted file for computed context.
  YamlDataWriter         writes YAML formatted file for computed context.
```


## Building Flow Context

Building flow context requires to use of the the available flow context builder modules: 

```
> Ethanol.ContextBuilder.exe Build-Context -r FlowmonJsonReader:file=test.flowmon.json,speed=1 -b TlsFlowContextBuilder:window=00:00:10,hop=00:00:05 -w YamlDataWriter:file=flow.context.yaml
```

## Building Host Context

Building host context is done by using host context builder module:

```
> Ethanol.ContextBuilder.exe Build-Context -r FlowmonJsonReader:file=test.flowmon.json -b IpHostContextBuilder:window=00:00:10,hop=00:00:05 -w YamlDataWriter:file=host.contex.yaml
```