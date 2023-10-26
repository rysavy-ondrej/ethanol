## ContextBuilder Test Data Documentation

### Overview
This document outlines the steps to test the context builder using representative sample data with PostgreSQL.

### Steps

1. **Setting Up the Database Environment**:
   - If you haven't already, set up a local PostgreSQL instance or use Docker to create a PostgreSQL container.

2. **Installing PostgreSQL Command Line Interface (CLI)**:
   - If not already installed, get the PostgreSQL CLI for easy database interaction:
     ```bash
     choco install psql
     ```

3. **Database Connection**:
   - Connect to your PostgreSQL database using the PostgreSQL client. If you're running the database locally or in a Docker container with a port mapped to 1605, you can use:
     ```bash
     psql -U postgres -d ethanol -p 1605
     ```

   - Note: Make sure the user `postgres` has access to the database named `ethanol`.

4. **Data Import**:
   - Copy the enrichment data from the provided CSV files into your PostgreSQL database:
     ```sql
     \COPY enrichment_data FROM smartads.csv WITH DELIMITER ',' CSV HEADER;
     \COPY enrichment_data FROM netify.csv WITH DELIMITER ',' CSV HEADER;
     ```

   - Ensure your current directory in the terminal points to where the CSV files are located.

5. **Running ContextBuilder**:
   - Execute the following command to run ContextBuilder on the given input samples:
     ```bash
     ..\Source\Ethanol.ContextBuilder\bin\Debug\net7.0\Ethanol.ContextBuilder.exe Build-Context -r FlowmonJson:{file=flows.json} -c config-postgres.yaml -w JsonWriter:{file=ctx.json}
     ```

   - This command will use the data from the PostgreSQL database, process it, and generate a context in the `ctx.json` output file.

### Result
After following the steps, you should have a `ctx.json` file in your directory. This file contains the computed context based on the input samples and the enrichment data from the PostgreSQL database. Review this file to ensure the context builder has worked as expected.