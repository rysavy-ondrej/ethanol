# Docker Deployment of Ethanol

Ethanol can be efficiently deployed as a containerized application using Docker. This setup involves utilizing various Docker images, including the official Postgres image and the Ethanol runtime image.

## Required Docker Images

- **Postgres Official Docker Image**: 
  - Repository: [Postgres on Docker Hub](https://hub.docker.com/_/postgres)
- **Ethanol Runtime Image**: 
  - Choose between the precompiled image or building from source, as described below.

## Deployment Options

Depending on your requirements, select one of the two Docker Compose configurations to deploy Ethanol:

### Option 1: Using Precompiled Ethanol Image

- **Image**: `rysavyondrej/ethanol:latest` ([Docker Hub](https://hub.docker.com/r/rysavyondrej/ethanol))
- **Use Case**: Ideal for deploying the latest stable version, particularly suitable for production environments.
- **Deployment Command**:
  ```bash
  docker-compose -f docker-compose.prod.yml up
  ```
- **Further Information**: Refer to [Docker-Production.md] for more details on production deployment.

### Option 2: Building from Source

- **Use Case**: Recommended during development and for experimental deployments of unpublished application versions.
- **Building and Running**:
  - First, build the image from source:
    ```bash
    docker compose -f docker-compose.devel.yml build
    ```
  - Then, run the application:
    ```bash
    docker compose -f docker-compose.devel.yml up
    ```
- **Further Information**: See [Docker-Development.md] for more details on development deployment.
