# Compose Deploy

This document outlines the method for deploying Docker-based applications. It assumes that all required images are already available in a Docker repository.

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
   - Transfer `docker-compose.yml` and `ethanol-db-init.sql` files to the *ethanol* machine.
   - Update the permissions of the SQL configuration file to make it readable by all:
     ```bash
     chmod a+r ethanol-db-init.sql
     ```

2. **Execute Docker Compose**:
   - Run the following command to start the deployment process:
     ```bash
     docker-compose up
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