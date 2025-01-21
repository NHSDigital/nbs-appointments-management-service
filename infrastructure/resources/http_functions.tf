# Http function app - app service and plan dedicated for running HttpTrigger functions.
# Only functions that use HttpTrigger are enabled for this function app.
# QueryAvailabilityFunction is enabled in dev and int for this service as only low loads are expected in these environments
# QueryAvailabilityFunction is disabled in staging and prod as high loads are expected in these environments

resource "azurerm_service_plan" "nbs_mya_http_func_service_plan" {
  name                = "${var.application}-fsp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
  os_type             = "Windows"
  sku_name            = "Y1"
}

resource "azurerm_windows_function_app" "nbs_mya_http_func_app" {
  name                = "${var.application}-func-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location

  storage_account_name       = azurerm_storage_account.nbs_mya_http_func_storage_account.name
  storage_account_access_key = azurerm_storage_account.nbs_mya_http_func_storage_account.primary_access_key
  service_plan_id            = azurerm_service_plan.nbs_mya_http_func_service_plan.id

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
    FUNCTIONS_WORKER_RUNTIME                                     = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE                                     = 1
    COSMOS_ENDPOINT                                              = azurerm_cosmosdb_account.nbs_mya_cosmos_db.endpoint
    COSMOS_TOKEN                                                 = azurerm_cosmosdb_account.nbs_mya_cosmos_db.primary_key
    LEASE_MANAGER_CONNECTION                                     = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING                        = azurerm_application_insights.nbs_mya_application_insights.connection_string
    Notifications_Provider                                       = "azure"
    ServiceBusConnectionString                                   = azurerm_servicebus_namespace.nbs_mya_service_bus.default_primary_connection_string
    SPLUNK_HOST_URL                                              = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                             = var.splunk_hec_token
    AuthProvider_Issuer                                          = var.auth_provider_issuer
    AuthProvider_AuthorizeUri                                    = var.auth_provider_authorize_uri
    AuthProvider_TokenUri                                        = var.auth_provider_token_uri
    AuthProvider_JwksUri                                         = var.auth_provider_jwks_uri
    AuthProvider_ChallengePhrase                                 = var.auth_provider_challenge_phrase
    AuthProvider_ClientId                                        = var.auth_provider_client_id
    AuthProvider_ClientSecret                                    = var.auth_provider_client_secret
    AuthProvider_ClientCodeExchangeUri                           = local.client_code_exchange_uri
    AuthProvider_ReturnUri                                       = local.auth_provider_return_uri
    "AzureWebJobs.QueryAvailabilityFunction.Disabled"            = var.disable_query_availability_function
    "AzureWebJobs.NotifyBookingCancelled.Disabled"               = true
    "AzureWebJobs.NotifyBookingMade.Disabled"                    = true
    "AzureWebJobs.NotifyBookingReminder.Disabled"                = true
    "AzureWebJobs.NotifyBookingRescheduled.Disabled"             = true
    "AzureWebJobs.NotifyUserRolesChanged.Disabled"               = true
    "AzureWebJobs.SendBookingReminders.Disabled"                 = true
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled" = true
  }

  sticky_settings {
    app_setting_names = [
      "AuthProvider_ClientCodeExchangeUri",
      "AuthProvider_ReturnUri",
      "Notifications_Provider",
      "ServiceBusConnectionString"
    ]
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_windows_function_app_slot" "nbs_mya_http_func_app_preview" {
  count                      = var.create_app_slot ? 1 : 0
  name                       = "preview"
  function_app_id            = azurerm_windows_function_app.nbs_mya_http_func_app.id
  storage_account_name       = azurerm_storage_account.nbs_mya_http_func_storage_account.name
  storage_account_access_key = azurerm_storage_account.nbs_mya_http_func_storage_account.primary_access_key

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
    FUNCTIONS_WORKER_RUNTIME                                     = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE                                     = 1
    COSMOS_ENDPOINT                                              = azurerm_cosmosdb_account.nbs_mya_cosmos_db.endpoint
    COSMOS_TOKEN                                                 = azurerm_cosmosdb_account.nbs_mya_cosmos_db.primary_key
    LEASE_MANAGER_CONNECTION                                     = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING                        = azurerm_application_insights.nbs_mya_application_insights.connection_string
    Notifications_Provider                                       = "none"
    SPLUNK_HOST_URL                                              = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                             = var.splunk_hec_token
    AuthProvider_Issuer                                          = var.auth_provider_issuer
    AuthProvider_AuthorizeUri                                    = var.auth_provider_authorize_uri
    AuthProvider_TokenUri                                        = var.auth_provider_token_uri
    AuthProvider_JwksUri                                         = var.auth_provider_jwks_uri
    AuthProvider_ChallengePhrase                                 = var.auth_provider_challenge_phrase
    AuthProvider_ClientId                                        = var.auth_provider_client_id
    AuthProvider_ClientSecret                                    = var.auth_provider_client_secret
    AuthProvider_ClientCodeExchangeUri                           = "${var.web_app_slot_base_uri}/manage-your-appointments/auth/set-cookie"
    AuthProvider_ReturnUri                                       = "${var.func_app_slot_base_uri}/api/auth-return"
    "AzureWebJobs.QueryAvailabilityFunction.Disabled"            = var.disable_query_availability_function
    "AzureWebJobs.NotifyBookingCancelled.Disabled"               = true
    "AzureWebJobs.NotifyBookingMade.Disabled"                    = true
    "AzureWebJobs.NotifyBookingReminder.Disabled"                = true
    "AzureWebJobs.NotifyBookingRescheduled.Disabled"             = true
    "AzureWebJobs.NotifyUserRolesChanged.Disabled"               = true
    "AzureWebJobs.SendBookingReminders.Disabled"                 = true
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled" = true
  }

  identity {
    type = "SystemAssigned"
  }
}
