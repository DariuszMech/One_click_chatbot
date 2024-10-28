# One_click_chatbot
The goal of this project is to create a pipeline enabling the creation of an entire chatbot system with one click




## Install dotnet 8.0:
    https://dotnet.microsoft.com/en-us/download/dotnet/8.0

## Install Terraform:
    https://developer.hashicorp.com/terraform/tutorials/azure-get-started/install-cli

    For Mac:

        1. brew tap hashicorp/tap

        2. brew install hashicorp/tap/terraform

        3. brew update 

        4. brew upgrade hashicorp/tap/terraform

        5. terraform -install-autocomplete

## Install the Azure CLI tool and login:
    brew update && brew install azure-cli

In your terminal, use the Azure CLI tool to setup your account permissions locally.
    
    az login

Your browser will open and prompt you to enter your Azure login credentials. After successful authentication, your terminal will display your subscription information.

## Set the account with the Azure CLI.

Find the id column for the subscription account you want to use. Once you have chosen the account subscription ID, set the account with the Azure CLI.

    az account set --subscription "subscription-id" 



## Set the environment variables 

There is a Service Principal application which is an Azure Active Directory with authentication tokens that Terraform needs to perform actions on your behalf
In your terminal, set the following environment variables 
which will connect your application to your Service Principal application 

(appropriate values should be provided to you by the owner of this project)


    export ARM_CLIENT_ID="<APPID_VALUE>"
    export ARM_CLIENT_SECRET="<PASSWORD_VALUE>"
    export ARM_SUBSCRIPTION_ID="<SUBSCRIPTION_ID>"
    export ARM_TENANT_ID="<TENANT_VALUE>"


## Initialize your Terraform configuration
    terraform init

## Format and validate the configuration
    1. terraform fmt
    2. terraform validate

## Apply your Terraform Configuration
    terraform apply

When you apply your configuration, Terraform writes data into a file called terraform.tfstate. This file contains the IDs and properties of the resources Terraform created so that it can manage or destroy those resources going forward. Your state file contains all of the data in your configuration and could also contain sensitive values in plaintext, so do not share it or check it in to source control.

## You can inspect your state

    terraform show
