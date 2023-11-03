cd /app/builder
./Ethanol.ContextBuilder run -r FlowmonJson:{tcp=0.0.0.0:${ETHANOL_PORT}} -c ethanol-config.yml -w "PostgresWriter:{server=${POSTGRES_IP},port=${POSTGRES_PORT},database=${POSTGRES_DATABASE},user=${POSTGRES_USER},password=${POSTGRES_PASSWORD},tableName=host_context}"
