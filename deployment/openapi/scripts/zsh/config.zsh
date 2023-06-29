#!/bin/zsh

set -e

workspacePath="[enter base path]/aws-connectedcar-dotnet-serverless"
bucket="[enter bucket name]"
service="ConnectedCar"
environment="Dev"
version="20220801"
stage="api"

number=$(date +"%H%M%S")
domain="connectedcar${number}"

echo " "
echo "*************************************************************"
echo "*            Validating the config.zsh variables            *"
echo "*************************************************************"
echo " "

if ! [ -d "${workspacePath}" ] ; then
  echo "Error: workspacePath is not valid"
  exit 1
fi

aws s3api head-bucket --bucket ${bucket}
