# PostgreSQL

## Temporal Data and Queries
PostgreSQL supports temporal tables through the use of extensions. A temporal table is a table that contains a "time period" column that specifies the time period for which a row is valid. The time period can be specified using a range data type, which consists of a start timestamp and an end timestamp. The temporal tables extension provides a set of functions and operators for querying temporal data, including functions for temporal joins and aggregations. The special datatype TSRANGE is used to define the validity of data.

We create a new database ethanol for storing all the data tables in it. Then to create a table with Smart ADS tags use the following steps:

```sql
CREATE DATABASE ethanol;

\c ethanol

CREATE TABLE smartads (
  KeyType VARCHAR(20),
  KeyValue VARCHAR(80),
  Source VARCHAR(40),
  Validity TSRANGE,
  Reliability REAL,
  Module VARCHAR(40),
  Data TEXT
);
```

Then to insert data in the table it is required to define its validity:

```sql
INSERT INTO smartads VALUES ('ip','147.229.176.80','os_by_tcpip','[2021-10-16 16:59:25.755,2021-11-09T03:35:58.783846)',1.0,'os_by_tcpip@collector-enta','"Windows"');
```

Finally, the query can filter the time period of data:

```sql
SELECT * FROM smartads WHERE KeyValue = '147.229.176.80' AND Validity @> '[2021-10-16 17:00:00,2021-10-16 17:05:00)';
```

## PostgreSQL Docker

Pull the PostgreSQL image: Open a terminal window and run the following command to pull the official PostgreSQL image from Docker Hub:
docker pull postgres
This will download the latest version of PostgreSQL image to your machine.

Run the PostgreSQL container: Run the following command to start a new PostgreSQL container:

```bash
docker run --name postgres-ethanol -e POSTGRES_PASSWORD=ethanol-password -d -p 5432:5432 postgres
```

This command will start a new container named postgres-ethanol, set the password for the postgres user to ethanol-password, and map the container's port 5432 to your local machine's port 5432.

Connect to the PostgreSQL server: You can connect to the PostgreSQL server using a PostgreSQL client like psql. To connect using psql, run the following command:

```bash
docker exec -it postgres-ethanol psql -U postgres -d ethanol
```

To copy file from the host to the container use the following command:

```bash
docker cp FILE_ON_HOST postgres-ethanol:/data/
```

## Loading Data

The CSV files with header thus must have the following format:

```csv
KeyType,KeyValue,Source,ValidFrom, ValidTo,Reliability,Module,Data
ip,147.229.176.80,os_by_tcpip,2021-10-16 16:59:25.755,2021-11-09T03:35:58.783846,1.0,os_by_tcpip@collector-enta,"Windows"
```

Unfortunately, the CSV file is not directly loadable to the database. First, we need to provide a valid time range. Second, the Data column contains JSON string, which need to be properly quoted. Thus, we prepare the data using Python's CSVKIT:
https://csvkit.readthedocs.io/en/latest/index.html

To prepare the data we run the following commands:

```bash
csvclean meta.csv

csvsql --query "select KeyType,KeyValue,Source,'[' || ValidFrom || ',' || ValidTo || ']' as Validity,Reliability,Module,Data from 'meta100'" meta-good.csv > import.csv
```

Now, import.csv contains data suitable for load to the database:
Use the following SQL command to copy the data from the CSV file into the table:

```sql
COPY smartads FROM 'path/to/csv/file' DELIMITER ',' CSV HEADER;
```

Replace path/to/csv/file with the path to your CSV file. The DELIMITER ',' option specifies that the CSV file uses commas as the field delimiter, and the CSV HEADER option specifies that the first line of the CSV file contains the column headers.