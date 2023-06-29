#!/bin/zsh

source config.zsh

echo " "
echo "*************************************************************"
echo "*       Uploading the OpenApi files to the S3 folder        *"
echo "*************************************************************"
echo " "

cat ${workspacePath}/deployment/openapi/specifications/admin.openapi.yaml \
    ${workspacePath}/deployment/openapi/specifications/schemas.openapi.yaml \
    | aws s3 cp - s3://${bucket}/${service}/${environment}/admin.openapi.yaml

cat ${workspacePath}/deployment/openapi/specifications/vehicle.openapi.yaml \
    ${workspacePath}/deployment/openapi/specifications/schemas.openapi.yaml \
    | aws s3 cp - s3://${bucket}/${service}/${environment}/vehicle.openapi.yaml

cat ${workspacePath}/deployment/openapi/specifications/customer.openapi.yaml \
    ${workspacePath}/deployment/openapi/specifications/schemas.openapi.yaml \
    | aws s3 cp - s3://${bucket}/${service}/${environment}/customer.openapi.yaml

echo " "
echo "*************************************************************"
echo "*  Uploading the CloudFormation templates to the S3 folder  *"
echo "*************************************************************"
echo " "

aws s3 cp \
    ${workspacePath}/deployment/openapi/templates/services.yaml \
    s3://${bucket}/${service}/${environment}/services.yaml

aws s3 cp \
    ${workspacePath}/deployment/openapi/templates/roles.yaml \
    s3://${bucket}/${service}/${environment}/roles.yaml

aws s3 cp \
    ${workspacePath}/deployment/openapi/templates/customer.yaml \
    s3://${bucket}/${service}/${environment}/customer.yaml

aws s3 cp \
    ${workspacePath}/deployment/openapi/templates/admin.yaml \
    s3://${bucket}/${service}/${environment}/admin.yaml

aws s3 cp \
    ${workspacePath}/deployment/openapi/templates/vehicle.yaml \
    s3://${bucket}/${service}/${environment}/vehicle.yaml

if ! aws cloudformation describe-stacks --stack-name ${service}${environment} &>/dev/null ; then

echo " "
echo "*************************************************************"
echo "*       Executing create stack and waiting for results      *"
echo "*************************************************************"
echo " "

aws cloudformation create-stack \
    --stack-name ${service}${environment} \
    --template-body file://${workspacePath}/deployment/openapi/templates/master.yaml \
    --parameters ParameterKey=BucketName,ParameterValue=${bucket} \
                 ParameterKey=ServiceName,ParameterValue=${service} \
                 ParameterKey=EnvironmentName,ParameterValue=${environment} \
                 ParameterKey=VersionNumber,ParameterValue=${version} \
                 ParameterKey=StageName,ParameterValue=${stage} \
                 ParameterKey=UserPoolDomainName,ParameterValue=${domain} \
    --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM CAPABILITY_AUTO_EXPAND

aws cloudformation wait stack-create-complete \
    --stack-name ${service}${environment}

else

echo " "
echo "*************************************************************"
echo "*       Executing update stack and waiting for results      *"
echo "*************************************************************"
echo " "

domain=$(aws cloudformation describe-stacks \
    --stack-name ${service}${environment} \
    --query "Stacks[0].Outputs[?OutputKey=='UserPoolDomainName'].OutputValue" \
    --output text)

aws cloudformation update-stack \
    --stack-name ${service}${environment} \
    --template-body file://${workspacePath}/deployment/openapi/templates/master.yaml \
    --parameters ParameterKey=BucketName,ParameterValue=${bucket} \
                 ParameterKey=ServiceName,ParameterValue=${service} \
                 ParameterKey=EnvironmentName,ParameterValue=${environment} \
                 ParameterKey=VersionNumber,ParameterValue=${version} \
                 ParameterKey=StageName,ParameterValue=${stage} \
                 ParameterKey=UserPoolDomainName,ParameterValue=${domain} \
    --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM CAPABILITY_AUTO_EXPAND

aws cloudformation wait stack-update-complete \
    --stack-name ${service}${environment}

fi

echo " "
echo "*************************************************************"
echo "*                 Listing the stack outputs                 *"
echo "*************************************************************"
echo " "

aws cloudformation describe-stacks \
    --stack-name ${service}${environment} \
    --query 'Stacks[].Outputs[]' \
    --output table

echo " "
