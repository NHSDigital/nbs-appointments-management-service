# High load function app - app service and plan dedicated for running high CPU load functions. 
# Functions enabled: QueryAvailabilityFunction

# This function app is only created in staging and production

resource "azurerm_service_plan" "nbs_mya_high_load_func_service_plan" {
  count               = length(var.high_load_functions) > 0 ? 1 : 0
  name                = "${var.application}-hlfsp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = var.location
  os_type             = "Windows"
  sku_name            = "Y1"
}

resource "azurerm_windows_function_app" "nbs_mya_high_load_func_app" {
  count               = length(var.high_load_functions) > 0 ? 1 : 0
  name                = "${var.application}-hlfunc-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = var.location

  storage_account_name       = azurerm_storage_account.nbs_mya_high_load_func_storage_account[0].name
  storage_account_access_key = azurerm_storage_account.nbs_mya_high_load_func_storage_account[0].primary_access_key
  service_plan_id            = azurerm_service_plan.nbs_mya_high_load_func_service_plan[0].id

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
    Notifications_Provider                                                 = "none"
    SPLUNK_HOST_URL                                                        = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                                       = var.splunk_hec_token
    Auth__Providers__0__Name                                               = "nhs-mail"
    Auth__Providers__0__Issuer                                             = var.nhs_mail_issuer
    Auth__Providers__0__AuthorizeUri                                       = var.nhs_mail_authorize_uri
    Auth__Providers__0__TokenUri                                           = var.nhs_mail_token_uri
    Auth__Providers__0__JwksUri                                            = var.nhs_mail_jwks_uri
    Auth__Providers__0__ChallengePhrase                                    = var.auth_provider_challenge_phrase
    Auth__Providers__0__ClientId                                           = var.nhs_mail_client_id
    Auth__Providers__0__ClientSecret                                       = var.nhs_mail_client_secret
    Auth__Providers__0__ClientCodeExchangeUri                              = "${local.client_code_exchange_uri}?provider=nhs-mail"
    Auth__Providers__0__ReturnUri                                          = "${local.auth_provider_return_uri}"
    Auth__Providers__1__Name                                               = "okta"
    Auth__Providers__1__Issuer                                             = var.okta_issuer
    Auth__Providers__1__AuthorizeUri                                       = var.okta_authorize_uri
    Auth__Providers__1__TokenUri                                           = var.okta_token_uri
    Auth__Providers__1__JwksUri                                            = var.okta_jwks_uri
    Auth__Providers__1__ChallengePhrase                                    = var.auth_provider_challenge_phrase
    Auth__Providers__1__ClientId                                           = var.okta_client_id
    Auth__Providers__1__ClientSecret                                       = var.okta_client_secret
    Auth__Providers__1__ClientCodeExchangeUri                              = "${local.client_code_exchange_uri}?provider=okta"
    Auth__Providers__1__ReturnUri                                          = "${local.auth_provider_return_uri}?provider=okta"
    Auth__Providers__1__RequiresStateForAuthorize                          = true
    "AzureWebJobs.ApplyAvailabilityTemplate.Disabled"                      = !contains(var.high_load_functions, "ApplyAvailabilityTemplate")
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled"                   = !contains(var.high_load_functions, "AuthenticateCallbackFunction")
    "AzureWebJobs.AuthenticateFunction.Disabled"                           = !contains(var.high_load_functions, "AuthenticateFunction")
    "AzureWebJobs.BulkImportFunction.Disabled"                             = !contains(var.high_load_functions, "BulkImportFunction")
    "AzureWebJobs.CancelBookingFunction.Disabled"                          = !contains(var.high_load_functions, "CancelBookingFunction")
    "AzureWebJobs.CancelSessionFunction.Disabled"                          = !contains(var.high_load_functions, "CancelSessionFunction")
    "AzureWebJobs.ClearLocalFeatureFlagOverridesFunction.Disabled"         = !contains(var.high_load_functions, "ClearLocalFeatureFlagOverridesFunction")
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled"              = !contains(var.high_load_functions, "ConfirmProvisionalBookingFunction")
    "AzureWebJobs.ConsentToEula.Disabled"                                  = !contains(var.high_load_functions, "ConsentToEula")
    "AzureWebJobs.GetAccessibilityDefinitionsFunction.Disabled"            = !contains(var.high_load_functions, "GetAccessibilityDefinitionsFunction")
    "AzureWebJobs.GetAuthTokenFunction.Disabled"                           = !contains(var.high_load_functions, "GetAuthTokenFunction")
    "AzureWebJobs.GetAvailabilityCreatedEventsFunction.Disabled"           = !contains(var.high_load_functions, "GetAvailabilityCreatedEventsFunction")
    "AzureWebJobs.GetClinicalServicesFunction.Disabled"                    = !contains(var.high_load_functions, "GetClinicalServicesFunction")
    "AzureWebJobs.GetDailyAvailabilityFunction.Disabled"                   = !contains(var.high_load_functions, "GetDailyAvailabilityFunction")
    "AzureWebJobs.GetEulaFunction.Disabled"                                = !contains(var.high_load_functions, "GetEulaFunction")
    "AzureWebJobs.GetFeatureFlagFunction.Disabled"                         = !contains(var.high_load_functions, "GetFeatureFlagFunction")
    "AzureWebJobs.GetRolesFunction.Disabled"                               = !contains(var.high_load_functions, "GetRolesFunction")
    "AzureWebJobs.GetSiteFunction.Disabled"                                = !contains(var.high_load_functions, "GetSiteFunction")
    "AzureWebJobs.GetSiteMetaData.Disabled"                                = !contains(var.high_load_functions, "GetSiteMetaData")
    "AzureWebJobs.GetSitesByAreaFunction.Disabled"                         = !contains(var.high_load_functions, "GetSitesByAreaFunction")
    "AzureWebJobs.GetSitesPreviewFunction.Disabled"                        = !contains(var.high_load_functions, "GetSitesPreviewFunction")
    "AzureWebJobs.GetPermissionsForUserFunction.Disabled"                  = !contains(var.high_load_functions, "GetPermissionsForUserFunction")
    "AzureWebJobs.GetUserProfileFunction.Disabled"                         = !contains(var.high_load_functions, "GetUserProfileFunction")
    "AzureWebJobs.GetUserRoleAssignmentsFunction.Disabled"                 = !contains(var.high_load_functions, "GetUserRoleAssignmentsFunction")
    "AzureWebJobs.GetWellKnownOdsCodeEntriesFunction.Disabled"             = !contains(var.high_load_functions, "GetWellKnownOdsCodeEntriesFunction")
    "AzureWebJobs.MakeBookingFunction.Disabled"                            = !contains(var.high_load_functions, "MakeBookingFunction")
    "AzureWebJobs.NotifyBookingCancelled.Disabled"                         = !contains(var.high_load_functions, "NotifyBookingCancelled")
    "AzureWebJobs.NotifyBookingMade.Disabled"                              = !contains(var.high_load_functions, "NotifyBookingMade")
    "AzureWebJobs.NotifyBookingReminder.Disabled"                          = !contains(var.high_load_functions, "NotifyBookingReminder")
    "AzureWebJobs.NotifyBookingRescheduled.Disabled"                       = !contains(var.high_load_functions, "NotifyBookingRescheduled")
    "AzureWebJobs.NotifyOktaUserRolesChanged.Disabled"                     = !contains(var.high_load_functions, "NotifyOktaUserRolesChanged")
    "AzureWebJobs.NotifyUserRolesChanged.Disabled"                         = !contains(var.high_load_functions, "NotifyUserRolesChanged")
    "AzureWebJobs.ProposePotentialUserFunction.Disabled"                   = !contains(var.high_load_functions, "ProposePotentialUserFunction")
    "AzureWebJobs.QueryAvailabilityFunction.Disabled"                      = !contains(var.high_load_functions, "QueryAvailabilityFunction")
    "AzureWebJobs.QueryBookingByNhsNumberReference.Disabled"               = !contains(var.high_load_functions, "QueryBookingByNhsNumberReference")
    "AzureWebJobs.QueryBookingByBookingReference.Disabled"                 = !contains(var.high_load_functions, "QueryBookingByBookingReference")
    "AzureWebJobs.QueryBookingsFunction.Disabled"                          = !contains(var.high_load_functions, "QueryBookingsFunction")
    "AzureWebJobs.RemoveUserFunction.Disabled"                             = !contains(var.high_load_functions, "RemoveUserFunction")
    "AzureWebJobs.SendBookingReminders.Disabled"                           = !contains(var.high_load_functions, "SendBookingReminders")
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled"           = !contains(var.high_load_functions, "RemoveUnconfirmedProvisionalBookings")
    "AzureWebJobs.SetAvailabilityFunction.Disabled"                        = !contains(var.high_load_functions, "SetAvailabilityFunction")
    "AzureWebJobs.SetBookingStatusFunction.Disabled"                       = !contains(var.high_load_functions, "SetBookingStatusFunction")
    "AzureWebJobs.SetLocalFeatureFlagOverrideFunction.Disabled"            = !contains(var.high_load_functions, "SetLocalFeatureFlagOverrideFunction")
    "AzureWebJobs.SetSiteAccessibilitiesFunction.Disabled"                 = !contains(var.high_load_functions, "SetSiteAccessibilitiesFunction")
    "AzureWebJobs.SetSiteDetailsFunction.Disabled"                         = !contains(var.high_load_functions, "SetSiteDetailsFunction")
    "AzureWebJobs.SetSiteInformationForCitizensFunction.Disabled"          = !contains(var.high_load_functions, "SetSiteInformationForCitizensFunction")
    "AzureWebJobs.SetSiteReferenceDetailsFunction.Disabled"                = !contains(var.high_load_functions, "SetSiteReferenceDetailsFunction")
    "AzureWebJobs.SetUserRoles.Disabled"                                   = !contains(var.high_load_functions, "SetUserRoles")
    "AzureWebJobs.TriggerBookingReminders.Disabled"                        = !contains(var.high_load_functions, "TriggerBookingReminders")
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = !contains(var.high_load_functions, "TriggerUnconfirmedProvisionalBookingsCollector")
    "AzureWebJobs.RenderOAuth2Redirect.Disabled"                           = true
    "AzureWebJobs.RenderOpenApiDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerUI.Disabled"                                = true
  }

  sticky_settings {
    app_setting_names = [
      "Auth__Providers__1__ClientCodeExchangeUri",
      "Auth__Providers__1__ReturnUri",
      "Auth__Providers__0__ClientCodeExchangeUri",
      "Auth__Providers__0__ReturnUri",
      "Notifications_Provider",
      "ServiceBusConnectionString"
    ]
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_windows_function_app_slot" "nbs_mya_high_load_func_app_preview" {
  count                      = length(var.high_load_functions) > 0 && var.create_app_slot ? 1 : 0
  name                       = "preview"
  function_app_id            = azurerm_windows_function_app.nbs_mya_high_load_func_app[0].id
  storage_account_name       = azurerm_storage_account.nbs_mya_high_load_func_storage_account[0].name
  storage_account_access_key = azurerm_storage_account.nbs_mya_high_load_func_storage_account[0].primary_access_key

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
    SPLUNK_HOST_URL                                                        = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                                       = var.splunk_hec_token
    Auth__Providers__0__Name                                               = "nhs-mail"
    Auth__Providers__0__Issuer                                             = var.nhs_mail_issuer
    Auth__Providers__0__AuthorizeUri                                       = var.nhs_mail_authorize_uri
    Auth__Providers__0__TokenUri                                           = var.nhs_mail_token_uri
    Auth__Providers__0__JwksUri                                            = var.nhs_mail_jwks_uri
    Auth__Providers__0__ChallengePhrase                                    = var.auth_provider_challenge_phrase
    Auth__Providers__0__ClientId                                           = var.nhs_mail_client_id
    Auth__Providers__0__ClientSecret                                       = var.nhs_mail_client_secret
    Auth__Providers__0__ClientCodeExchangeUri                              = "${local.client_code_exchange_uri}?provider=nhs-mail"
    Auth__Providers__0__ReturnUri                                          = "${local.auth_provider_return_uri}"
    Auth__Providers__1__Name                                               = "okta"
    Auth__Providers__1__Issuer                                             = var.okta_issuer
    Auth__Providers__1__AuthorizeUri                                       = var.okta_authorize_uri
    Auth__Providers__1__TokenUri                                           = var.okta_token_uri
    Auth__Providers__1__JwksUri                                            = var.okta_jwks_uri
    Auth__Providers__1__ChallengePhrase                                    = var.auth_provider_challenge_phrase
    Auth__Providers__1__ClientId                                           = var.okta_client_id
    Auth__Providers__1__ClientSecret                                       = var.okta_client_secret
    Auth__Providers__1__ClientCodeExchangeUri                              = "${local.client_code_exchange_uri}?provider=okta"
    Auth__Providers__1__ReturnUri                                          = "${local.auth_provider_return_uri}?provider=okta"
    Auth__Providers__1__RequiresStateForAuthorize                          = true
    "AzureWebJobs.ApplyAvailabilityTemplate.Disabled"                      = !contains(var.high_load_functions, "ApplyAvailabilityTemplate")
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled"                   = !contains(var.high_load_functions, "AuthenticateCallbackFunction")
    "AzureWebJobs.AuthenticateFunction.Disabled"                           = !contains(var.high_load_functions, "AuthenticateFunction")
    "AzureWebJobs.BulkImportFunction.Disabled"                             = !contains(var.high_load_functions, "BulkImportFunction")
    "AzureWebJobs.CancelBookingFunction.Disabled"                          = !contains(var.high_load_functions, "CancelBookingFunction")
    "AzureWebJobs.CancelSessionFunction.Disabled"                          = !contains(var.high_load_functions, "CancelSessionFunction")
    "AzureWebJobs.ClearLocalFeatureFlagOverridesFunction.Disabled"         = !contains(var.high_load_functions, "ClearLocalFeatureFlagOverridesFunction")
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled"              = !contains(var.high_load_functions, "ConfirmProvisionalBookingFunction")
    "AzureWebJobs.ConsentToEula.Disabled"                                  = !contains(var.high_load_functions, "ConsentToEula")
    "AzureWebJobs.GetAccessibilityDefinitionsFunction.Disabled"            = !contains(var.high_load_functions, "GetAccessibilityDefinitionsFunction")
    "AzureWebJobs.GetAuthTokenFunction.Disabled"                           = !contains(var.high_load_functions, "GetAuthTokenFunction")
    "AzureWebJobs.GetAvailabilityCreatedEventsFunction.Disabled"           = !contains(var.high_load_functions, "GetAvailabilityCreatedEventsFunction")
    "AzureWebJobs.GetClinicalServicesFunction.Disabled"                    = !contains(var.high_load_functions, "GetClinicalServicesFunction")
    "AzureWebJobs.GetDailyAvailabilityFunction.Disabled"                   = !contains(var.high_load_functions, "GetDailyAvailabilityFunction")
    "AzureWebJobs.GetEulaFunction.Disabled"                                = !contains(var.high_load_functions, "GetEulaFunction")
    "AzureWebJobs.GetFeatureFlagFunction.Disabled"                         = !contains(var.high_load_functions, "GetFeatureFlagFunction")
    "AzureWebJobs.GetRolesFunction.Disabled"                               = !contains(var.high_load_functions, "GetRolesFunction")
    "AzureWebJobs.GetSiteFunction.Disabled"                                = !contains(var.high_load_functions, "GetSiteFunction")
    "AzureWebJobs.GetSiteMetaData.Disabled"                                = !contains(var.high_load_functions, "GetSiteMetaData")
    "AzureWebJobs.GetSitesByAreaFunction.Disabled"                         = !contains(var.high_load_functions, "GetSitesByAreaFunction")
    "AzureWebJobs.GetSitesPreviewFunction.Disabled"                        = !contains(var.high_load_functions, "GetSitesPreviewFunction")
    "AzureWebJobs.GetPermissionsForUserFunction.Disabled"                  = !contains(var.high_load_functions, "GetPermissionsForUserFunction")
    "AzureWebJobs.GetUserProfileFunction.Disabled"                         = !contains(var.high_load_functions, "GetUserProfileFunction")
    "AzureWebJobs.GetUserRoleAssignmentsFunction.Disabled"                 = !contains(var.high_load_functions, "GetUserRoleAssignmentsFunction")
    "AzureWebJobs.GetWellKnownOdsCodeEntriesFunction.Disabled"             = !contains(var.high_load_functions, "GetWellKnownOdsCodeEntriesFunction")
    "AzureWebJobs.MakeBookingFunction.Disabled"                            = !contains(var.high_load_functions, "MakeBookingFunction")
    "AzureWebJobs.NotifyBookingCancelled.Disabled"                         = !contains(var.high_load_functions, "NotifyBookingCancelled")
    "AzureWebJobs.NotifyBookingMade.Disabled"                              = !contains(var.high_load_functions, "NotifyBookingMade")
    "AzureWebJobs.NotifyBookingReminder.Disabled"                          = !contains(var.high_load_functions, "NotifyBookingReminder")
    "AzureWebJobs.NotifyBookingRescheduled.Disabled"                       = !contains(var.high_load_functions, "NotifyBookingRescheduled")
    "AzureWebJobs.NotifyOktaUserRolesChanged.Disabled"                     = !contains(var.high_load_functions, "NotifyOktaUserRolesChanged")
    "AzureWebJobs.NotifyUserRolesChanged.Disabled"                         = !contains(var.high_load_functions, "NotifyUserRolesChanged")
    "AzureWebJobs.ProposePotentialUserFunction.Disabled"                   = !contains(var.high_load_functions, "ProposePotentialUserFunction")
    "AzureWebJobs.QueryAvailabilityFunction.Disabled"                      = !contains(var.high_load_functions, "QueryAvailabilityFunction")
    "AzureWebJobs.QueryBookingByNhsNumberReference.Disabled"               = !contains(var.high_load_functions, "QueryBookingByNhsNumberReference")
    "AzureWebJobs.QueryBookingByBookingReference.Disabled"                 = !contains(var.high_load_functions, "QueryBookingByBookingReference")
    "AzureWebJobs.QueryBookingsFunction.Disabled"                          = !contains(var.high_load_functions, "QueryBookingsFunction")
    "AzureWebJobs.RemoveUserFunction.Disabled"                             = !contains(var.high_load_functions, "RemoveUserFunction")
    "AzureWebJobs.SendBookingReminders.Disabled"                           = !contains(var.high_load_functions, "SendBookingReminders")
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled"           = !contains(var.high_load_functions, "RemoveUnconfirmedProvisionalBookings")
    "AzureWebJobs.SetAvailabilityFunction.Disabled"                        = !contains(var.high_load_functions, "SetAvailabilityFunction")
    "AzureWebJobs.SetBookingStatusFunction.Disabled"                       = !contains(var.high_load_functions, "SetBookingStatusFunction")
    "AzureWebJobs.SetLocalFeatureFlagOverrideFunction.Disabled"            = !contains(var.high_load_functions, "SetLocalFeatureFlagOverrideFunction")
    "AzureWebJobs.SetSiteAccessibilitiesFunction.Disabled"                 = !contains(var.high_load_functions, "SetSiteAccessibilitiesFunction")
    "AzureWebJobs.SetSiteDetailsFunction.Disabled"                         = !contains(var.high_load_functions, "SetSiteDetailsFunction")
    "AzureWebJobs.SetSiteInformationForCitizensFunction.Disabled"          = !contains(var.high_load_functions, "SetSiteInformationForCitizensFunction")
    "AzureWebJobs.SetSiteReferenceDetailsFunction.Disabled"                = !contains(var.high_load_functions, "SetSiteReferenceDetailsFunction")
    "AzureWebJobs.SetUserRoles.Disabled"                                   = !contains(var.high_load_functions, "SetUserRoles")
    "AzureWebJobs.TriggerBookingReminders.Disabled"                        = !contains(var.high_load_functions, "TriggerBookingReminders")
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = !contains(var.high_load_functions, "TriggerUnconfirmedProvisionalBookingsCollector")
    "AzureWebJobs.RenderOAuth2Redirect.Disabled"                           = true
    "AzureWebJobs.RenderOpenApiDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerDocument.Disabled"                          = true
    "AzureWebJobs.RenderSwaggerUI.Disabled"                                = true
  }

  identity {
    type = "SystemAssigned"
  }
}
