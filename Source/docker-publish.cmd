docker build -t ethanol .
docker built
docker login
docker tag ethanol:latest rysavyondrej/ethanol:latest
docker push rysavyondrej/ethanol:latest

pause