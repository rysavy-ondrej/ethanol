-- WARNING: When changing this script do not forget to update it also at the following locations:
-- TestData/ContextBuilder/db_init
-- Deploy/Docker/
--
-- The command grants all privileges on the 'ethanol' database to the 'postgres' user.
-- This includes the ability to create, read, update, and delete data as well as manage the database structure.
GRANT ALL PRIVILEGES ON DATABASE ethanol TO postgres;

-- Create a new table named 'enrichment_data' if it doesn't already exist.
-- This table is designed to store additional information (metadata) associated with hosts identified by their IP/MAC addresses.
-- The enrichment data is categorized by a type and key for identification and retrieval purposes.

-- The table includes the following columns:
-- id: An auto-incrementing integer that serves as the primary key.
-- type: A string to define the type of enrichment data.
-- key: A string used as a unique identifier for the enrichment data.
-- value: A string to store the enrichment data value itself.
-- reliability: A floating-point number to indicate the reliability of the enrichment data.
-- validity: A time range (TSRANGE) to represent the period during which the data is considered valid.
-- details: A JSON object to store additional details related to the enrichment data in a structured format.

CREATE TABLE IF NOT EXISTS enrichment_data (
    id SERIAL PRIMARY KEY,  
    type VARCHAR(32) NOT NULL,
    key VARCHAR(64) NOT NULL,
    value VARCHAR(128),
    reliability REAL,
    details JSON,
    validity TSTZRANGE
);

-- Create indexes on the 'key' and 'type' columns of the 'enrichment_data' table if they don't exist.
-- These indexes are intended to speed up queries filtering by 'key' or 'type'.
CREATE INDEX IF NOT EXISTS enrichment_data_key_idx ON enrichment_data (key);
CREATE INDEX IF NOT EXISTS enrichment_data_type_idx ON enrichment_data (type);
CREATE INDEX IF NOT EXISTS enrichment_data_validity_idx ON enrichment_data USING GIST (validity);

-- Create a new table named 'netify_data' similar to 'enrichment_data'.
-- This table is tailored for storing data specific to Netify sources, which also provide enrichment information.
-- The structure of this table and the purpose of its columns are identical to those in the 'enrichment_data' table.

CREATE TABLE IF NOT EXISTS netify_data (
    id SERIAL PRIMARY KEY,  
    type VARCHAR(32) NOT NULL,
    key VARCHAR(64) NOT NULL,
    value VARCHAR(128),
    reliability REAL,
    details JSON,
    validity TSTZRANGE NOT NULL
);

-- Create indexes on the 'key' and 'type' columns of the 'netify_data' table to enhance the performance of queries.
CREATE INDEX IF NOT EXISTS netify_data_key_idx ON netify_data (key);
CREATE INDEX IF NOT EXISTS netify_data_type_idx ON netify_data (type);
CREATE INDEX IF NOT EXISTS netify_data_validity_idx ON netify_data USING GIST (validity);

-- The 'host_context' table is designed to store contextual information about hosts.
-- This table aggregates various types of data that provide a comprehensive context for a host, including tags and connections made or accepted by the host.

-- The table includes the following columns:
-- id: An auto-incrementing integer that serves as the primary key.
-- key: A string that uniquely identifies the host context.
-- tags: A JSON object containing tags related to the host.
-- initiatedconnections: A JSON object with details of connections initiated by the host.
-- acceptedconnections: A JSON object with details of connections accepted by the host.
-- resolveddomains: A JSON object listing domains that have been resolved by the host.
-- weburls: A JSON object with a list of web URLs associated with the host.
-- tlshandshakes: A JSON object detailing TLS handshakes involving the host.
-- validity: A time range indicating the validity period of the context data.

CREATE TABLE IF NOT EXISTS host_context ( 
    key VARCHAR(255) NOT NULL,
    tags JSON,
    connections JSON,
    resolveddomains JSON,
    weburls JSON,
    tlshandshakes JSON,
    validity TSTZRANGE not null
);

-- An index is created on the 'key' column of the 'host_context' table to facilitate quick lookups based on the host identifier.
CREATE INDEX IF NOT EXISTS host_context_key_idx ON host_context (key);

CREATE INDEX IF NOT EXISTS host_context_validity_idx ON host_context USING GIST (validity);
