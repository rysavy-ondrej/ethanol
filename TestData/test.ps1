dotnet build ../../Source/Ethanol.Cli/Ethanol.Cli.csproj

Write-Output "Running tests in ContextBuilder"
cd ContextBuilder
test.ps1
cd ..

Write-Output "Running tests in ContextProvider"
cd ContextProvider
test.ps1
cd ..

Write-Output "Running tests in MalwareSonar"
cd MalwareSonar
test.ps1
cd ..
