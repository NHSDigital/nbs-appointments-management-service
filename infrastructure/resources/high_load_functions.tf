# High load function app - app service and plan dedicated for running high CPU load functions. 
# Functions enabled: QueryAvailabilityFunction

# This function app is only created in staging and production

resource "azurerm_service_plan" "nbs_mya_high_load_func_service_plan" {
  count               = var.create_high_load_function_app ? 1 : 0
  name                = "${var.application}-hlfsp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
  os_type             = "Windows"
  sku_name            = "Y1"
}

resource "azurerm_windows_function_app" "nbs_mya_high_load_func_app" {
  count               = var.create_high_load_function_app ? 1 : 0
  name                = "${var.application}-hlfunc-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location

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
    COSMOS_ENDPOINT                                                        = azurerm_cosmosdb_account.nbs_mya_cosmos_db.endpoint
    COSMOS_TOKEN                                                           = azurerm_cosmosdb_account.nbs_mya_cosmos_db.primary_key
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
    "AzureWebJobs.SendBookingReminders.Disabled"                           = true
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled"           = true
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
  count                      = var.create_high_load_function_app && var.create_app_slot ? 1 : 0
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
    COSMOS_ENDPOINT                                                        = azurerm_cosmosdb_account.nbs_mya_cosmos_db.endpoint
    COSMOS_TOKEN                                                           = azurerm_cosmosdb_account.nbs_mya_cosmos_db.primary_key
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
  }

  identity {
    type = "SystemAssigned"
  }
}