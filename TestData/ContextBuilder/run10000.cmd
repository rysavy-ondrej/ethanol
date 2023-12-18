REM Description: Run 10000 times insert-bige.sql
REM              This script is used to populate the database with a lot of records.
set PGPASSWORD=postgres

@echo off
FOR /L %%G IN (1,1,1000) DO (
    psql -U postgres -d ethanol -p 1605 -f insert-bige.sql
)


