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
commonPath="[enter base path]/aws-connectedcar-common"
bucket="[enter bucket name]"
zip="connectedcar.zip"
service="ConnectedCar"
environment="Dev"
stage="api"

buildFile="buildspec/${deployment}.buildspec.yml"
deployFile="deployment/${deployment}/templates/master.yaml"
testFile="buildspec/test.buildspec.yml"

echo " "
echo "*************************************************************"
echo "*                   Cleaning the solution                   *"
echo "*************************************************************"
echo " "

dotnet clean "${workspacePath}/src/ConnectedCar.sln" -v quiet

echo " "
echo "*************************************************************"
echo "*               Creating zip file of solution               *"
echo "*************************************************************"
echo " "

rm -f ${TMPDIR}/${zip}

cd ${workspacePath}

zip -r ${TMPDIR}/${zip} . -x '*.git*' -x '*/bin/*' -x '*/obj/*' -x '*/*.zip' -x '*.DS_Store*'

cd ${commonPath}

zip -r ${TMPDIR}/${zip} . -x '*.git*' -x '*/bin/*' -x '*/obj/*' -x '*/*.zip' -x '*.DS_Store*'


echo " "
echo "*************************************************************"
echo "*              Uploading zip file to S3 folder              *"
echo "*************************************************************"
echo " "

aws s3 cp \
    ${TMPDIR}/${zip} \
    s3://${bucket}/${service}/Repo/${zip}

echo " "
echo "*************************************************************"
echo "*      Executing create stack command for the pipeline      *"
echo "*************************************************************"
echo " "

aws cloudformation create-stack \
    --stack-name ${service}Pipeline${environment} \
    --template-body file://${workspacePath}/pipeline/pipeline.yaml \
    --parameters ParameterKey=BucketName,ParameterValue=${bucket} \
                 ParameterKey=ZipFile,ParameterValue=${zip} \
                 ParameterKey=ServiceName,ParameterValue=${service} \
                 ParameterKey=EnvironmentName,ParameterValue=${environment} \
                 ParameterKey=StageName,ParameterValue=${stage}  \
                 ParameterKey=BuildFile,ParameterValue=${buildFile} \
                 ParameterKey=TestFile,ParameterValue=${testFile} \
                 ParameterKey=DeployFile,ParameterValue=${deployFile} \
    --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM CAPABILITY_AUTO_EXPAND

echo " "
