#-------------------------------------------------------------------------------
# A Docker file for Ethanol.ContextBuilder tool.
#-------------------------------------------------------------------------------
# To build the image:
#
#   $ docker build -t ethanol .
#
# ..or directly from GitHub:
#
#   $ docker build -t ethanol https://github.com/rysavy-ondrej/ethanol
#
# To run the application, replace TARGET-HOST with valid Tcp server connection string, e.g., 192.168.1.21:6364, and execute:
#
#   $ docker run -p 6363:6363 ethanol Build-Context -r FlowmonJson:{tcp=0.0.0.0:6363} -c default-config.yaml -w JsonWriter:{tcp=TARGET-HOST}
#
# The file can be also used from docker-compose:
# In this case the following arguments shold be specify to provide information on other service points.
#-------------------------------------------------------------------------------

# Use Microsoft's official .NET 7 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory for compilation of the application to /src
WORKDIR /src

# Copy the source code and build the application
COPY ./Source/ .

# Compile and publish:
RUN dotnet publish -c Release -o /bin -r linux-x64 --self-contained true /p:PublishReadyToRun=true /p:PublishSingleFile=true Ethanol.ContextBuilder/

# Build the runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0 AS runtime

# Set the working directory to /app and copy the published output
WORKDIR /app
COPY --from=build /bin ./
# COPY ./Source/Ethanol.ContextBuilder/Configurations ./
COPY ./Deploy/Docker/ethanol-config.yml ./

# Set the entry point for the container
# This runs the builder with the following i/o bindings:
# input: Tcp server listens for incoming JSON formated data with input flows
# output: PostreSQL table hostcontext is inserted with generated context

CMD sh -c './Ethanol.ContextBuilder Build-Context -r FlowmonJson:{tcp=0.0.0.0:${ETHANOL_PORT}} -c ethanol-config.yml -w PostgresWriter:{server=${POSTGRES_IP},port=${POSTGRES_PORT},database=ethanol,user=${POSTGRES_USER},password=${POSTGRES_PASSWORD},tableName=host_context}'