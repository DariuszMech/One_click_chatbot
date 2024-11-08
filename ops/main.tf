terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.8.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "3.0.2"
    }
  }
  required_version = ">= 1.1.0"
}

variable "subscription_id" {
  type = string
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
  subscription_id = var.subscription_id
}

provider "azuread" {
  tenant_id = data.azurerm_client_config.current.tenant_id
}

# Retrieve current Azure client configuration
data "azurerm_client_config" "current" {}

variable "resource_group_number" {
  description = "The number for the resource group"
  type        = string
}

resource "azurerm_resource_group" "rg" {
  name     = "TerraformOCB${var.resource_group_number}"
  location = "uksouth"
}


resource "azurerm_service_plan" "appserviceplan" {
  name                = "${azurerm_resource_group.rg.name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Windows"
  sku_name            = "F1"
}

resource "azurerm_windows_web_app" "app_service" {
  name                = "${azurerm_resource_group.rg.name}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.appserviceplan.id

  site_config {
    minimum_tls_version = "1.2"
    always_on           = false
  }

  app_settings = {
    "MicrosoftAppId"       = "${azuread_application_registration.app_registration.client_id}"
    "MicrosoftAppPassword" = "${azuread_application_password.registration_password.value}"
  }
}

resource "azuread_application_registration" "app_registration" {
  display_name     = "${azurerm_resource_group.rg.name}"
  sign_in_audience = "AzureADMultipleOrgs"
}

# Configure the Azure Bot Service using the new App Registration details
resource "azurerm_bot_service_azure_bot" "bot_service" {
  name                = "${azurerm_resource_group.rg.name}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = "global"
  microsoft_app_id    = azuread_application_registration.app_registration.client_id
  sku                 = "F0"
  endpoint            = "https://${azurerm_windows_web_app.app_service.default_hostname}/api/messages"
  microsoft_app_type  = "MultiTenant"
}

resource "azuread_application_password" "registration_password" {
  application_id = azuread_application_registration.app_registration.id
}

