$exec = $Env:ETHANOL_EXE
echo $PWD
echo $exec
 
Get-Content flows.ipfixcol.json | & ${exec} builder run -c context-builder.plain.config.json > ctx.ipfixcol.json