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
  -r, --input-reader <String>        The reader module for processing input stream. (Required)
  -b, --context-builder <String>     The builder module to create a context. (Required)
  -e, --context-enricher <String>    The enricher module to extend a context with additional information. (Required)
  -w, --output-writer <String>       The writer module for producing the output. (Required)
```

To use the tool, three modules need to be specified:

* Reader necessary to process conrrectly the input files or stream.
* Builder performing the main task - bulding the context according to the given parameters.
* Writer used to produce the output in the required format and target.

The list of available modules is get by the following command:

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


## Building Host Context

To building host context use the following command:

```
> Ethanol.ContextBuilder.exe Build-Context -r NfdumpCsv:{file=test.nfdump.csv} -b IpHostContext:{window=00:05:00,hop=00:05:00} -e VoidContextEnricher -w YamlWriter:{file=iphost-csv.contex.yaml}
```

To building host context enriched from Postgres stored metadata use the following command:

```
> Ethanol.ContextBuilder.exe Build-Context -r NfdumpCsv:{file=test.nfdump.csv} -b IpHostContext:{window=00:05:00,hop=00:05:00} -e VoidContextEnricher -w YamlWriter:{file=iphost-csv.contex.yaml}
```