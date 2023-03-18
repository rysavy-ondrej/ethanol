#-------------------------------------------------------------------------------
# A Docker file for Ethanol.ContextBuilder tool.
#-------------------------------------------------------------------------------
# To build the image:
#   $ docker build -t ethanol .
#
# To run the application:
#   $ docker run -p 6363:6363 ethanol  "Build-Context", "-r", "FlowmonJson:{tcp=127.0.0.1:6363}", "-c", "config.yaml", "-w", "JsonWriter:{tcp=192.168.1.21:8888}"]
#
#-------------------------------------------------------------------------------
# Use Microsoft's official .NET 7 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory to /app
WORKDIR /app


# Copy the source code and build the application
COPY ./Source/ ./

RUN dotnet restore Ethanol.ContextBuilder/

RUN dotnet publish -c Release -o bin -r linux-x64 --self-contained true /p:PublishTrimmed=true Ethanol.ContextBuilder/

# Build the runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0 AS runtime

# Set the working directory to /app and copy the published output
WORKDIR /app
COPY --from=build /app/bin ./

# Set the entry point for the container
ENTRYPOINT ["dotnet","./Ethanol.ContextBuilder.dll"] 

