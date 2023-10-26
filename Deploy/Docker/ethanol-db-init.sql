-- Create a fresh database
GRANT ALL PRIVILEGES ON DATABASE ethanol TO postgres;

-------------------------------------------------------------------------------
-- Smart ADS and other enrichment data sources enable to annotate host identified by 
-- their IP/MAC addresses with extra information depending on the tag type.
-------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS enrichment_data (
    type VARCHAR(32) NOT NULL,
    key VARCHAR(64) NOT NULL,
    value VARCHAR(128),
    reliability REAL,
    validity TSRANGE,
    details JSON
);

CREATE INDEX IF NOT EXISTS enrichment_data_key_idx ON enrichment_data (key);

-------------------------------------------------------------------------------
-- Host context table contains resulted context computed by 
-- the context builder.
-------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS _host_context (
    tag TEXT,
    time TIMESTAMP WITHOUT TIME ZONE,
    data JSONB
);

CREATE TABLE IF NOT EXISTS host_context (    
    id SERIAL PRIMARY KEY, 
    key VARCHAR(255) NOT NULL,
    tags JSON,
    initiatedconnections JSON,
    acceptedconnections JSON,
    resolveddomains JSON,
    weburls JSON,
    tlshandshakes JSON,
    validity TSRANGE
);

CREATE INDEX IF NOT EXISTS host_context_key_idx ON host_context (key);

CREATE OR REPLACE FUNCTION host_context_transform_and_insert()
RETURNS TRIGGER AS $$
DECLARE
    time_range tsrange;
    start_time timestamp;
    end_time timestamp;
BEGIN
    start_time := NEW.data->>'StartTime';
    end_time := NEW.data->>'EndTime';
    time_range := tsrange(start_time, end_time, '[)');
    INSERT INTO hostctx (key, tags, initiatedconnections, acceptedconnections, resolveddomains,weburls,tlshandshakes,validity)
    VALUES (NEW.data#>>'{Payload,Key}', 
            NEW.data#>'{Payload,Tags}', 
            NEW.data#>'{Payload,InitiatedConnections}', 
            NEW.data#>'{Payload,AcceptedConnections}',
            NEW.data#>'{Payload,ResolvedDomains}', 
            NEW.data#>'{Payload,WebUrls}',
            NEW.data#>'{Payload,TlsHandshakes}', 
            time_range);
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER insert_trigger
BEFORE INSERT ON _host_context
FOR EACH ROW
EXECUTE FUNCTION host_context_transform_and_insert();