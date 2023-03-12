# FlowSonar Host Configuration

This host consumes monitoring data and performs the processing.

It runs Fluent-Bit for consuming source data and ProstreSQL for storing tags.


```bash
/opt/fluent-bit/bin/fluent-bit -c fluent-bit.conf | ~/Application/Ethanol/

```




## Fluent-Bit

To read source data, the Fluent-Bit is used. The installation is by follow the instructions:
<https://docs.fluentbit.io/manual/installation/linux/redhat-centos#install-on-redhat-centos>

Upon installation the tool can be executed as follows:

```bash
/opt/fluent-bit/bin/fluent-bit -c fluent-bit.conf
```

The Fluent Bit configuration specifies an input and output plugin:

* The first input plugin listens on TCP port 5170 and reads incoming flowmonexp5 data.
* The second input plugin listens on TCP port 5175 and expects flowtag produced by collect-tcp script on the sandbox host.
* The first output plugin formats the incoming data as JSON lines and writes them to the standard output. 
* The second plugin pumps the data to PostgreSQL database (see bellow).

Then the data can be either stored in the file:

```bash
/opt/fluent-bit/bin/fluent-bit -c fluent-bit.conf > FLOW-FILE.ndjson
```

It is also possible to pipe it to the next tool, for instance, Ethanol Context Builder:

```bash
/opt/fluent-bit/bin/fluent-bit -c fluent-bit.conf | Ethnol.ContextBulder -i Build-Context -r FlowmonJson:"{file=stdin}" -c context-builder.conf -w JsonWriter:"{file=stdout}"
```

## PostgreSQL

### Installing database

You can install PostgreSQL on CentOS 9 by following these steps:

First, update the package repository index:

```bash
sudo dnf update
```

Install the PostgreSQL server and client packages:

```bash
sudo dnf install postgresql postgresql-server
```

Initialize the PostgreSQL database:

```bash
sudo /usr/bin/postgresql-setup --initdb
```

Start the PostgreSQL service:

```bash
sudo systemctl start postgresql
```

Enable the PostgreSQL service to start automatically on system boot:

```bash
sudo systemctl enable postgresql
```

Configure the firewall to allow remote connections to the PostgreSQL server:

```bash
sudo firewall-cmd --add-service=postgresql --permanent
sudo firewall-cmd --reload
```

Check the status of the database:

```bash
sudo systemctl status postgresql
```

### Initializing database

Create necessary database configuration for Ethanol system.

```bash
sudo su - postgres
psql
```

```sql
postgres=# CREATE USER ethanol WITH PASSWORD 'ethanol-password';
postgres=# CREATE DATABASE ethanol;
postgres=# GRANT ALL PRIVILEGES ON DATABASE ethanol TO ethanol;
SHOW hba_file;
\q
```

The last command shows the location of the configuration file (`/var/lib/pgsql/data/pg_hba.conf`).
Here it is necessary to modify the way the password is verified. Change peer to md5:

```txt
local  all      all          md5
```

More info is at:
<https://gist.github.com/AtulKsol/4470d377b448e56468baef85af7fd614>

Then it is possible to open `psql` and create support for ingesting data from Fluent-Bit:

```bash
psql -U ethanol -d ethanol
```

### Enable Network Access
1 Open the PostgreSQL configuration file `postgresql.conf`. By default, it is located in the data directory of the PostgreSQL installation. To find the location of the postgresql.conf file, you can run the following SQL command in a PostgreSQL session:

```sql
SHOW config_file;
```

2. Locate the listen_addresses parameter in the configuration file. By default, it is commented out. Uncomment the line and set the value to * to allow incoming connections from any IP address.

```txt
listen_addresses
```

3. Save and close the configuration file.

4. Open the PostgreSQL `pg_hba.conf` file, which is also located in the data directory.

5. Add a new line to the file to allow incoming connections from the network segment. The line should specify the IP address range that is allowed to connect, the authentication method, and the database that the connection can access.

```txt
host    all  all   192.168.111.0/24   md5
```

### Creating Tables for FlowTags

Two table are required: One for raw data ingested by Fluent-Bit and the target table
filled by BEFORE INSERT trigger.

This table is used by the output module `pgsql` to store ingested data for further processing or analysis.

```sql
CREATE TABLE _flowtags (
    tag TEXT,
    time TIMESTAMP WITHOUT TIME ZONE,
    data JSONB
);
```

The table has the follwing columns:

* tag - A text column for storing a tag associated with the data being ingested.
* time - A timestamp column without timezone, indicating the time at which the data was ingested.
* data - A JSONB column for storing the data being ingested

However, the Ethanol Context Builder requires different table:

```sql
CREATE TABLE flowtags (
    LocalAddress VARCHAR(32),
    LocalPort INTEGER,
    RemoteAddress VARCHAR(32),
    RemotePort INTEGER,
    ProcessName VARCHAR(128),
    Validity TSRANGE
);
```

The way of providing the necessary data for the Ethanol is to implement a trigger that executes transformation function:

```sql
CREATE OR REPLACE FUNCTION transform_and_insert()
RETURNS TRIGGER AS $$
DECLARE
    time_range tsrange;
    start_time timestamp;
    end_time timestamp;
    local_port integer;
    remote_port integer;
BEGIN
    start_time := NEW.data->>'StartTime';
    end_time := NEW.data->>'EndTime';
    time_range := tsrange(start_time, end_time, '[)');
    local_port := CAST(NEW.data->>'LocalPort'  AS INTEGER);
    remote_port := CAST(NEW.data->>'LocalPort'  AS INTEGER);
    INSERT INTO flowtags (LocalAddress, LocalPort, RemoteAddress, RemotePort, ProcessName,Validity)
    VALUES (NEW.data->>'LocalAddress', local_port, NEW.data->>'RemoteAddress', remote_port, NEW.data->>'ProcessName', time_range);
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER insert_trigger
BEFORE INSERT ON _flowtags
FOR EACH ROW
EXECUTE FUNCTION transform_and_insert();
```

The above PL/pgSQL block is creating a function named transform_and_insert(). This function takes no input parameters and returns a trigger. The trigger is associated with the flowtag_ingest table and will execute before an insert is performed on the table for each row being inserted.

The trigger calls the transform_and_insert() function to process the data before it is inserted into the flowtag_ingest table. Inside the function, it transforms the data to create a tsrange value using the StartTime and EndTime fields from the data column in the NEW row. Then it inserts the data from the data column into the flowtags table with LocalAddress, LocalPort, RemoteAddress, RemotePort, ProcessName, and Validity columns.

Finally, it returns a NULL value, indicating that the trigger has completed its task and that the no data will be insterted in _flowtags table.

### Creating Tables for Host Tags

```sql
CREATE TABLE smartads (
    KeyType VARCHAR(8),
    KeyValue VARCHAR(32),
    Source VARCHAR(40),
    Reliability REAL,
    Module VARCHAR(40),
    Data JSON,
    Validity TSRANGE
);
```