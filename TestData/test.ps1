Write-Host  "Building the solution.." -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host

dotnet build ../../Source/Ethanol.Cli/Ethanol.Cli.csproj

$ethanolExe = "../../Source/Ethanol.Cli/bin/Debug/net7.0/ethanol"

Write-Host "Running tests in ContextBuilder" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host

cd ContextBuilder
test.ps1
cd ..

Write-Host "Running tests in ContextProvider" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host

cd ContextProvider
test.ps1
cd ..

Write-Host "Running tests in MalwareSonar" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host

cd MalwareSonar
test.ps1
cd ..
