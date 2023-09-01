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
