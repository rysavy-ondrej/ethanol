## Context Builder Configuraton

The configuration file for Ethanol CLI is structured in JSON format, which makes it easy to read and write.
A typical context builder configuration file can look like follows:

```json
{
	"input": {
		"tcp": {
			"listen": "0.0.0.0",
			"port": "5180",
			"format": "flowmon-json"
		},
		"stdin": {
			"format": "flowmon-json"
		}
	},

	"builder": {
		"windowsize": "00:05:00",
		"windowhop": "00:05:00",
		"networks": [ "192.168.0.0/16", "172.16.0.0/16", "10.0.0.0/8", "147.229.0.0/16" ]
	},

	"enricher": {
		"postgres": {
			"server": "${POSTGRES_IP}",
			"port": "${POSTGRES_PORT}",
			"database": "${POSTGRES_DATABASE}",
			"user": "${POSTGRES_USER}",
			"password": "${POSTGRES_PASSWORD}",
			"tablename": "netify_data"
		}
	},

	"output": {
		"postgres": {
			"server": "${POSTGRES_IP}",
			"port": "${POSTGRES_PORT}",
			"database": "${POSTGRES_DATABASE}",
			"user": "${POSTGRES_USER}",
			"password": "${POSTGRES_PASSWORD}",
			"tablename": "host_context"
		},
		"stdout": {
			"format": "json"
		}
	}
}

```

It is divided into several sections, each specifying different aspects of the application's behavior:

- `input`: This section defines the sources from which Ethanol will read data. There are two types of input sources. At least one must be configured. If both are provided, it is possible to read input data in parallel from both inputs.
  - `tcp`: This specifies that Ethanol should listen on a TCP port for incoming data. The `listen` key under `tcp` is the address on which to listen, `port` is the TCP port to listen on, and `format` specifies the expected format of the incoming data.
  - `stdin`: This indicates that Ethanol can also read data from the standard input (`stdin`). The `format` specifies the data format expected from `stdin`.

- `builder`: This section configures the builder component of Ethanol.
  - `windowsize`: Defines the time window size for processing data. The format "00:05:00" might represent a duration of 5 minutes.
  - `windowhop`: Specifies the frequency with which the window moves forward (similar to a sliding window mechanism). In this case, it's also set to 5 minutes.
  - `networks`: An array of network CIDR blocks that Ethanol should consider for processing. These can, for instance, represent the internal networks that are relevant to the application's context.

- `enricher`: This section is about data enrichment, where additional data is added to the existing data set.
  - `postgres`: Specifies a PostgreSQL database connection for enriching the data. It includes keys for server address, port, database name, user, and password (the latter four are placeholders that would be replaced by environment variables or actual values). The `tablename` is the specific table where the enriched data will be stored or queried from.

- `output`: This section details how and where the processed data should be outputted. Multiple outputs are possible as the sink of the data. All produced data are then outputted to configured outputs.
  - `postgres`: Similar to the `enricher` section's `postgres`, this specifies the details for a PostgreSQL database where the final output should be written. The `tablename` suggests that the processed data will be stored in a table named `host_context`.
  - `stdout`: Indicates that the output should also be written to the standard output (`stdout`) in JSON format.

The usage of placeholders like `${POSTGRES_IP}` within the configuration file enables  to use the actual values for these variables from the environment. This approach enables the configuration file to be used in different environments with different settings without changing the file itself, enhancing portability and security (especially for sensitive information like passwords).