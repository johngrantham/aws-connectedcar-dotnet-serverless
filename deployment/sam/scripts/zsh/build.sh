#!/bin/zsh

source config.sh

echo " "
echo "*************************************************************"
echo "*            Running the lambda component tests             *"
echo "*************************************************************"
echo " "

dotnet test ${workspacePath}/src/ConnectedCar.Lambda.Test

echo " "
echo "*************************************************************"
echo "*        Building and packaging the Lambda zip file         *"
echo "*************************************************************"
echo " "

dotnet lambda package \
    --framework net6.0 \
    -pl ${workspacePath}/src/ConnectedCar.Lambda \
    -o ${TMPDIR}/Lambda-${version}.zip

echo " "
echo "*************************************************************"
echo "*      Uploading the Lambda zip file to the S3 folder       *"
echo "*************************************************************"
echo " "

aws s3 cp \
    ${TMPDIR}/Lambda-${version}.zip \
    s3://${bucket}/${service}/${environment}/Lambda-${version}.zip

echo " "

