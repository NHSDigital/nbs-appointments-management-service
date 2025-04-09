# ServiceBus function app - app service and plan dedicated for running TimerTrigger functions.
# Only functions that use TimerTrigger are enabled for this function app

resource "azurerm_service_plan" "nbs_mya_timer_func_service_plan" {
  name                = "${var.application}-timerfsp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = var.location
  os_type             = "Windows"
  sku_name            = "Y1"
}

resource "azurerm_windows_function_app" "nbs_mya_timer_func_app" {
  name                = "${var.application}-timerfunc-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = var.location

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
    COSMOS_ENDPOINT                                                        = var.cosmos_endpoint != "" ? var.cosmos_endpoint : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
    COSMOS_TOKEN                                                           = var.cosmos_token != "" ? var.cosmos_token : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
    APP_CONFIG_CONNECTION                                                  = azurerm_app_configuration.nbs_mya_app_configuration[0].primary_read_key[0].connection_string
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
    "AzureWebJobs.NotifyBookingCancelled.Disabled"                         = true
    "AzureWebJobs.NotifyBookingMade.Disabled"                              = true
    "AzureWebJobs.NotifyBookingReminder.Disabled"                          = true
    "AzureWebJobs.NotifyBookingRescheduled.Disabled"                       = true
    "AzureWebJobs.NotifyUserRolesChanged.Disabled"                         = true
    "AzureWebJobs.NotifyOktaUserRolesChanged.Disabled"                     = true
    "AzureWebJobs.ApplyAvailabilityTemplateFunction.Disabled"              = true
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled"                   = true
    "AzureWebJobs.AuthenticateFunction.Disabled"                           = true
    "AzureWebJobs.CancelBookingFunction.Disabled"                          = true
    "AzureWebJobs.CancelSessionFunction.Disabled"                          = true
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled"              = true
    "AzureWebJobs.ConsentToEula.Disabled"                                  = true
    "AzureWebJobs.GetAccessibilityDefinitionsFunction.Disabled"            = true
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
    "AzureWebJobs.SetSiteDetailsFunction.Disabled"                         = true
    "AzureWebJobs.SetSiteReferenceDetailsFunction.Disabled"                = true
    "AzureWebJobs.SetUserRoles.Disabled"                                   = true
    "AzureWebJobs.TriggerBookingReminders.Disabled"                        = true
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = true
    "AzureWebJobs.BulkImportFunction.Disabled"                             = true
    "AzureWebJobs.RenderOAuth2Redirect.Disabled"                           = true
    "AzureWebJobs.RenderOpenApiDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerUI.Disabled"                                = true
    "AzureWebJobs.ClearLocalFeatureFlagOverridesFunction.Disabled"         = true
    "AzureWebJobs.SetLocalFeatureFlagOverrideFunction.Disabled"            = true
    "AzureWebJobs.GetFeatureFlagFunction.Disabled"                         = true
    "AzureWebJobs.GetClinicalServicesFunction.Disabled"                    = true
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
    COSMOS_ENDPOINT                                                        = azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
    COSMOS_TOKEN                                                           = azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
    APP_CONFIG_CONNECTION                                                  = azurerm_app_configuration.nbs_mya_app_configuration[0].primary_read_key[0].connection_string
    LEASE_MANAGER_CONNECTION                                               = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING                                  = azurerm_application_insights.nbs_mya_application_insights.connection_string
    Notifications_Provider                                                 = "none"
    GovNotifyBaseUri                                                       = var.gov_notify_base_uri
    GovNotifyApiKey                                                        = var.gov_notify_api_key
    BookingRemindersCronSchedule                                           = var.booking_reminders_cron_schedule
    UnconfirmedProvisionalBookingsCronSchedule                             = var.unconfirmed_provisional_bookings_cron_schedule
    SPLUNK_HOST_URL                                                        = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                                       = var.splunk_hec_token
    "AzureWebJobs.NotifyBookingCancelled.Disabled"                         = true
    "AzureWebJobs.NotifyBookingMade.Disabled"                              = true
    "AzureWebJobs.NotifyBookingReminder.Disabled"                          = true
    "AzureWebJobs.NotifyBookingRescheduled.Disabled"                       = true
    "AzureWebJobs.NotifyUserRolesChanged.Disabled"                         = true
    "AzureWebJobs.NotifyOktaUserRolesChanged.Disabled"                     = true
    "AzureWebJobs.ApplyAvailabilityTemplateFunction.Disabled"              = true
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled"                   = true
    "AzureWebJobs.AuthenticateFunction.Disabled"                           = true
    "AzureWebJobs.CancelBookingFunction.Disabled"                          = true
    "AzureWebJobs.CancelSessionFunction.Disabled"                          = true
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled"              = true
    "AzureWebJobs.ConsentToEula.Disabled"                                  = true
    "AzureWebJobs.GetAccessibilityDefinitionsFunction.Disabled"            = true
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
    "AzureWebJobs.SetSiteDetailsFunction.Disabled"                         = true
    "AzureWebJobs.SetSiteReferenceDetailsFunction.Disabled"                = true
    "AzureWebJobs.SetUserRoles.Disabled"                                   = true
    "AzureWebJobs.TriggerBookingReminders.Disabled"                        = true
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = true
    "AzureWebJobs.SendBookingReminders.Disabled"                           = true
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled"           = true
    "AzureWebJobs.ClearLocalFeatureFlagOverridesFunction.Disabled"         = true
    "AzureWebJobs.SetLocalFeatureFlagOverrideFunction.Disabled"            = true
    "AzureWebJobs.GetFeatureFlagFunction.Disabled"                         = true
  }

  identity {
    type = "SystemAssigned"
  }
}
