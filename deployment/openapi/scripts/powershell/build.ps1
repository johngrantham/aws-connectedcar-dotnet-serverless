
. "./config.ps1"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*            Running the lambda component tests             *"
Write-Host "*************************************************************"
Write-Host " "

dotnet test "${workspacePath}/src/ConnectedCar.Lambda.Test"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*              Packaging the Lambda zip file                *"
Write-Host "*************************************************************"
Write-Host " "

dotnet lambda package `
    --framework net6.0 `
    -pl "${workspacePath}/src/ConnectedCar.Lambda" `
    -o "${env:TEMP}/Lambda-${version}.zip"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*              Uploading the Lambda zip file                *"
Write-Host "*************************************************************"
Write-Host " "

Write-S3Object -BucketName ${bucket} `
    -File "${env:TEMP}/Lambda-${version}.zip" `
    -Key "${service}/${environment}/Lambda-${version}.zip"

Write-Host " "
