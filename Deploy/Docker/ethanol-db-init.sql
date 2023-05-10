-- Database seems to exist
-- CREATE DATABASE ethanol;
-- GRANT ALL PRIVILEGES ON DATABASE ethanol TO postgres;

-------------------------------------------------------------------------------
-- Flow Tags table enables to annotate flows with their process names.
-------------------------------------------------------------------------------
CREATE TABLE _flowtags (
    tag TEXT,
    time TIMESTAMP WITHOUT TIME ZONE,
    data JSONB
);
CREATE TABLE flowtags (
    LocalAddress VARCHAR(32),
    LocalPort INTEGER,
    RemoteAddress VARCHAR(32),
    RemotePort INTEGER,
    ProcessName VARCHAR(128),
    Validity TSRANGE
);
CREATE FUNCTION flowtags_transform_and_insert()
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
    remote_port := CAST(NEW.data->>'RemotePort'  AS INTEGER);
    INSERT INTO flowtags (LocalAddress, LocalPort, RemoteAddress, RemotePort, ProcessName,Validity)
    VALUES (NEW.data->>'LocalAddress', local_port, NEW.data->>'RemoteAddress', remote_port, NEW.data->>'ProcessName', time_range);
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER insert_trigger
BEFORE INSERT ON _flowtags
FOR EACH ROW
EXECUTE FUNCTION flowtags_transform_and_insert();

-------------------------------------------------------------------------------
-- Host context table contains resulted context computed by 
-- the context builder.
-------------------------------------------------------------------------------
CREATE TABLE _hostctx (
    tag TEXT,
    time TIMESTAMP WITHOUT TIME ZONE,
    data JSONB
);

CREATE TABLE  hostctx (
    HostAddress VARCHAR(32),
    OperatingSystem VARCHAR(64),
    InitiatedConnections JSONB,
    AcceptedConnections JSONB,
    ResolvedDomains JSONB,
    WebUrls JSONB,
    TlsHandshakes JSONB,
    Validity TSRANGE
);

CREATE FUNCTION hostctx_transform_and_insert()
RETURNS TRIGGER AS $$
DECLARE
    time_range tsrange;
    start_time timestamp;
    end_time timestamp;
BEGIN
    start_time := NEW.data->>'StartTime';
    end_time := NEW.data->>'EndTime';
    time_range := tsrange(start_time, end_time, '[)');
    INSERT INTO hostctx (HostAddress, OperatingSystem, InitiatedConnections, AcceptedConnections, ResolvedDomains,WebUrls,TlsHandshakes,Validity)
    VALUES (NEW.data#>>'{Payload,HostAddress}', NEW.data#>>'{Payload,OperatingSystem}', 
            NEW.data#>'{Payload,InitiatedConnections}', NEW.data#>'{Payload,AcceptedConnections}',
            NEW.data#>'{Payload,ResolvedDomains}', NEW.data#>'{Payload,WebUrls}',
            NEW.data#>'{Payload,TlsHandshakes}', time_range);
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;


CREATE TRIGGER insert_trigger
BEFORE INSERT ON _hostctx
FOR EACH ROW
EXECUTE FUNCTION hostctx_transform_and_insert();

-------------------------------------------------------------------------------
-- Netify tables support annotation of flows with known web application based 
-- on Netify provided information.
-------------------------------------------------------------------------------
CREATE TABLE netify_applications (
    id INT PRIMARY KEY,
    tag VARCHAR(100),
    short_name VARCHAR(50),
    full_name VARCHAR(100),
    description TEXT,
    url VARCHAR(255),
    category VARCHAR(50)
);

CREATE TABLE netify_addresses (
    id INT PRIMARY KEY,
    value VARCHAR(50),
    ip_version INT,
    shared INT,
    app_id INT,
    platform_id INT,
    asn_tag VARCHAR(20),
    asn_label VARCHAR(128),
    asn_route VARCHAR(50),
    asn_entity_id INT
);

CREATE INDEX ips_value_idx ON netify_addresses (value);
CREATE INDEX ips_app_id_idx ON netify_addresses (app_id);

-------------------------------------------------------------------------------
-- Smart ADS table enables to annotate host identifies by 
-- their IP/MAC addresses with extra information depending on the tag type.
-------------------------------------------------------------------------------
CREATE TABLE hosttags (
    KeyType VARCHAR(8),
    KeyValue VARCHAR(32),
    Source VARCHAR(40),
    Reliability REAL,
    Module VARCHAR(40),
    Data JSON,
    Validity TSRANGE
);

CREATE INDEX hosttags_source_idx ON hosttags (Source);