terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0.2"
    }
    azuread = {
      source = "hashicorp/azuread"
      version = "3.0.2"
    }
  }
  required_version = ">= 1.1.0"
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

provider "azuread" {
  tenant_id = data.azurerm_client_config.current.tenant_id
}

# Retrieve current Azure client configuration
data "azurerm_client_config" "current" {}

resource "random_integer" "ri" {
  min = 10000
  max = 99999
}

resource "azurerm_resource_group" "rg" {
  name     = "TerraformOCB"
  location = "westus2"
}


resource "azurerm_service_plan" "appserviceplan" {
  name                = "${azurerm_resource_group.rg.name}-WebApp-ASP-${random_integer.ri.result}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Windows"
  sku_name            = "F1"

}

resource "azurerm_windows_web_app" "app_service" {
  name                = "${azurerm_resource_group.rg.name}-WebApp-${random_integer.ri.result}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.appserviceplan.id

  site_config {
    minimum_tls_version = "1.2"
    always_on           = false
  }

  app_settings = {
    "MicrosoftAppId" = "${azuread_application_registration.app_registration.client_id}"
    "MicrosoftAppPassword" = "${azuread_application_password.registration_password.value}"
  }
}

resource "azuread_application_registration" "app_registration" {
  display_name     = "${azurerm_resource_group.rg.name}-AppReg-${random_integer.ri.result}"
}

# Configure the Azure Bot Service using the new App Registration details
resource "azurerm_bot_service_azure_bot" "bot_service" {
  name                    = "${azurerm_resource_group.rg.name}-Bot-${random_integer.ri.result}"
  resource_group_name     = azurerm_resource_group.rg.name
  location                = "global"
  microsoft_app_id        = azuread_application_registration.app_registration.client_id
  sku                     = "F0"
  endpoint                = "https://${azurerm_windows_web_app.app_service.default_hostname}/api/messages"
}


resource "azuread_application_password" "registration_password" {
  application_id = azuread_application_registration.app_registration.id
}

