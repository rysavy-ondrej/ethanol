DELETE FROM netify_data;

\COPY netify_data(type,key,value,reliability,validity,details) FROM netify.csv WITH DELIMITER ';' CSV HEADER;