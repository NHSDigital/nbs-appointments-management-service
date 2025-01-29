# ServiceBus function app - app service and plan dedicated for running TimerTrigger functions.
# Only functions that use TimerTrigger are enabled for this function app

resource "azurerm_service_plan" "nbs_mya_timer_func_service_plan" {
  name                = "${var.application}-timerfsp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
  os_type             = "Windows"
  sku_name            = "Y1"
}

resource "azurerm_windows_function_app" "nbs_mya_timer_func_app" {
  name                = "${var.application}-timerfunc-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location

  storage_account_name       = azurerm_storage_account.nbs_mya_timer_func_storage_account.name
  storage_account_access_key = azurerm_storage_account.nbs_mya_timer_func_storage_account.primary_access_key
  service_plan_id            = azurerm_service_plan.nbs_mya_timer_func_service_plan.id

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
    FUNCTIONS_WORKER_RUNTIME                                               = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE                                               = 1
    COSMOS_ENDPOINT                                                        = azurerm_cosmosdb_account.nbs_mya_cosmos_db.endpoint
    COSMOS_TOKEN                                                           = azurerm_cosmosdb_account.nbs_mya_cosmos_db.primary_key
    LEASE_MANAGER_CONNECTION                                               = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING                                  = azurerm_application_insights.nbs_mya_application_insights.connection_string
    Notifications_Provider                                                 = "azure"
    GovNotifyBaseUri                                                       = var.gov_notify_base_uri
    GovNotifyApiKey                                                        = var.gov_notify_api_key
    ServiceBusConnectionString                                             = azurerm_servicebus_namespace.nbs_mya_service_bus.default_primary_connection_string
    BookingRemindersCronSchedule                                           = var.booking_reminders_cron_schedule
    UnconfirmedProvisionalBookingsCronSchedule                             = var.unconfirmed_provisional_bookings_cron_schedule
    SPLUNK_HOST_URL                                                        = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                                       = var.splunk_hec_token
    AuthProvider_Issuer                                                    = var.auth_provider_issuer
    AuthProvider_AuthorizeUri                                              = var.auth_provider_authorize_uri
    AuthProvider_TokenUri                                                  = var.auth_provider_token_uri
    AuthProvider_JwksUri                                                   = var.auth_provider_jwks_uri
    AuthProvider_ChallengePhrase                                           = var.auth_provider_challenge_phrase
    AuthProvider_ClientId                                                  = var.auth_provider_client_id
    AuthProvider_ClientSecret                                              = var.auth_provider_client_secret
    AuthProvider_ClientCodeExchangeUri                                     = local.client_code_exchange_uri
    AuthProvider_ReturnUri                                                 = local.auth_provider_return_uri
    "AzureWebJobs.NotifyBookingCancelled.Disabled"                         = true
    "AzureWebJobs.NotifyBookingMade.Disabled"                              = true
    "AzureWebJobs.NotifyBookingReminder.Disabled"                          = true
    "AzureWebJobs.NotifyBookingRescheduled.Disabled"                       = true
    "AzureWebJobs.NotifyUserRolesChanged.Disabled"                         = true
    "AzureWebJobs.ApplyAvailabilityTemplateFunction.Disabled"              = true
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled"                   = true
    "AzureWebJobs.AuthenticateFunction.Disabled"                           = true
    "AzureWebJobs.CancelBookingFunction.Disabled"                          = true
    "AzureWebJobs.CancelSessionFunction.Disabled"                          = true
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled"              = true
    "AzureWebJobs.ConsentToEula.Disabled"                                  = true
    "AzureWebJobs.GetAttributeDefinitionsFunction.Disabled"                = true
    "AzureWebJobs.GetAuthTokenFunction.Disabled"                           = true
    "AzureWebJobs.GetAvailabilityCreatedEventsFunction.Disabled"           = true
    "AzureWebJobs.GetDailyAvailabilityFunction.Disabled"                   = true
    "AzureWebJobs.GetEulaFunction.Disabled"                                = true
    "AzureWebJobs.GetRolesFunction.Disabled"                               = true
    "AzureWebJobs.GetSiteFunction.Disabled"                                = true
    "AzureWebJobs.GetSiteMetaData.Disabled"                                = true
    "AzureWebJobs.GetSitesByAreaFunction.Disabled"                         = true
    "AzureWebJobs.GetSitesPreviewFunction.Disabled"                        = true
    "AzureWebJobs.GetPermissionsForUserFunction.Disabled"                  = true
    "AzureWebJobs.GetUserProfileFunction.Disabled"                         = true
    "AzureWebJobs.GetUserRoleAssignmentsFunction.Disabled"                 = true
    "AzureWebJobs.GetWellKnownOdsCodeEntriesFunction.Disabled"             = true
    "AzureWebJobs.MakeBookingFunction.Disabled"                            = true
    "AzureWebJobs.QueryAvailabilityFunction.Disabled"                      = true
    "AzureWebJobs.QueryBookingByNhsNumberReference.Disabled"               = true
    "AzureWebJobs.QueryBookingByBookingReference.Disabled"                 = true
    "AzureWebJobs.QueryBookingsFunction.Disabled"                          = true
    "AzureWebJobs.RemoveUserFunction.Disabled"                             = true
    "AzureWebJobs.SetAvailabilityFunction.Disabled"                        = true
    "AzureWebJobs.SetBookingStatusFunction.Disabled"                       = true
    "AzureWebJobs.SetSiteAccessibilitiesFunction.Disabled"                 = true
    "AzureWebJobs.SetSiteInformationForCitizensFunction.Disabled"          = true
    "AzureWebJobs.SetUserRoles.Disabled"                                   = true
    "AzureWebJobs.TriggerBookingReminders.Disabled"                        = true
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = true
  }

  sticky_settings {
    app_setting_names = [
      "AuthProvider_ClientCodeExchangeUri",
      "AuthProvider_ReturnUri",
      "Notifications_Provider",
      "ServiceBusConnectionString",
      "AzureWebJobs.SendBookingReminders.Disabled",
      "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled"
    ]
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_windows_function_app_slot" "nbs_mya_timer_func_app_preview" {
  count                      = var.create_app_slot ? 1 : 0
  name                       = "preview"
  function_app_id            = azurerm_windows_function_app.nbs_mya_timer_func_app.id
  storage_account_name       = azurerm_storage_account.nbs_mya_timer_func_storage_account.name
  storage_account_access_key = azurerm_storage_account.nbs_mya_timer_func_storage_account.primary_access_key

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
    FUNCTIONS_WORKER_RUNTIME                                               = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE                                               = 1
    COSMOS_ENDPOINT                                                        = azurerm_cosmosdb_account.nbs_mya_cosmos_db.endpoint
    COSMOS_TOKEN                                                           = azurerm_cosmosdb_account.nbs_mya_cosmos_db.primary_key
    LEASE_MANAGER_CONNECTION                                               = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING                                  = azurerm_application_insights.nbs_mya_application_insights.connection_string
    Notifications_Provider                                                 = "none"
    GovNotifyBaseUri                                                       = var.gov_notify_base_uri
    GovNotifyApiKey                                                        = var.gov_notify_api_key
    BookingRemindersCronSchedule                                           = var.booking_reminders_cron_schedule
    UnconfirmedProvisionalBookingsCronSchedule                             = var.unconfirmed_provisional_bookings_cron_schedule
    SPLUNK_HOST_URL                                                        = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                                       = var.splunk_hec_token
    AuthProvider_Issuer                                                    = var.auth_provider_issuer
    AuthProvider_AuthorizeUri                                              = var.auth_provider_authorize_uri
    AuthProvider_TokenUri                                                  = var.auth_provider_token_uri
    AuthProvider_JwksUri                                                   = var.auth_provider_jwks_uri
    AuthProvider_ChallengePhrase                                           = var.auth_provider_challenge_phrase
    AuthProvider_ClientId                                                  = var.auth_provider_client_id
    AuthProvider_ClientSecret                                              = var.auth_provider_client_secret
    AuthProvider_ClientCodeExchangeUri                                     = local.client_code_exchange_uri
    AuthProvider_ReturnUri                                                 = local.auth_provider_return_uri
    "AzureWebJobs.NotifyBookingCancelled.Disabled"                         = true
    "AzureWebJobs.NotifyBookingMade.Disabled"                              = true
    "AzureWebJobs.NotifyBookingReminder.Disabled"                          = true
    "AzureWebJobs.NotifyBookingRescheduled.Disabled"                       = true
    "AzureWebJobs.NotifyUserRolesChanged.Disabled"                         = true
    "AzureWebJobs.ApplyAvailabilityTemplateFunction.Disabled"              = true
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled"                   = true
    "AzureWebJobs.AuthenticateFunction.Disabled"                           = true
    "AzureWebJobs.CancelBookingFunction.Disabled"                          = true
    "AzureWebJobs.CancelSessionFunction.Disabled"                          = true
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled"              = true
    "AzureWebJobs.ConsentToEula.Disabled"                                  = true
    "AzureWebJobs.GetAttributeDefinitionsFunction.Disabled"                = true
    "AzureWebJobs.GetAuthTokenFunction.Disabled"                           = true
    "AzureWebJobs.GetAvailabilityCreatedEventsFunction.Disabled"           = true
    "AzureWebJobs.GetDailyAvailabilityFunction.Disabled"                   = true
    "AzureWebJobs.GetEulaFunction.Disabled"                                = true
    "AzureWebJobs.GetRolesFunction.Disabled"                               = true
    "AzureWebJobs.GetSiteFunction.Disabled"                                = true
    "AzureWebJobs.GetSiteMetaData.Disabled"                                = true
    "AzureWebJobs.GetSitesByAreaFunction.Disabled"                         = true
    "AzureWebJobs.GetPermissionsForUserFunction.Disabled"                  = true
    "AzureWebJobs.GetUserProfileFunction.Disabled"                         = true
    "AzureWebJobs.GetUserRoleAssignmentsFunction.Disabled"                 = true
    "AzureWebJobs.GetWellKnownOdsCodeEntriesFunction.Disabled"             = true
    "AzureWebJobs.MakeBookingFunction.Disabled"                            = true
    "AzureWebJobs.QueryAvailabilityFunction.Disabled"                      = true
    "AzureWebJobs.QueryBookingByNhsNumberReference.Disabled"               = true
    "AzureWebJobs.QueryBookingByBookingReference.Disabled"                 = true
    "AzureWebJobs.QueryBookingsFunction.Disabled"                          = true
    "AzureWebJobs.RemoveUserFunction.Disabled"                             = true
    "AzureWebJobs.SetAvailabilityFunction.Disabled"                        = true
    "AzureWebJobs.SetBookingStatusFunction.Disabled"                       = true
    "AzureWebJobs.SetSiteAttributesFunction.Disabled"                      = true
    "AzureWebJobs.SetUserRoles.Disabled"                                   = true
    "AzureWebJobs.TriggerBookingReminders.Disabled"                        = true
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = true
    "AzureWebJobs.SendBookingReminders.Disabled"                           = true
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled"           = true
  }

  identity {
    type = "SystemAssigned"
  }
}
