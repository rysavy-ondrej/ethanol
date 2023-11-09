## ContextBuilder Test Data Documentation

### Overview
This document outlines the steps to test the context builder using representative sample data with PostgreSQL.

### Steps

1. **Setting Up the Database Environment**:
   - If you haven't already, set up a local PostgreSQL instance or use Docker to create a PostgreSQL container. You can use provided `dockerfile` to create 
     a new PostgreSQL container.  

     ```bash
     docker run --name ethanol-postgres -e POSTGRES_DB=ethanol -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 1605:5432 -d postgres
     ```

2. **Installing PostgreSQL Command Line Interface (CLI)**:
   - If not already installed, get the PostgreSQL CLI for easy database interaction:
   
     ```bash
     choco install psql
     ```

3. **Database Connection**:
   - Connect to your PostgreSQL database using the PostgreSQL client and create tables. If you're running the database locally or in a Docker container with a port mapped to 1605, you can use:

     ```bash
     psql -U postgres -d ethanol -p 1605 -f db_init/ethanol-db-init.sql
     ```

   - Note: Make sure the user `postgres` has access to the database named `ethanol`.



4. **Data Import**:
   - Copy the enrichment data from the provided CSV files into your PostgreSQL database:

     ```sql
     psql -U postgres -d ethanol -p 1605
     
     \COPY enrichment_data(type,key,value,reliability,validity,details) FROM smartads.csv WITH DELIMITER ',' CSV HEADER;
     \COPY netify_data(type,key,value,reliability,validity,details) FROM netify.csv WITH DELIMITER ',' CSV HEADER;
     ```

   - Ensure your current directory points to where the CSV files are located.

5. **Running ContextBuilder**:
   - Execute the following command to run ContextBuilder on the given input samples:

     ```bash
     ..\Source\Ethanol.Cli\bin\Debug\net7.0\ethanol.exe run-builder -c context-builder.config.json < flows.json > ctx.json
     ```

   - This command will use the data from the PostgreSQL database, process it, and generate a context in the `ctx.json` output file.

### Result
After following the steps, you should have a `ctx.json` file in your directory. This file contains the computed context based on the input samples and the enrichment data from the PostgreSQL database. Review this file to ensure the context builder has worked as expected.