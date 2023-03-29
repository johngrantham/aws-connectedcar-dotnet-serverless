
Import-Module AWSPowerShell.NetCore

$ErrorActionPreference = "Stop"

$workspacePath="[enter base path]/aws-connectedcar-dotnet-serverless"
$bucket="[enter bucket name]"
$service="ConnectedCar"
$environment="Dev"
$version="20220801"
$stage="api"

$number=(Get-Date -UFormat "%H%M%S")
$domain="connectedcar${number}"

Write-Host " "
Write-Host "*************************************************************"
Write-Host "*            Validating the config.ps1 variables            *"
Write-Host "*************************************************************"
Write-Host " "

if (!(Test-S3Bucket -BucketName ${bucket})) {
    Write-Host "Error: bucket is not valid"
    return
}

if (!(Test-Path -path ${workspacePath})) {
    Write-Host "Error: workspacePath is not valid"
    return
}


