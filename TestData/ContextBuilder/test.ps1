Write-Host "Testing ContextBuilder:" -ForegroundColor Yellow -BackgroundColor DarkGreen

function Compare-FileContent {
    param (
        [string]$ReferenceFilePath,
        [string]$FilePath
    )

    $referenceFile = Get-Content -Path $ReferenceFilePath
    $fileToCompare = Get-Content -Path $FilePath

    $result = Compare-Object -ReferenceObject $referenceFile -DifferenceObject $fileToCompare

    Write-Host "  Comparing $FilePath to reference file ${ReferenceFilePath}:" -ForegroundColor White -BackgroundColor DarkGreen
    if ($null -eq $result) {
        Write-Host "  OK: Files are identical." -ForegroundColor Gray -BackgroundColor DarkGreen
    } else {
        Write-host "  ERROR: Files are different." -ForegroundColor Magenta -BackgroundColor DarkGreen
    }
}

Write-Host  "● Test1: run-builder" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host  " "
Get-Content ./flows.json | ../../Source/Ethanol.Cli/bin/Debug/net7.0/ethanol.exe run-builder -c context-builder.plain.config.json > ctx.test.1.json
Compare-FileContent ctx.reference.1.json ctx.test.1.json


Write-Host  "● Test2: exec-builder" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host  " "
Get-Content ./flows.json | ../../Source/Ethanol.Cli/bin/Debug/net7.0/ethanol.exe exec-builder -c context-builder.plain.config.json > ctx.test.2.json
Compare-FileContent ctx.reference.2.json ctx.test.2.json

# TODO: implement also tests for TCP input and DB output...