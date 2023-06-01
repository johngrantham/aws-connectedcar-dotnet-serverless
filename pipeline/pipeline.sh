#!/bin/zsh

if [ -z "$1" ]; then
    echo "Usage: must include deployment parameter (sam, openapi)"
    exit 1
elif [ "$1" != "sam" ] && [ "$1" != "openapi" ]; then
    echo "Usage: must include deployment parameter (sam, openapi)"
    exit 1
fi

deployment="$1"
workspacePath="[enter base path]/aws-connectedcar-dotnet-serverless"
bucket="[enter bucket name]"
service="ConnectedCar"
environment="Dev"
stage="api"

buildFile="buildspec/${deployment}.buildspec.yml"
deployFile="deployment/${deployment}/templates/master.yaml"
testFile="buildspec/test.buildspec.yml"

repoOwner=johngrantham
sourceRepoName=aws-connectedcar-dotnet-serverless
commonRepoName=aws-connectedcar-common

echo " "
echo "*************************************************************"
echo "*      Executing create stack command for the pipeline      *"
echo "*************************************************************"
echo " "

aws cloudformation create-stack \
    --stack-name ${service}Pipeline${environment} \
    --template-body file://${workspacePath}/pipeline/pipeline.yaml \
    --parameters ParameterKey=BucketName,ParameterValue=${bucket} \
                 ParameterKey=ServiceName,ParameterValue=${service} \
                 ParameterKey=EnvironmentName,ParameterValue=${environment} \
                 ParameterKey=StageName,ParameterValue=${stage}  \
                 ParameterKey=BuildFile,ParameterValue=${buildFile} \
                 ParameterKey=TestFile,ParameterValue=${testFile} \
                 ParameterKey=DeployFile,ParameterValue=${deployFile} \
                 ParameterKey=RepoOwner,ParameterValue=${repoOwner} \
                 ParameterKey=SourceRepoName,ParameterValue=${sourceRepoName} \
                 ParameterKey=CommonRepoName,ParameterValue=${commonRepoName} \
    --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM CAPABILITY_AUTO_EXPAND

echo " "
