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
Write-Host  "  EXEC: $ethanolExe builder run -c context-builder.plain.config.json  " -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host
Get-Content ./flows.json | & $ethanolExe builder run -c context-builder.plain.config.json > ctx.test.1.json
Compare-FileContent ctx.reference.1.json ctx.test.1.json


Write-Host  "● Test 2: exec-builder" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host  "  EXEC: $ethanolExe builder exec  " -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host
Get-Content ./flows.json | & $ethanolExe builder exec > ctx.test.2.json
Compare-FileContent ctx.reference.2.json ctx.test.2.json

# TODO: implement also tests for TCP input and DB output

Write-Host  "● Test 3: run-builder ipfixcol" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host  "  EXEC: $ethanolExe builder run -c context-builder.ipfixcol.config.json  " -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host
Get-Content ./flows.ipfixcol.json | & $ethanolExe builder run -c context-builder.ipfixcol.config.json > ctx.test.3.json
Compare-FileContent ctx.reference.3.json ctx.test.3.json

Write-Host  "● Test 3: exec-builder ipfixcol" -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host  "  $ethanolExe builder exec -f ipfixcol  " -ForegroundColor Yellow -BackgroundColor DarkGreen
Write-Host
Get-Content ./flows.ipfixcol.json | & $ethanolExe builder exec -f ipfixcol-json > ctx.test.4.json
Compare-FileContent ctx.reference.4.json ctx.test.4.json
