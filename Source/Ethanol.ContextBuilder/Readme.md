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
  FlowmonJson     Reads JSON file with IPFIX data produced by flowmonexp5 tool.
  IpfixcolJson    Reads NDJSON exported from ipfixcol2 tool.
  NfdumpCsv       Reads CSV file produced by nfdump.
BUILDERS:
  HostContext     Builds the context for Ip hosts identified in the source IPFIX stream.
  FlowContext     Builds the context for TLS flows in the source IPFIX stream.
WRITERS:
  JsonWriter      Writes NDJSON formatted file for computed context.
  YamlWriter      Writes YAML formatted file for computed context.
```


## Building Flow Context

Building flow context requires to use of the the available flow context builder modules: 

```
> Ethanol.ContextBuilder.exe Build-Context -r FlowmonJson:file=test.flowmon.json,speed=1 -b FlowContext:window=00:00:10,hop=00:00:05 -w YamlWriter:file=flow.context.yaml
```

## Building Host Context

Building host context is done by using host context builder module:

```
> Ethanol.ContextBuilder.exe Build-Context -r FlowmonJson:file=test.flowmon.json -b HostContext:window=00:00:10,hop=00:00:05 -w YamlWriter:file=host.contex.yaml
```