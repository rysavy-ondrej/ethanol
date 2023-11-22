set PGPASSWORD=postgres

@echo off
FOR /L %%G IN (1,1,1000) DO (
    psql -U postgres -d ethanol -p 1605 -f insert-bige.sql
)


