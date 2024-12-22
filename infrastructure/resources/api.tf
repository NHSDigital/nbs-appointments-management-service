data "azurerm_resource_group" "nbs_mya_resource_group" {
  name = "${var.application}-rg-${var.environment}-${var.loc}"
}

# Function app resources
resource "azurerm_service_plan" "nbs_mya_func_service_plan" {
  name                = "${var.application}-fsp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
  os_type             = "Windows"
  sku_name            = "Y1"
}

resource "azurerm_windows_function_app" "nbs_mya_func_app" {
  name                = "${var.application}-func-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location

  storage_account_name       = azurerm_storage_account.nbs_mya_func_storage_account.name
  storage_account_access_key = azurerm_storage_account.nbs_mya_func_storage_account.primary_access_key
  service_plan_id            = azurerm_service_plan.nbs_mya_func_service_plan.id

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
    FUNCTIONS_WORKER_RUNTIME                   = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE                   = 1
    COSMOS_ENDPOINT                            = azurerm_cosmosdb_account.nbs_mya_cosmos_db.endpoint
    COSMOS_TOKEN                               = azurerm_cosmosdb_account.nbs_mya_cosmos_db.primary_key
    LEASE_MANAGER_CONNECTION                   = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING      = azurerm_application_insights.nbs_mya_application_insights.connection_string
    AuthProvider_Issuer                        = var.auth_provider_issuer
    AuthProvider_AuthorizeUri                  = var.auth_provider_authorize_uri
    AuthProvider_TokenUri                      = var.auth_provider_token_uri
    AuthProvider_JwksUri                       = var.auth_provider_jwks_uri
    AuthProvider_ChallengePhrase               = var.auth_provider_challenge_phrase
    AuthProvider_ClientId                      = var.auth_provider_client_id
    AuthProvider_ClientSecret                  = var.auth_provider_client_secret
    AuthProvider_ClientCodeExchangeUri         = "${var.web_app_base_uri}/auth/set-cookie"
    AuthProvider_ReturnUri                     = "${var.func_app_base_uri}/api/auth-return"
    Notifications_Provider                     = "azure"
    GovNotifyBaseUri                           = var.gov_notify_base_uri
    GovNotifyApiKey                            = var.gov_notify_api_key
    ServiceBusConnectionString                 = azurerm_servicebus_namespace.nbs_mya_service_bus.default_primary_connection_string
    BookingRemindersCronSchedule               = var.booking_reminders_cron_schedule
    UnconfirmedProvisionalBookingsCronSchedule = var.unconfirmed_provisional_bookings_cron_schedule
    SPLUNK_HOST_URL                            = var.splunk_host_url
    SPLUNK_HEC_TOKEN                           = var.splunk_hec_token
  }

  sticky_settings {
    app_setting_names = ["AuthProvider_ClientCodeExchangeUri", "AuthProvider_ReturnUri"]
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_windows_function_app_slot" "nbs_mya_func_app_preview" {
  count                      = var.do_create_swap_slot ? 1 : 0
  name                       = "preview"
  function_app_id            = azurerm_windows_function_app.nbs_mya_func_app.id
  storage_account_name       = azurerm_storage_account.nbs_mya_func_storage_account.name
  storage_account_access_key = azurerm_storage_account.nbs_mya_func_storage_account.primary_access_key

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
    FUNCTIONS_WORKER_RUNTIME                   = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE                   = 1
    COSMOS_ENDPOINT                            = azurerm_cosmosdb_account.nbs_mya_cosmos_db.endpoint
    COSMOS_TOKEN                               = azurerm_cosmosdb_account.nbs_mya_cosmos_db.primary_key
    LEASE_MANAGER_CONNECTION                   = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING      = azurerm_application_insights.nbs_mya_application_insights.connection_string
    AuthProvider_Issuer                        = var.auth_provider_issuer
    AuthProvider_AuthorizeUri                  = var.auth_provider_authorize_uri
    AuthProvider_TokenUri                      = var.auth_provider_token_uri
    AuthProvider_JwksUri                       = var.auth_provider_jwks_uri
    AuthProvider_ChallengePhrase               = var.auth_provider_challenge_phrase
    AuthProvider_ClientId                      = var.auth_provider_client_id
    AuthProvider_ClientSecret                  = var.auth_provider_client_secret
    AuthProvider_ClientCodeExchangeUri         = "${var.web_app_slot_base_uri}/auth/set-cookie"
    AuthProvider_ReturnUri                     = "${var.func_app_slot_base_uri}/api/auth-return"
    Notifications_Provider                     = "azure"
    GovNotifyBaseUri                           = var.gov_notify_base_uri
    GovNotifyApiKey                            = var.gov_notify_api_key
    ServiceBusConnectionString                 = azurerm_servicebus_namespace.nbs_mya_service_bus.default_primary_connection_string
    BookingRemindersCronSchedule               = var.booking_reminders_cron_schedule
    UnconfirmedProvisionalBookingsCronSchedule = var.unconfirmed_provisional_bookings_cron_schedule
    SPLUNK_HOST_URL                            = var.splunk_host_url
    SPLUNK_HEC_TOKEN                           = var.splunk_hec_token
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_storage_account" "nbs_mya_func_storage_account" {
  name                     = "${var.application_short}strgfunc${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  location                 = data.azurerm_resource_group.nbs_mya_resource_group.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

## Storage account and container for concurrency leases
resource "azurerm_storage_account" "nbs_mya_leases_storage_account" {
  name                     = "${var.application_short}strglease${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  location                 = data.azurerm_resource_group.nbs_mya_resource_group.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

resource "azurerm_storage_container" "nbs_mya_leases_container" {
  name               = "leases"
  storage_account_id = azurerm_storage_account.nbs_mya_leases_storage_account.id
}

## Application insights
resource "azurerm_application_insights" "nbs_mya_application_insights" {
  name                = "${var.application}-ai-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
  application_type    = "web"
  sampling_percentage = var.app_insights_sampling_percentage
}

## Service Bus
resource "azurerm_servicebus_namespace" "nbs_mya_service_bus" {
  name                = "${var.application}-sb-${var.environment}-${var.loc}"
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  sku                 = "Standard"
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_user_roles" {
  name         = "user-roles-changed"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_booking_made" {
  name         = "booking-made"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_booking_cancelled" {
  name         = "booking-cancelled"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_bookingreminder" {
  name         = "booking-reminder"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_booking_rescheduled" {
  name         = "booking-rescheduled"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}
