data "azurerm_resource_group" "nbs_appts_rg" {
  name = "${var.application}-rg-${var.environment}-${var.loc}"
}

resource "azurerm_service_plan" "nbs_appts_func_sp" {
  name                = "${var.application}-sp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_appts_rg.name
  location            = data.azurerm_resource_group.nbs_appts_rg.location
  os_type             = "Windows"
  sku_name            = "Y1"
}

resource "azurerm_windows_function_app" "nbs_appts_func_app" {
  name                = "${var.application}-func-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_appts_rg.name
  location            = data.azurerm_resource_group.nbs_appts_rg.location

  storage_account_name       = azurerm_storage_account.nbs_appts_func_stacc.name
  storage_account_access_key = azurerm_storage_account.nbs_appts_func_stacc.primary_access_key
  service_plan_id            = azurerm_service_plan.nbs_appts_func_sp.id

  site_config {
    cors {
      allowed_origins = ["*"]
    }
    use_32_bit_worker = false
    application_stack {
      dotnet_version = "v8.0"
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME              = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE              = 1
    COSMOS_ENDPOINT                       = azurerm_cosmosdb_account.nbs_appts_cdb.endpoint
    COSMOS_TOKEN                          = azurerm_cosmosdb_account.nbs_appts_cdb.primary_key
    LEASE_MANAGER_CONNECTION              = azurerm_storage_account.nbs_appts_leases_stacc.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.nbs_appts_ai.connection_string
    API_KEYS                              = var.api_keys
    AuthProvider_Issuer                   = var.auth_provider_issuer
    AuthProvider_AuthorizeUri             = var.auth_provider_authorize_uri
    AuthProvider_TokenUri                 = var.auth_provider_token_uri
    AuthProvider_JwksUri                  = var.auth_provider_jwks_uri
    AuthProvider_ChallengePhrase          = var.auth_provider_challenge_phrase
    AuthProvider_ClientId                 = var.auth_provider_client_id
    AuthProvider_ReturnUri                = var.auth_provider_return_uri
    HMAC_SIGNING_KEY                      = var.hmac_signing_key
    Notifications_Provider                = "azure"
    GovNotifyApiKey                       = var.gov_notify_api_key
    ServiceBusConnectionString            = azurerm_servicebus_namespace.nbs_appts_sb.default_primary_connection_string
    DisableAvailabilityCheck              = var.disable_availability_check
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_storage_account" "nbs_appts_func_stacc" {
  name                     = "${var.application_short}func${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_appts_rg.name
  location                 = data.azurerm_resource_group.nbs_appts_rg.location
  account_replication_type = "LRS"
  account_tier             = "Standard"
}

resource "azurerm_storage_container" "nbs_appts_func_container" {
  name                 = "${var.application_short}func${var.environment}${var.loc}"
  storage_account_name = azurerm_storage_account.nbs_appts_func_stacc.name
}

## Storage account and container for concurrency leases

resource "azurerm_storage_account" "nbs_appts_leases_stacc" {
  name                     = "${var.application_short}leases${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_appts_rg.name
  location                 = data.azurerm_resource_group.nbs_appts_rg.location
  account_replication_type = "LRS"
  account_tier             = "Standard"
}

resource "azurerm_storage_container" "nbs_appts_leases_container" {
  name                 = "leases"
  storage_account_name = azurerm_storage_account.nbs_appts_leases_stacc.name
}

## Application insights

resource "azurerm_application_insights" "nbs_appts_ai" {
  name                = "${var.application}-ai-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_appts_rg.name
  location            = data.azurerm_resource_group.nbs_appts_rg.location
  application_type    = "web"
  retention_in_days   = 30
}

## Cosmos

resource "azurerm_cosmosdb_account" "nbs_appts_cdb" {
  name                       = "${var.application}-cdb-${var.environment}-${var.loc}"
  location                   = data.azurerm_resource_group.nbs_appts_rg.location
  resource_group_name        = data.azurerm_resource_group.nbs_appts_rg.name
  offer_type                 = "Standard"
  kind                       = "GlobalDocumentDB"
  automatic_failover_enabled = false
  geo_location {
    location          = data.azurerm_resource_group.nbs_appts_rg.location
    failover_priority = 0
  }

  capabilities {
    name = "EnableServerless"
  }

  consistency_policy {
    consistency_level = "Session"
  }
}

## Service Bus

resource "azurerm_servicebus_namespace" "nbs_appts_sb" {
  name                = "${var.application}-sb-${var.environment}-${var.loc}"
  location            = data.azurerm_resource_group.nbs_appts_rg.location
  resource_group_name = data.azurerm_resource_group.nbs_appts_rg.name
  sku                 = "Standard"
}

resource "azurerm_servicebus_queue" "nbs_appts_sbq_userroles" {
  name         = "user-roles-changed"
  namespace_id = azurerm_servicebus_namespace.nbs_appts_sb.id
}

resource "azurerm_servicebus_queue" "nbs_appts_sbq_bookingmade" {
  name         = "booking-made"
  namespace_id = azurerm_servicebus_namespace.nbs_appts_sb.id
}

resource "azurerm_servicebus_queue" "nbs_appts_sbq_bookingreminder" {
  name         = "booking-reminder"
  namespace_id = azurerm_servicebus_namespace.nbs_appts_sb.id
}