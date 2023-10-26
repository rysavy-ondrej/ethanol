\COPY enrichment_data FROM smartads.csv WITH DELIMITER ',' CSV HEADER;
\COPY enrichment_data FROM netify.csv WITH DELIMITER ',' CSV HEADER;