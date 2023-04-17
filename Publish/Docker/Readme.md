# Docker-based deployment

The docker-based deployment conists of three containers:

* Fluent-Bit
* PostreSQL
* Ethanol.ContextBuilder

The only Fluent-Bit is publicly accessible as it performs data stream routing for the application:

* tcp/1600 is an input entry point accepting Flowmon's JSON data. These records are then forwared to ethanol application for processing.
* tcp/1605 is an entry point for communicating with PostreSQL. It can be used to populate enrichment table `hosttags` used by ethanol for reading 
some extra information for hosts and also to access the table with computed context called `hostctx`.

The application works by reading the input data from Flowmon and processing them to context-based information.
The resulting data is then stored in PostreSQL table `hostctx`.

Change working folder to `Publish/Docker` and execute `docker-compose` command to build and run the application.

__Building application:__ Docker compose is used to build the application from the source codes by running the following command:

```bash
docker-compose build
```

__Running the application:__ Run the following command to start the services defined in your Docker Compose file:

```bash
docker-compose up
```

__Stopping the application:__ To stop the running services, run the following command in the same directory where you ran the docker-compose up command:

```bash
docker-compose down
```

__Cleaning environment:__ This command will stop and remove all the containers created by docker-compose up. If you want to remove the containers, networks, and volumes used by your services, you can use the --volumes and --remove-orphans options:

```bash
docker-compose down --volumes --remove-orphans
```

