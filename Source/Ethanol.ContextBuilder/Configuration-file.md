# Configuration File

Configuration is a YAML-formatted file that is meant for setting up input data processing and data analysis pipelines.
The file has several parameters that control aspects like window size, target IP range, and various data enrichment sources.

The following example demonstrates a common configuration file:

```yaml
window-size: 00:05:0
window-hop: 00:05:00
target-prefix: 192.168.111.0/24
flow-tag-enricher:
    jsonfile:
        filename: webuser.tcp.json
        collection: flows
netify-tag-enricher:
    postgres:
        server: 192.168.111.21
        port: 5432
        database: ethanol
        user: ethanol
        password: ethanol-password
        tableName: netify
host-tag-enricher:
    csvfile:
        filename: webuser.fakesads.csv
```

* `windows-size` - This line specifies the size of the time window for the analysis. This means that the data is captured, analyzed, or aggregated in given intervals.
* `windows-hop` - This line specifies the "hop" or step size for moving the window. This usually means that once one analysis window is completed, the next window starts immediately without any overlap or gap.
* `target-prefix` - This line sets the target IP address range for the analysis using CIDR notation. Context for hosts with IP addresses within the specified range are only computed.
* `flow-tag-enricher` - This section is focused on enriching the data with flow-specific information.
* `netify-tag-enricher` - This section is focused on further enrichment, using data about network applications and IPs as provided by Netify.
* `host-tag-enricher` - This section deals with enrichment based on host information, e.g. sourced from SmartADS.

Each enricher section provides configuration depending on the source. 

## JSON Source

The JSON file source is defined as filename and the name of the collection

```yaml
    jsonfile:
        filename: webuser.tcp.json
        collection: flows
```

The JSON data are expected to be a valid JSON object, which must contain an array of objects: 

```json
{
  "flows" : [
      /* DATA  */
   ]
}
```

## CSV Source
The CSV source is given by the filename of the input CSV file. Its structure depends on the enrichment type.

```yaml
    csvfile:
        filename: webuser.fakesads.csv
```

## Postres Source

The Postgres configuration section specifies the connection details like the server address, port, database name, username, and password. 
Additionally, it specifies a table name within the database that the enricher should interact with.

```yaml
    postgres:
        server: 192.168.111.21
        port: 5432
        database: ethanol
        user: ethanol
        password: ethanol-password
        tableName: netify
```
