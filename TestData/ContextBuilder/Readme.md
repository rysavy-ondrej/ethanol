## ContextBuilder Test Data

To test context builder with the representative sample of data proceed as follows:

1. Create a local or docker-based PostgreSQL database

2. Install PostreSQL CLI:

```
choco install psql
```

3. Connect to the database using PostgreSQL client:

```
psql -U USERNAME -d DATABASE -h HOSTNAME -p PORT
```

For example:
```
psql -U postgres -d ethanol -p 1605
```

4. Copy the enrichment data from CSV files to the database:

```
\COPY your_table_name FROM '/path/to/your/csvfile.csv' WITH DELIMITER ',' CSV HEADER;
```

3. Execute the following command that runs ContextBuilder on the input samples:

```cmd
..\Source\Ethanol.ContextBuilder\bin\Debug\net7.0\Ethanol.ContextBuilder.exe Build-Context -r FlowmonJson:{file=flows.json} -c config-postgres.yaml -w JsonWriter:{file=ctx.json}
```

It creates `ctx.json` output with computed context.
