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
    APP_CONFIG_CONNECTION                                                  = var.app_config_connection != "" ? var.app_config_connection : azurerm_app_configuration.nbs_mya_app_configuration[0].primary_read_key[0].connection_string
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
    "AzureWebJobs.ApplyAvailabilityTemplate.Disabled" = !contains(var.timer_functions, "ApplyAvailabilityTemplate")
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled" = !contains(var.timer_functions, "AuthenticateCallbackFunction")
    "AzureWebJobs.AuthenticateFunction.Disabled" = !contains(var.timer_functions, "AuthenticateFunction")
    "AzureWebJobs.BulkImportFunction.Disabled" = !contains(var.timer_functions, "BulkImportFunction")
    "AzureWebJobs.CancelBookingFunction.Disabled" = !contains(var.timer_functions, "CancelBookingFunction")
    "AzureWebJobs.CancelSessionFunction.Disabled" = !contains(var.timer_functions, "CancelSessionFunction")
    "AzureWebJobs.ClearLocalFeatureFlagOverridesFunction.Disabled" = !contains(var.timer_functions, "ClearLocalFeatureFlagOverridesFunction")
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled" = !contains(var.timer_functions, "ConfirmProvisionalBookingFunction")
    "AzureWebJobs.ConsentToEula.Disabled" = !contains(var.timer_functions, "ConsentToEula")
    "AzureWebJobs.GetAccessibilityDefinitionsFunction.Disabled" = !contains(var.timer_functions, "GetAccessibilityDefinitionsFunction")
    "AzureWebJobs.GetAuthTokenFunction.Disabled" = !contains(var.timer_functions, "GetAuthTokenFunction")
    "AzureWebJobs.GetAvailabilityCreatedEventsFunction.Disabled" = !contains(var.timer_functions, "GetAvailabilityCreatedEventsFunction")
    "AzureWebJobs.GetClinicalServicesFunction.Disabled" = !contains(var.timer_functions, "GetClinicalServicesFunction")
    "AzureWebJobs.GetDailyAvailabilityFunction.Disabled" = !contains(var.timer_functions, "GetDailyAvailabilityFunction")
    "AzureWebJobs.GetEulaFunction.Disabled" = !contains(var.timer_functions, "GetEulaFunction")
    "AzureWebJobs.GetFeatureFlagFunction.Disabled" = !contains(var.timer_functions, "GetFeatureFlagFunction")
    "AzureWebJobs.GetRolesFunction.Disabled" = !contains(var.timer_functions, "GetRolesFunction")
    "AzureWebJobs.GetSiteFunction.Disabled" = !contains(var.timer_functions, "GetSiteFunction")
    "AzureWebJobs.GetSiteMetaData.Disabled" = !contains(var.timer_functions, "GetSiteMetaData")
    "AzureWebJobs.GetSitesByAreaFunction.Disabled" = !contains(var.timer_functions, "GetSitesByAreaFunction")
    "AzureWebJobs.GetSitesPreviewFunction.Disabled" = !contains(var.timer_functions, "GetSitesPreviewFunction")
    "AzureWebJobs.GetPermissionsForUserFunction.Disabled" = !contains(var.timer_functions, "GetPermissionsForUserFunction")
    "AzureWebJobs.GetUserProfileFunction.Disabled" = !contains(var.timer_functions, "GetUserProfileFunction")
    "AzureWebJobs.GetUserRoleAssignmentsFunction.Disabled" = !contains(var.timer_functions, "GetUserRoleAssignmentsFunction")
    "AzureWebJobs.GetWellKnownOdsCodeEntriesFunction.Disabled" = !contains(var.timer_functions, "GetWellKnownOdsCodeEntriesFunction")
    "AzureWebJobs.MakeBookingFunction.Disabled" = !contains(var.timer_functions, "MakeBookingFunction")
    "AzureWebJobs.NotifyBookingCancelled.Disabled" = !contains(var.timer_functions, "NotifyBookingCancelled")
    "AzureWebJobs.NotifyBookingMade.Disabled" = !contains(var.timer_functions, "NotifyBookingMade")
    "AzureWebJobs.NotifyBookingReminder.Disabled" = !contains(var.timer_functions, "NotifyBookingReminder")
    "AzureWebJobs.NotifyBookingRescheduled.Disabled" = !contains(var.timer_functions, "NotifyBookingRescheduled")
    "AzureWebJobs.NotifyOktaUserRolesChanged.Disabled" = !contains(var.timer_functions, "NotifyOktaUserRolesChanged")
    "AzureWebJobs.NotifyUserRolesChanged.Disabled" = !contains(var.timer_functions, "NotifyUserRolesChanged")
    "AzureWebJobs.ProposePotentialUserFunction.Disabled" = !contains(var.timer_functions, "ProposePotentialUserFunction")
    "AzureWebJobs.QueryAvailabilityFunction.Disabled" = !contains(var.timer_functions, "QueryAvailabilityFunction")
    "AzureWebJobs.QueryBookingByNhsNumberReference.Disabled" = !contains(var.timer_functions, "QueryBookingByNhsNumberReference")
    "AzureWebJobs.QueryBookingByBookingReference.Disabled" = !contains(var.timer_functions, "QueryBookingByBookingReference")
    "AzureWebJobs.QueryBookingsFunction.Disabled" = !contains(var.timer_functions, "QueryBookingsFunction")
    "AzureWebJobs.RemoveUserFunction.Disabled" = !contains(var.timer_functions, "RemoveUserFunction")
    "AzureWebJobs.SendBookingReminders.Disabled" = !contains(var.timer_functions, "SendBookingReminders")
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled" = !contains(var.timer_functions, "RemoveUnconfirmedProvisionalBookings")
    "AzureWebJobs.SetAvailabilityFunction.Disabled" = !contains(var.timer_functions, "SetAvailabilityFunction")
    "AzureWebJobs.SetBookingStatusFunction.Disabled" = !contains(var.timer_functions, "SetBookingStatusFunction")
    "AzureWebJobs.SetLocalFeatureFlagOverrideFunction.Disabled" = !contains(var.timer_functions, "SetLocalFeatureFlagOverrideFunction")
    "AzureWebJobs.SetSiteAccessibilitiesFunction.Disabled" = !contains(var.timer_functions, "SetSiteAccessibilitiesFunction")
    "AzureWebJobs.SetSiteDetailsFunction.Disabled" = !contains(var.timer_functions, "SetSiteDetailsFunction")
    "AzureWebJobs.SetSiteInformationForCitizensFunction.Disabled" = !contains(var.timer_functions, "SetSiteInformationForCitizensFunction")
    "AzureWebJobs.SetSiteReferenceDetailsFunction.Disabled" = !contains(var.timer_functions, "SetSiteReferenceDetailsFunction")
    "AzureWebJobs.SetUserRoles.Disabled" = !contains(var.timer_functions, "SetUserRoles")
    "AzureWebJobs.TriggerBookingReminders.Disabled" = !contains(var.timer_functions, "TriggerBookingReminders")
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = !contains(var.timer_functions, "TriggerUnconfirmedProvisionalBookingsCollector")
    "AzureWebJobs.RenderOAuth2Redirect.Disabled"                           = true
    "AzureWebJobs.RenderOpenApiDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerUI.Disabled"                                = true
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
    APP_CONFIG_CONNECTION                                                  = var.app_config_connection != "" ? var.app_config_connection : azurerm_app_configuration.nbs_mya_app_configuration[0].primary_read_key[0].connection_string
    LEASE_MANAGER_CONNECTION                                               = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING                                  = azurerm_application_insights.nbs_mya_application_insights.connection_string
    Notifications_Provider                                                 = "none"
    GovNotifyBaseUri                                                       = var.gov_notify_base_uri
    GovNotifyApiKey                                                        = var.gov_notify_api_key
    BookingRemindersCronSchedule                                           = var.booking_reminders_cron_schedule
    UnconfirmedProvisionalBookingsCronSchedule                             = var.unconfirmed_provisional_bookings_cron_schedule
    SPLUNK_HOST_URL                                                        = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                                       = var.splunk_hec_token
    "AzureWebJobs.ApplyAvailabilityTemplate.Disabled" = !contains(var.timer_functions, "ApplyAvailabilityTemplate")
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled" = !contains(var.timer_functions, "AuthenticateCallbackFunction")
    "AzureWebJobs.AuthenticateFunction.Disabled" = !contains(var.timer_functions, "AuthenticateFunction")
    "AzureWebJobs.BulkImportFunction.Disabled" = !contains(var.timer_functions, "BulkImportFunction")
    "AzureWebJobs.CancelBookingFunction.Disabled" = !contains(var.timer_functions, "CancelBookingFunction")
    "AzureWebJobs.CancelSessionFunction.Disabled" = !contains(var.timer_functions, "CancelSessionFunction")
    "AzureWebJobs.ClearLocalFeatureFlagOverridesFunction.Disabled" = !contains(var.timer_functions, "ClearLocalFeatureFlagOverridesFunction")
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled" = !contains(var.timer_functions, "ConfirmProvisionalBookingFunction")
    "AzureWebJobs.ConsentToEula.Disabled" = !contains(var.timer_functions, "ConsentToEula")
    "AzureWebJobs.GetAccessibilityDefinitionsFunction.Disabled" = !contains(var.timer_functions, "GetAccessibilityDefinitionsFunction")
    "AzureWebJobs.GetAuthTokenFunction.Disabled" = !contains(var.timer_functions, "GetAuthTokenFunction")
    "AzureWebJobs.GetAvailabilityCreatedEventsFunction.Disabled" = !contains(var.timer_functions, "GetAvailabilityCreatedEventsFunction")
    "AzureWebJobs.GetClinicalServicesFunction.Disabled" = !contains(var.timer_functions, "GetClinicalServicesFunction")
    "AzureWebJobs.GetDailyAvailabilityFunction.Disabled" = !contains(var.timer_functions, "GetDailyAvailabilityFunction")
    "AzureWebJobs.GetEulaFunction.Disabled" = !contains(var.timer_functions, "GetEulaFunction")
    "AzureWebJobs.GetFeatureFlagFunction.Disabled" = !contains(var.timer_functions, "GetFeatureFlagFunction")
    "AzureWebJobs.GetRolesFunction.Disabled" = !contains(var.timer_functions, "GetRolesFunction")
    "AzureWebJobs.GetSiteFunction.Disabled" = !contains(var.timer_functions, "GetSiteFunction")
    "AzureWebJobs.GetSiteMetaData.Disabled" = !contains(var.timer_functions, "GetSiteMetaData")
    "AzureWebJobs.GetSitesByAreaFunction.Disabled" = !contains(var.timer_functions, "GetSitesByAreaFunction")
    "AzureWebJobs.GetSitesPreviewFunction.Disabled" = !contains(var.timer_functions, "GetSitesPreviewFunction")
    "AzureWebJobs.GetPermissionsForUserFunction.Disabled" = !contains(var.timer_functions, "GetPermissionsForUserFunction")
    "AzureWebJobs.GetUserProfileFunction.Disabled" = !contains(var.timer_functions, "GetUserProfileFunction")
    "AzureWebJobs.GetUserRoleAssignmentsFunction.Disabled" = !contains(var.timer_functions, "GetUserRoleAssignmentsFunction")
    "AzureWebJobs.GetWellKnownOdsCodeEntriesFunction.Disabled" = !contains(var.timer_functions, "GetWellKnownOdsCodeEntriesFunction")
    "AzureWebJobs.MakeBookingFunction.Disabled" = !contains(var.timer_functions, "MakeBookingFunction")
    "AzureWebJobs.NotifyBookingCancelled.Disabled" = !contains(var.timer_functions, "NotifyBookingCancelled")
    "AzureWebJobs.NotifyBookingMade.Disabled" = !contains(var.timer_functions, "NotifyBookingMade")
    "AzureWebJobs.NotifyBookingReminder.Disabled" = !contains(var.timer_functions, "NotifyBookingReminder")
    "AzureWebJobs.NotifyBookingRescheduled.Disabled" = !contains(var.timer_functions, "NotifyBookingRescheduled")
    "AzureWebJobs.NotifyOktaUserRolesChanged.Disabled" = !contains(var.timer_functions, "NotifyOktaUserRolesChanged")
    "AzureWebJobs.NotifyUserRolesChanged.Disabled" = !contains(var.timer_functions, "NotifyUserRolesChanged")
    "AzureWebJobs.ProposePotentialUserFunction.Disabled" = !contains(var.timer_functions, "ProposePotentialUserFunction")
    "AzureWebJobs.QueryAvailabilityFunction.Disabled" = !contains(var.timer_functions, "QueryAvailabilityFunction")
    "AzureWebJobs.QueryBookingByNhsNumberReference.Disabled" = !contains(var.timer_functions, "QueryBookingByNhsNumberReference")
    "AzureWebJobs.QueryBookingByBookingReference.Disabled" = !contains(var.timer_functions, "QueryBookingByBookingReference")
    "AzureWebJobs.QueryBookingsFunction.Disabled" = !contains(var.timer_functions, "QueryBookingsFunction")
    "AzureWebJobs.RemoveUserFunction.Disabled" = !contains(var.timer_functions, "RemoveUserFunction")
    "AzureWebJobs.SendBookingReminders.Disabled" = !contains(var.timer_functions, "SendBookingReminders")
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled" = !contains(var.timer_functions, "RemoveUnconfirmedProvisionalBookings")
    "AzureWebJobs.SetAvailabilityFunction.Disabled" = !contains(var.timer_functions, "SetAvailabilityFunction")
    "AzureWebJobs.SetBookingStatusFunction.Disabled" = !contains(var.timer_functions, "SetBookingStatusFunction")
    "AzureWebJobs.SetLocalFeatureFlagOverrideFunction.Disabled" = !contains(var.timer_functions, "SetLocalFeatureFlagOverrideFunction")
    "AzureWebJobs.SetSiteAccessibilitiesFunction.Disabled" = !contains(var.timer_functions, "SetSiteAccessibilitiesFunction")
    "AzureWebJobs.SetSiteDetailsFunction.Disabled" = !contains(var.timer_functions, "SetSiteDetailsFunction")
    "AzureWebJobs.SetSiteInformationForCitizensFunction.Disabled" = !contains(var.timer_functions, "SetSiteInformationForCitizensFunction")
    "AzureWebJobs.SetSiteReferenceDetailsFunction.Disabled" = !contains(var.timer_functions, "SetSiteReferenceDetailsFunction")
    "AzureWebJobs.SetUserRoles.Disabled" = !contains(var.timer_functions, "SetUserRoles")
    "AzureWebJobs.TriggerBookingReminders.Disabled" = !contains(var.timer_functions, "TriggerBookingReminders")
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = !contains(var.timer_functions, "TriggerUnconfirmedProvisionalBookingsCollector")
    "AzureWebJobs.RenderOAuth2Redirect.Disabled"                           = true
    "AzureWebJobs.RenderOpenApiDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerUI.Disabled"                                = true
  }

  identity {
    type = "SystemAssigned"
  }
}
