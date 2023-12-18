# Deployment Options for Ethanol Application

This folder contains various deployment options for the Ethanol application, catering to different environments and preferences. Below are the available methods:

## 1. Binary Executable

For users who prefer a straightforward, traditional application installation, the binary executable of the Ethanol application is available. This option is suitable for those who want to run the application directly on their local machine or server without containerization.

### Details

- **Location**: The binary executable and its documentation can be found in the [Bin directory](Bin/Readme.md).
- **Contents**: The Bin directory contains the precompiled binary of the Ethanol application, along with a README.md file that provides detailed instructions on how to install and run the application.
- **Usage**: Ideal for standalone deployments where simplicity and direct execution are key. This method does not require Docker or container management skills.

## 2. Docker-Based Deployment

For users who prefer containerization, a Docker-based deployment option is available. This method bundles the Ethanol application along with its backend database in Docker containers, providing an isolated and consistent environment.

### Details

- **Location**: The Docker deployment files and instructions are located in the [Docker directory](Docker/Readme.md).
- **Contents**: The Docker directory includes Dockerfiles and Docker Compose scripts needed to set up the Ethanol application and its backend database in containers.
- **Usage**: Suitable for scenarios where containerization is preferred, such as in cloud environments, microservices architectures, or for ensuring consistency across different systems.
- **Advantages**: Docker-based deployment offers easy scalability, quick setup, and reduced conflicts between running applications due to isolated environments.

Each deployment option is tailored to different user needs and system environments. For detailed instructions on how to deploy using either of these methods, please refer to the respective README.md files in the Bin and Docker directories.