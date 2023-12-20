# Ethanol Deployment in Production

This document outlines the method for deploying Docker-based applications. It assumes that Ethanol image was already push in a Docker Hub (https://hub.docker.com/r/rysavyondrej/ethanol).

## Prerequisites

Before beginning, ensure the following requirements are met:

1. **Ethanol Docker Image**: The Ethanol docker image should be created and pushed to the repository.

2. **Docker Environment**: A Docker environment with `docker-compose` support must be installed and running. This environment is referred to as the *ethanol* machine. It can be, for example, a standalone container host, such as [PhotonOS](https://vmware.github.io/photon/).

3. **Enabling Docker and Docker-Compose on PhotonOS**:
   - Docker is preinstalled on PhotonOS. However, `docker-compose` must be installed manually using the following command:  
     ```bash
     tdnf install docker-compose
     ```
   - Start the Docker service with:
     ```bash
     systemctl start docker
     ```
   - To ensure the Docker daemon service automatically starts on every VM reboot, use:
     ```bash
     systemctl enable docker
     ```

## Deployment Steps

Follow these steps to deploy your application:

1. **Prepare the Ethanol Machine**:
   - Transfer `docker-compose.devel.yml` and `ethanol-db-init.sql` files to the *ethanol* machine.
   - Update the permissions of the SQL configuration file to make it readable by all:
     
     ```bash
     chmod a+r ethanol-db-init.sql
     ```

2. **Execute Docker Compose**:
   - Run the following command to start the deployment process:
     
     ```bash
     docker-compose -f docker-compose.prod.yml up
     ```

   - This command fetches the latest versions of the containers and runs the entire environment.

3. **Post-Deployment Operations**:
   After successful deployment, you can:
   - Send Flowmon-JSON formatted flow data to the *ethanol* machine on port 1600 via a TCP connection.
   - Connect to the Postgres database running at *ethanol*:1605.
   - Access data via the Ethanol REST API at *ethanol*:1610.

## Export Netflows from flowmonexp5

The configuration for the IPFIX export using the Flowmon exporter is specified in the [probe-ethanol.json](probe-ethanol.json) configuration file.
This file should be uploaded to the `~/ethanol` folder on the Flowmon host. To generate IPFIX records in the required JSON format, run the following command on the Flowmon host:

```bash
sudo flowmonexp5 ~/ethanol/probe-ethanol.json | while(true); do nc --send-only IP-OF-ETHANOL-DOCKER-HOST 1600; sleep 5; done
```

## Export Netflows from ipfixcol

The JSON formatted IFPIX records can be exported from the IPFIX collector [ipfixcol2](https://github.com/CESNET/ipfixcol2).
In order to start the collector, run it with the configuration file provided.

```bash
ipfixcol2 -c ethanol-ipfixcol2-config.xml
```

The possible contents of the [configuration file](ethanol-ipfixcol2-config.xml) are shown below. Note that 
the IPFIX collector listens on port *4739* and sends the JSON formatted output to the 
to the tcp endpoint *IP-OF-ETHANOL-DOCKER-HOST:1601*. It also writes the JSON to 
to a collection of output files in the specified folder.

```xml
<ipfixcol2>
<inputPlugins>
<input>
    <name>UDP input</name>
    <plugin>udp</plugin>
    <params>
        <localPort>4739</localPort>
        <localIPAddress></localIPAddress>
        <!-- Optional parameters -->
        <connectionTimeout>600</connectionTimeout>
        <templateLifeTime>1800</templateLifeTime>
        <optionsTemplateLifeTime>1800</optionsTemplateLifeTime>
    </params>
</input>
</inputPlugins>
<outputPlugins>
<output>
    <name>JSON output</name>
    <plugin>json</plugin>
    <params>
        <tcpFlags>formatted</tcpFlags>
        <timestamp>formatted</timestamp>
        <protocol>formatted</protocol>
        <ignoreUnknown>true</ignoreUnknown>
        <ignoreOptions>true</ignoreOptions>
        <nonPrintableChar>true</nonPrintableChar>
        <octetArrayAsUint>true</octetArrayAsUint>
        <numericNames>false</numericNames>
        <splitBiflow>false</splitBiflow>
        <detailedInfo>false</detailedInfo>
        <templateInfo>false</templateInfo>
        <outputs>
            <send>
                <name>Send to ethanol builder</name>
                <ip>IP-OF-ETHANOL-DOCKER-HOST</ip>
                <port>1601</port>
                <protocol>tcp</protocol>
                <blocking>no</blocking>
            </send>
            
            <file>
                <name>Store to files</name>
                <path>./flow/%Y/%m/%d/</path>
                <prefix>json.</prefix>
                <timeWindow>300</timeWindow>
                <timeAlignment>yes</timeAlignment>
                <compression>none</compression>
            </file>
        </outputs>
    </params>
</output>
</outputPlugins>
</ipfixcol2>
```