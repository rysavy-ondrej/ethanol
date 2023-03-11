# FlowSonar Host Configuration

This host consumes monitoring data and performs the processing.

To read source data, the Fluent-Bit is used. The installation is by follow the instructions:
<https://docs.fluentbit.io/manual/installation/linux/redhat-centos#install-on-redhat-centos>

Upon installation the tool can be executed as follows:

```bash
/opt/fluent-bit/bin/fluent-bit -c fluent-bit.conf
```

The Fluent Bit configuration specifies an input and output plugin:

* The first input plugin listens on TCP port 5170 and reads incoming flowmonexp5 data.
* The second input plugin listens on TCP port 5175 and expects flowtag produced by collect-tcp script on the sandbox host.
* The output plugin formats the incoming data as JSON lines and writes them to the standard output. It uses the "FLOWMON_EXPORT_TIME" field as the date key in the JSON format and specifies the date format as "java_sql_timestamp".

Then the data can be either stored in the file:

```bash
/opt/fluent-bit/bin/fluent-bit -c fluent-bit.conf > FLOW-FILE.ndjson
```

It is also possible to pipe it to the next tool, for instance, Ethanol Context Builder:

```bash
/opt/fluent-bit/bin/fluent-bit -c fluent-bit.conf | Ethnol.ContextBulder -i Build-Context -r FlowmonJson:"{file=stdin}" -c context-builder.conf -w JsonWriter:"{file=stdout}"
```

## Fluent-Bit configuration files

Two predefined configuration files are provided:

* `fluent-bit.std.conf` -- this file forwards the recived flowmonexp data to standard output.
* `fluent-bit.std+file.conf` -- this file forwards the recived flowmonexp data to standard output and also stores them to the file.