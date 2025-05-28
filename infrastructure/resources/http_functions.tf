# Http function app - app service and plan dedicated for running HttpTrigger functions.
# Only functions that use HttpTrigger are enabled for this function app.
# QueryAvailabilityFunction is enabled in dev and int for this service as only low loads are expected in these environments
# QueryAvailabilityFunction is disabled in staging and prod as high loads are expected in these environments

resource "azurerm_service_plan" "nbs_mya_http_func_service_plan" {
  name                = "${var.application}-fsp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = var.location
  os_type             = "Windows"
  sku_name            = "Y1"
}

resource "azurerm_windows_function_app" "nbs_mya_http_func_app" {
  name                = "${var.application}-func-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = var.location

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
    FUNCTIONS_WORKER_RUNTIME                                       = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE                                       = 1
    COSMOS_ENDPOINT                                                = var.cosmos_endpoint != "" ? var.cosmos_endpoint : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
    COSMOS_TOKEN                                                   = var.cosmos_token != "" ? var.cosmos_token : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
    APP_CONFIG_CONNECTION                                          = var.app_config_connection != "" ? var.app_config_connection : azurerm_app_configuration.nbs_mya_app_configuration[0].primary_read_key[0].connection_string
    LEASE_MANAGER_CONNECTION                                       = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING                          = azurerm_application_insights.nbs_mya_application_insights.connection_string
    Notifications_Provider                                         = "azure"
    ServiceBusConnectionString                                     = azurerm_servicebus_namespace.nbs_mya_service_bus.default_primary_connection_string
    SPLUNK_HOST_URL                                                = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                               = var.splunk_hec_token
    Auth__Providers__0__Name                                       = "nhs-mail"
    Auth__Providers__0__Issuer                                     = var.nhs_mail_issuer
    Auth__Providers__0__AuthorizeUri                               = var.nhs_mail_authorize_uri
    Auth__Providers__0__TokenUri                                   = var.nhs_mail_token_uri
    Auth__Providers__0__JwksUri                                    = var.nhs_mail_jwks_uri
    Auth__Providers__0__ChallengePhrase                            = var.auth_provider_challenge_phrase
    Auth__Providers__0__ClientId                                   = var.nhs_mail_client_id
    Auth__Providers__0__ClientSecret                               = var.nhs_mail_client_secret
    Auth__Providers__0__ClientCodeExchangeUri                      = "${local.client_code_exchange_uri}?provider=nhs-mail"
    Auth__Providers__0__ReturnUri                                  = "${local.auth_provider_return_uri}"
    Auth__Providers__1__Name                                       = "okta"
    Auth__Providers__1__Issuer                                     = var.okta_issuer
    Auth__Providers__1__AuthorizeUri                               = var.okta_authorize_uri
    Auth__Providers__1__TokenUri                                   = var.okta_token_uri
    Auth__Providers__1__JwksUri                                    = var.okta_jwks_uri
    Auth__Providers__1__ChallengePhrase                            = var.auth_provider_challenge_phrase
    Auth__Providers__1__ClientId                                   = var.okta_client_id
    Auth__Providers__1__ClientSecret                               = var.okta_client_secret
    Auth__Providers__1__ClientCodeExchangeUri                      = "${local.client_code_exchange_uri}?provider=okta"
    Auth__Providers__1__ReturnUri                                  = "${local.auth_provider_return_uri}?provider=okta"
    Auth__Providers__1__RequiresStateForAuthorize                  = true
    Okta__Domain                                                   = var.okta_domain
    Okta__ManagementId                                             = var.okta_management_id
    Okta__PrivateKeyKid                                            = var.okta_private_key_kid
    Okta__PEM                                                      = var.okta_pem
    "AzureWebJobs.ApplyAvailabilityTemplate.Disabled" = !contains(var.http_functions, "ApplyAvailabilityTemplate")
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled" = !contains(var.http_functions, "AuthenticateCallbackFunction")
    "AzureWebJobs.AuthenticateFunction.Disabled" = !contains(var.http_functions, "AuthenticateFunction")
    "AzureWebJobs.BulkImportFunction.Disabled" = !contains(var.http_functions, "BulkImportFunction")
    "AzureWebJobs.CancelBookingFunction.Disabled" = !contains(var.http_functions, "CancelBookingFunction")
    "AzureWebJobs.CancelSessionFunction.Disabled" = !contains(var.http_functions, "CancelSessionFunction")
    "AzureWebJobs.ClearLocalFeatureFlagOverridesFunction.Disabled" = !contains(var.http_functions, "ClearLocalFeatureFlagOverridesFunction")
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled" = !contains(var.http_functions, "ConfirmProvisionalBookingFunction")
    "AzureWebJobs.ConsentToEula.Disabled" = !contains(var.http_functions, "ConsentToEula")
    "AzureWebJobs.GetAccessibilityDefinitionsFunction.Disabled" = !contains(var.http_functions, "GetAccessibilityDefinitionsFunction")
    "AzureWebJobs.GetAuthTokenFunction.Disabled" = !contains(var.http_functions, "GetAuthTokenFunction")
    "AzureWebJobs.GetAvailabilityCreatedEventsFunction.Disabled" = !contains(var.http_functions, "GetAvailabilityCreatedEventsFunction")
    "AzureWebJobs.GetClinicalServicesFunction.Disabled" = !contains(var.http_functions, "GetClinicalServicesFunction")
    "AzureWebJobs.GetDailyAvailabilityFunction.Disabled" = !contains(var.http_functions, "GetDailyAvailabilityFunction")
    "AzureWebJobs.GetEulaFunction.Disabled" = !contains(var.http_functions, "GetEulaFunction")
    "AzureWebJobs.GetFeatureFlagFunction.Disabled" = !contains(var.http_functions, "GetFeatureFlagFunction")
    "AzureWebJobs.GetRolesFunction.Disabled" = !contains(var.http_functions, "GetRolesFunction")
    "AzureWebJobs.GetSiteFunction.Disabled" = !contains(var.http_functions, "GetSiteFunction")
    "AzureWebJobs.GetSiteMetaData.Disabled" = !contains(var.http_functions, "GetSiteMetaData")
    "AzureWebJobs.GetSitesByAreaFunction.Disabled" = !contains(var.http_functions, "GetSitesByAreaFunction")
    "AzureWebJobs.GetSitesPreviewFunction.Disabled" = !contains(var.http_functions, "GetSitesPreviewFunction")
    "AzureWebJobs.GetPermissionsForUserFunction.Disabled" = !contains(var.http_functions, "GetPermissionsForUserFunction")
    "AzureWebJobs.GetUserProfileFunction.Disabled" = !contains(var.http_functions, "GetUserProfileFunction")
    "AzureWebJobs.GetUserRoleAssignmentsFunction.Disabled" = !contains(var.http_functions, "GetUserRoleAssignmentsFunction")
    "AzureWebJobs.GetWellKnownOdsCodeEntriesFunction.Disabled" = !contains(var.http_functions, "GetWellKnownOdsCodeEntriesFunction")
    "AzureWebJobs.MakeBookingFunction.Disabled" = !contains(var.http_functions, "MakeBookingFunction")
    "AzureWebJobs.NotifyBookingCancelled.Disabled" = !contains(var.http_functions, "NotifyBookingCancelled")
    "AzureWebJobs.NotifyBookingMade.Disabled" = !contains(var.http_functions, "NotifyBookingMade")
    "AzureWebJobs.NotifyBookingReminder.Disabled" = !contains(var.http_functions, "NotifyBookingReminder")
    "AzureWebJobs.NotifyBookingRescheduled.Disabled" = !contains(var.http_functions, "NotifyBookingRescheduled")
    "AzureWebJobs.NotifyOktaUserRolesChanged.Disabled" = !contains(var.http_functions, "NotifyOktaUserRolesChanged")
    "AzureWebJobs.NotifyUserRolesChanged.Disabled" = !contains(var.http_functions, "NotifyUserRolesChanged")
    "AzureWebJobs.ProposePotentialUserFunction.Disabled" = !contains(var.http_functions, "ProposePotentialUserFunction")
    "AzureWebJobs.QueryAvailabilityFunction.Disabled" = !contains(var.http_functions, "QueryAvailabilityFunction")
    "AzureWebJobs.QueryBookingByNhsNumberReference.Disabled" = !contains(var.http_functions, "QueryBookingByNhsNumberReference")
    "AzureWebJobs.QueryBookingByBookingReference.Disabled" = !contains(var.http_functions, "QueryBookingByBookingReference")
    "AzureWebJobs.QueryBookingsFunction.Disabled" = !contains(var.http_functions, "QueryBookingsFunction")
    "AzureWebJobs.RemoveUserFunction.Disabled" = !contains(var.http_functions, "RemoveUserFunction")
    "AzureWebJobs.SendBookingReminders.Disabled" = !contains(var.http_functions, "SendBookingReminders")
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled" = !contains(var.http_functions, "RemoveUnconfirmedProvisionalBookings")
    "AzureWebJobs.SetAvailabilityFunction.Disabled" = !contains(var.http_functions, "SetAvailabilityFunction")
    "AzureWebJobs.SetBookingStatusFunction.Disabled" = !contains(var.http_functions, "SetBookingStatusFunction")
    "AzureWebJobs.SetLocalFeatureFlagOverrideFunction.Disabled" = !contains(var.http_functions, "SetLocalFeatureFlagOverrideFunction")
    "AzureWebJobs.SetSiteAccessibilitiesFunction.Disabled" = !contains(var.http_functions, "SetSiteAccessibilitiesFunction")
    "AzureWebJobs.SetSiteDetailsFunction.Disabled" = !contains(var.http_functions, "SetSiteDetailsFunction")
    "AzureWebJobs.SetSiteInformationForCitizensFunction.Disabled" = !contains(var.http_functions, "SetSiteInformationForCitizensFunction")
    "AzureWebJobs.SetSiteReferenceDetailsFunction.Disabled" = !contains(var.http_functions, "SetSiteReferenceDetailsFunction")
    "AzureWebJobs.SetUserRoles.Disabled" = !contains(var.http_functions, "SetUserRoles")
    "AzureWebJobs.TriggerBookingReminders.Disabled" = !contains(var.http_functions, "TriggerBookingReminders")
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = !contains(var.http_functions, "TriggerUnconfirmedProvisionalBookingsCollector")
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
    FUNCTIONS_WORKER_RUNTIME                                       = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE                                       = 1
    COSMOS_ENDPOINT                                                = azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
    COSMOS_TOKEN                                                   = azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
    APP_CONFIG_CONNECTION                                          = var.app_config_connection != "" ? var.app_config_connection : azurerm_app_configuration.nbs_mya_app_configuration[0].primary_read_key[0].connection_string
    LEASE_MANAGER_CONNECTION                                       = azurerm_storage_account.nbs_mya_leases_storage_account.primary_blob_connection_string
    APPLICATIONINSIGHTS_CONNECTION_STRING                          = azurerm_application_insights.nbs_mya_application_insights.connection_string
    Notifications_Provider                                         = "none"
    SPLUNK_HOST_URL                                                = var.splunk_host_url
    SPLUNK_HEC_TOKEN                                               = var.splunk_hec_token
    Auth__Providers__0__Name                                       = "nhs-mail"
    Auth__Providers__0__Issuer                                     = var.nhs_mail_issuer
    Auth__Providers__0__AuthorizeUri                               = var.nhs_mail_authorize_uri
    Auth__Providers__0__TokenUri                                   = var.nhs_mail_token_uri
    Auth__Providers__0__JwksUri                                    = var.nhs_mail_jwks_uri
    Auth__Providers__0__ChallengePhrase                            = var.auth_provider_challenge_phrase
    Auth__Providers__0__ClientId                                   = var.nhs_mail_client_id
    Auth__Providers__0__ClientSecret                               = var.nhs_mail_client_secret
    Auth__Providers__0__ClientCodeExchangeUri                      = "${var.web_app_slot_base_uri}/manage-your-appointments/auth/set-cookie?provider=nhs-mail"
    Auth__Providers__0__ReturnUri                                  = "${var.func_app_slot_base_uri}/api/auth-return"
    Auth__Providers__1__Name                                       = "okta"
    Auth__Providers__1__Issuer                                     = var.okta_issuer
    Auth__Providers__1__AuthorizeUri                               = var.okta_authorize_uri
    Auth__Providers__1__TokenUri                                   = var.okta_token_uri
    Auth__Providers__1__JwksUri                                    = var.okta_jwks_uri
    Auth__Providers__1__ChallengePhrase                            = var.auth_provider_challenge_phrase
    Auth__Providers__1__ClientId                                   = var.okta_client_id
    Auth__Providers__1__ClientSecret                               = var.okta_client_secret
    Auth__Providers__1__ClientCodeExchangeUri                      = "${var.web_app_slot_base_uri}/manage-your-appointments/auth/set-cookie?provider=okta"
    Auth__Providers__1__ReturnUri                                  = "${var.func_app_slot_base_uri}/api/auth-return?provider=okta"
    Auth__Providers__1__RequiresStateForAuthorize                  = true
    Okta__Domain                                                   = var.okta_domain
    Okta__ManagementId                                             = var.okta_management_id
    Okta__PrivateKeyKid                                            = var.okta_private_key_kid
    Okta__PEM                                                      = var.okta_pem
    "AzureWebJobs.ApplyAvailabilityTemplate.Disabled" = !contains(var.http_functions, "ApplyAvailabilityTemplate")
    "AzureWebJobs.AuthenticateCallbackFunction.Disabled" = !contains(var.http_functions, "AuthenticateCallbackFunction")
    "AzureWebJobs.AuthenticateFunction.Disabled" = !contains(var.http_functions, "AuthenticateFunction")
    "AzureWebJobs.BulkImportFunction.Disabled" = !contains(var.http_functions, "BulkImportFunction")
    "AzureWebJobs.CancelBookingFunction.Disabled" = !contains(var.http_functions, "CancelBookingFunction")
    "AzureWebJobs.CancelSessionFunction.Disabled" = !contains(var.http_functions, "CancelSessionFunction")
    "AzureWebJobs.ClearLocalFeatureFlagOverridesFunction.Disabled" = !contains(var.http_functions, "ClearLocalFeatureFlagOverridesFunction")
    "AzureWebJobs.ConfirmProvisionalBookingFunction.Disabled" = !contains(var.http_functions, "ConfirmProvisionalBookingFunction")
    "AzureWebJobs.ConsentToEula.Disabled" = !contains(var.http_functions, "ConsentToEula")
    "AzureWebJobs.GetAccessibilityDefinitionsFunction.Disabled" = !contains(var.http_functions, "GetAccessibilityDefinitionsFunction")
    "AzureWebJobs.GetAuthTokenFunction.Disabled" = !contains(var.http_functions, "GetAuthTokenFunction")
    "AzureWebJobs.GetAvailabilityCreatedEventsFunction.Disabled" = !contains(var.http_functions, "GetAvailabilityCreatedEventsFunction")
    "AzureWebJobs.GetClinicalServicesFunction.Disabled" = !contains(var.http_functions, "GetClinicalServicesFunction")
    "AzureWebJobs.GetDailyAvailabilityFunction.Disabled" = !contains(var.http_functions, "GetDailyAvailabilityFunction")
    "AzureWebJobs.GetEulaFunction.Disabled" = !contains(var.http_functions, "GetEulaFunction")
    "AzureWebJobs.GetFeatureFlagFunction.Disabled" = !contains(var.http_functions, "GetFeatureFlagFunction")
    "AzureWebJobs.GetRolesFunction.Disabled" = !contains(var.http_functions, "GetRolesFunction")
    "AzureWebJobs.GetSiteFunction.Disabled" = !contains(var.http_functions, "GetSiteFunction")
    "AzureWebJobs.GetSiteMetaData.Disabled" = !contains(var.http_functions, "GetSiteMetaData")
    "AzureWebJobs.GetSitesByAreaFunction.Disabled" = !contains(var.http_functions, "GetSitesByAreaFunction")
    "AzureWebJobs.GetSitesPreviewFunction.Disabled" = !contains(var.http_functions, "GetSitesPreviewFunction")
    "AzureWebJobs.GetPermissionsForUserFunction.Disabled" = !contains(var.http_functions, "GetPermissionsForUserFunction")
    "AzureWebJobs.GetUserProfileFunction.Disabled" = !contains(var.http_functions, "GetUserProfileFunction")
    "AzureWebJobs.GetUserRoleAssignmentsFunction.Disabled" = !contains(var.http_functions, "GetUserRoleAssignmentsFunction")
    "AzureWebJobs.GetWellKnownOdsCodeEntriesFunction.Disabled" = !contains(var.http_functions, "GetWellKnownOdsCodeEntriesFunction")
    "AzureWebJobs.MakeBookingFunction.Disabled" = !contains(var.http_functions, "MakeBookingFunction")
    "AzureWebJobs.NotifyBookingCancelled.Disabled" = !contains(var.http_functions, "NotifyBookingCancelled")
    "AzureWebJobs.NotifyBookingMade.Disabled" = !contains(var.http_functions, "NotifyBookingMade")
    "AzureWebJobs.NotifyBookingReminder.Disabled" = !contains(var.http_functions, "NotifyBookingReminder")
    "AzureWebJobs.NotifyBookingRescheduled.Disabled" = !contains(var.http_functions, "NotifyBookingRescheduled")
    "AzureWebJobs.NotifyOktaUserRolesChanged.Disabled" = !contains(var.http_functions, "NotifyOktaUserRolesChanged")
    "AzureWebJobs.NotifyUserRolesChanged.Disabled" = !contains(var.http_functions, "NotifyUserRolesChanged")
    "AzureWebJobs.ProposePotentialUserFunction.Disabled" = !contains(var.http_functions, "ProposePotentialUserFunction")
    "AzureWebJobs.QueryAvailabilityFunction.Disabled" = !contains(var.http_functions, "QueryAvailabilityFunction")
    "AzureWebJobs.QueryBookingByNhsNumberReference.Disabled" = !contains(var.http_functions, "QueryBookingByNhsNumberReference")
    "AzureWebJobs.QueryBookingByBookingReference.Disabled" = !contains(var.http_functions, "QueryBookingByBookingReference")
    "AzureWebJobs.QueryBookingsFunction.Disabled" = !contains(var.http_functions, "QueryBookingsFunction")
    "AzureWebJobs.RemoveUserFunction.Disabled" = !contains(var.http_functions, "RemoveUserFunction")
    "AzureWebJobs.SendBookingReminders.Disabled" = !contains(var.http_functions, "SendBookingReminders")
    "AzureWebJobs.RemoveUnconfirmedProvisionalBookings.Disabled" = !contains(var.http_functions, "RemoveUnconfirmedProvisionalBookings")
    "AzureWebJobs.SetAvailabilityFunction.Disabled" = !contains(var.http_functions, "SetAvailabilityFunction")
    "AzureWebJobs.SetBookingStatusFunction.Disabled" = !contains(var.http_functions, "SetBookingStatusFunction")
    "AzureWebJobs.SetLocalFeatureFlagOverrideFunction.Disabled" = !contains(var.http_functions, "SetLocalFeatureFlagOverrideFunction")
    "AzureWebJobs.SetSiteAccessibilitiesFunction.Disabled" = !contains(var.http_functions, "SetSiteAccessibilitiesFunction")
    "AzureWebJobs.SetSiteDetailsFunction.Disabled" = !contains(var.http_functions, "SetSiteDetailsFunction")
    "AzureWebJobs.SetSiteInformationForCitizensFunction.Disabled" = !contains(var.http_functions, "SetSiteInformationForCitizensFunction")
    "AzureWebJobs.SetSiteReferenceDetailsFunction.Disabled" = !contains(var.http_functions, "SetSiteReferenceDetailsFunction")
    "AzureWebJobs.SetUserRoles.Disabled" = !contains(var.http_functions, "SetUserRoles")
    "AzureWebJobs.TriggerBookingReminders.Disabled" = !contains(var.http_functions, "TriggerBookingReminders")
    "AzureWebJobs.TriggerUnconfirmedProvisionalBookingsCollector.Disabled" = !contains(var.http_functions, "TriggerUnconfirmedProvisionalBookingsCollector")
  }

  identity {
    type = "SystemAssigned"
  }
}
