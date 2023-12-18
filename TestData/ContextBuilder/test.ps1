if ($null -eq $ethanolExe){ $ethanolExe  = "../../Source/Ethanol.Cli/bin/Debug/net7.0/ethanol" }

Write-Host "Testing ContextBuilder:" -ForegroundColor Yellow -BackgroundColor DarkGreen


function Compare-FileContent {
    param (
        [string]$ReferenceFilePath,
        [string]$FilePath
    )

    $referenceFile = Get-Content -Path $ReferenceFilePath
    $fileToCompare = Get-Content -Path $FilePath

    $result = Compare-Object -ReferenceObject $referenceFile -DifferenceObject $fileToCompare

    Write-Host "  Comparing $FilePath to reference file ${ReferenceFilePath}: " -ForegroundColor White -BackgroundColor DarkGreen
    if ($null -eq $result) {
        Write-Host "  OK: Files are identical." -ForegroundColor Gray -BackgroundColor DarkGreen
    } else {
        Write-host "  ERROR: Files are different." -ForegroundColor Magenta -BackgroundColor DarkGreen
    }
    Write-Host
}

Write-Host  "● Test 1: run-builder" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host  "  EXEC: $ethanolExe run-builder -c context-builder.plain.config.json" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host
Get-Content ./flows.json | & $ethanolExe run-builder -c context-builder.plain.config.json > ctx.test.1.json
Compare-FileContent ctx.reference.1.json ctx.test.1.json


Write-Host  "● Test 2: exec-builder" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host  "  EXEC: $ethanolExe exec-builder" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host
Get-Content ./flows.json | & $ethanolExe exec-builder > ctx.test.2.json
Compare-FileContent ctx.reference.2.json ctx.test.2.json

# TODO: implement also tests for TCP input and DB output...