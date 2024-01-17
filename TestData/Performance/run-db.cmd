docker run --name ethanol-postgres -e POSTGRES_DB=ethanol -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 1605:5432 -d postgres

timeout /t 10

set PGPASSWORD=postgres

psql -U postgres -d ethanol -p 1605 -f ..\ContextBuilder\db_init\ethanol-db-init.sql

