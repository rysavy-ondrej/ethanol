# Executes the Ethanol Provider service 
# Note: It is crucial that this file uses LF for line ends (not CRLF)!
#       Otherwise the docker container will not start!
cd /app/
./ethanol start-service -c ethanol-provider.config.json
