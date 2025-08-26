terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.37.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "nbs-mya-rg-int-uks"
    storage_account_name = "myatfintuks"
    container_name       = "tfstate"
    key                  = "int.tfstate"
  }

  required_version = ">= 1.6.5"
}

provider "azurerm" {
  features {
    app_configuration {
      purge_soft_delete_on_destroy = false
      recover_soft_deleted         = false
    }
  }
}

module "mya_application_int" {
  environment                                     = "int"
  location                                        = "uksouth"
  loc                                             = "uks"
  build_number                                    = var.BUILD_NUMBER
  source                                          = "../../resources"
  nhs_mail_issuer                                 = var.NHS_MAIL_ISSUER
  nhs_mail_authorize_uri                          = var.NHS_MAIL_AUTHORIZE_URI
  nhs_mail_token_uri                              = var.NHS_MAIL_TOKEN_URI
  nhs_mail_jwks_uri                               = var.NHS_MAIL_JWKS_URI
  nhs_mail_client_id                              = var.NHS_MAIL_CLIENT_ID
  nhs_mail_client_secret                          = var.NHS_MAIL_CLIENT_SECRET
  okta_issuer                                     = var.OKTA_ISSUER
  okta_authorize_uri                              = var.OKTA_AUTHORIZE_URI
  okta_token_uri                                  = var.OKTA_TOKEN_URI
  okta_jwks_uri                                   = var.OKTA_JWKS_URI
  okta_client_id                                  = var.OKTA_CLIENT_ID
  okta_client_secret                              = var.OKTA_CLIENT_SECRET
  okta_domain                                     = var.OKTA_ISSUER
  okta_management_id                              = var.OKTA_MANAGEMENT_ID
  okta_private_key_kid                            = var.OKTA_PRIVATE_KEY_KID
  okta_pem                                        = var.OKTA_PEM
  auth_provider_challenge_phrase                  = var.AUTH_PROVIDER_CHALLENGE_PHRASE
  func_app_base_uri                               = var.FUNC_APP_BASE_URI
  web_app_base_uri                                = var.WEB_APP_BASE_URI
  gov_notify_base_uri                             = var.GOV_NOTIFY_BASE_URI
  gov_notify_api_key                              = var.GOV_NOTIFY_API_KEY
  booking_reminders_cron_schedule                 = var.BOOKING_REMINDERS_CRON_SCHEDULE
  unconfirmed_provisional_bookings_cron_schedule  = var.UNCONFIRMED_PROVISIONAL_BOOKINGS_CRON_SCHEDULE
  daily_site_summary_aggregation_cron_schedule    = var.DAILY_SITE_SUMMARY_AGGREGATION_CRON_SCHEDULE
  site_summary_days_forward                       = var.SITE_SUMMARY_DAYS_FORWARD
  site_summary_days_chunk_size                    = var.SITE_SUMMARY_DAYS_CHUNK_SIZE
  site_summary_first_run_date                     = var.SITE_SUMMARY_FIRST_RUN_DATE
  splunk_hec_token                                = var.SPLUNK_HEC_TOKEN
  splunk_host_url                                 = var.SPLUNK_HOST_URL
  disable_query_availability_function             = true
  create_high_load_function_app                   = false
  create_app_slot                                 = false
  create_autoscale_settings                       = false
  create_frontdoor                                = false
  create_cosmos_db                                = true
  create_app_config                               = true
  web_app_service_sku                             = "B1"
  web_app_service_plan_default_worker_count       = 1
  app_service_plan_zone_redundancy_enabled        = false
  app_insights_sampling_percentage                = 100
  storage_account_replication_type                = "LRS"
  cosmos_automatic_failover_enabled               = false
  disable_bulk_import_function                    = false
  cosmos_geo_locations = [{
    location          = "uksouth"
    failover_priority = 0
    zone_redundant    = false
  }]
  cosmos_capabilities = [{
    name = "EnableServerless"
  }]
}
