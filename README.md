# One_click_chatbot
The goal of this project is to create a pipeline enabling the creation of an entire chatbot system with one click


## Prerequisites

### Install dotnet 8.0:
    https://dotnet.microsoft.com/en-us/download/dotnet/8.0

### Install Terraform:
    https://developer.hashicorp.com/terraform/tutorials/azure-get-started/install-cli

    For Mac:

        1. brew tap hashicorp/tap

        2. brew install hashicorp/tap/terraform

        3. brew update 

        4. brew upgrade hashicorp/tap/terraform

        5. terraform -install-autocomplete

### Install the Azure CLI tool and login:
    brew update && brew install azure-cli

In your terminal, use the Azure CLI tool to setup your account permissions locally.
    
    az login

Your browser will open and prompt you to enter your Azure login credentials. After successful authentication, your terminal will display your subscription information.

### Set the account with the Azure CLI.

Find the id column for the subscription account you want to use. Once you have chosen the account subscription ID, set the account with the Azure CLI.

    az account set --subscription "subscription-id" 



### Set the environment variables 

There is a Service Principal application which is an Azure Active Directory with authentication tokens that Terraform needs to perform actions on your behalf
In your terminal, set the following environment variables 
which will connect your application to your Service Principal application 

(appropriate values should be provided to you by the owner of this project)


    export ARM_CLIENT_ID="<APPID_VALUE>"
    export ARM_CLIENT_SECRET="<PASSWORD_VALUE>"
    export ARM_SUBSCRIPTION_ID="<SUBSCRIPTION_ID>"
    export ARM_TENANT_ID="<TENANT_VALUE>"


### Initialize your Terraform configuration
    terraform init

### Format and validate the configuration
    1. terraform fmt
    2. terraform validate

### Apply your Terraform Configuration
    terraform apply

When you apply your configuration, Terraform writes data into a file called terraform.tfstate. This file contains the IDs and properties of the resources Terraform created so that it can manage or destroy those resources going forward. Your state file contains all of the data in your configuration and could also contain sensitive values in plaintext, so do not share it or check it in to source control.

### You can inspect your state

    terraform show


# ExampleBot
This project is an example configuration of Azure Bot Service that can be deployed to Azure. To do that in <b>Visual Studio Code</b> you can simply:
    
    1. Right-click on ExampleBot folder
    2. Choose option 'Deploy to WebApp'
    3. Choose resource to which you want to deploy in a top bar
    4. If there appear confirmation box just click 'Deploy"

After few seconds deployment should be finished and you could test this Bot in Azure Bot Service recource to which you deployed to (-> Settings -> Test in Web Chat)

# ExampleBotTerminal
This project is an interface to talk to Azure Bot Service through a terminal on your local machine

## Prerequisites
### Set the environment variable
This value can be found in Azure portal in Bot Service recource where you deployed your ExampleBot to (-> Settings -> Channels -> Direct Line)

    export DIRECT_LINE_SECRET="<DIRECT_LINE_SECRET_VALUE>"

### Run the script
    dotnet run

 