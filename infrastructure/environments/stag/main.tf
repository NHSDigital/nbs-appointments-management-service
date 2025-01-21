terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.5.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "nbs-mya-rg-stag-uks"
    storage_account_name = "myatfstaguks"
    container_name       = "tfstate"
    key                  = "stag.tfstate"
  }

  required_version = ">= 1.6.5"
}

provider "azurerm" {
  features {}
}

module "mya_application_stag" {
  environment                                    = "stag"
  source                                         = "../../resources"
  auth_provider_issuer                           = var.AUTH_PROVIDER_ISSUER
  auth_provider_authorize_uri                    = var.AUTH_PROVIDER_AUTHORIZE_URI
  auth_provider_token_uri                        = var.AUTH_PROVIDER_TOKEN_URI
  auth_provider_jwks_uri                         = var.AUTH_PROVIDER_JWKS_URI
  auth_provider_challenge_phrase                 = var.AUTH_PROVIDER_CHALLENGE_PHRASE
  auth_provider_client_id                        = var.AUTH_PROVIDER_CLIENT_ID
  auth_provider_client_secret                    = var.AUTH_PROVIDER_CLIENT_SECRET
  nhs_host_url                                   = var.NHS_HOST_URL
  func_app_base_uri                              = var.FUNC_APP_BASE_URI
  func_app_slot_base_uri                         = var.FUNC_APP_SLOT_BASE_URI
  web_app_base_uri                               = var.WEB_APP_BASE_URI
  web_app_slot_base_uri                          = var.WEB_APP_SLOT_BASE_URI
  gov_notify_base_uri                            = var.GOV_NOTIFY_BASE_URI
  gov_notify_api_key                             = var.GOV_NOTIFY_API_KEY
  booking_reminders_cron_schedule                = var.BOOKING_REMINDERS_CRON_SCHEDULE
  unconfirmed_provisional_bookings_cron_schedule = var.UNCONFIRMED_PROVISIONAL_BOOKINGS_CRON_SCHEDULE
  splunk_hec_token                               = var.SPLUNK_HEC_TOKEN
  splunk_host_url                                = var.SPLUNK_HOST_URL
  autoscale_notification_email_address           = var.AUTOSCALE_NOTIFICATION_EMAIL_ADDRESS
  disable_query_availability_function            = true
  create_high_load_function_app                  = true
  create_app_slot                                = true
  create_autoscale_settings                      = true
  create_frontdoor                               = true
  web_app_service_sku                            = "P2v3"
  web_app_service_plan_default_worker_count      = 3
  app_service_plan_zone_redundancy_enabled       = true
  web_app_service_plan_min_worker_count          = 1
  web_app_service_plan_max_worker_count          = 20
  web_app_service_plan_scale_out_worker_count    = 1
  web_app_service_plan_scale_in_worker_count     = 1
  app_insights_sampling_percentage               = 12.5
  storage_account_replication_type               = "ZRS"
  cosmos_automatic_failover_enabled              = true
  cosmos_geo_locations = [{
    location          = "uksouth"
    failover_priority = 0
    zone_redundant    = true
    },
    {
      location          = "ukwest"
      failover_priority = 1
      zone_redundant    = false
  }]
  cosmos_booking_autoscale_settings = [{
    max_throughput = 30000
  }]
  cosmos_core_autoscale_settings = [{
    max_throughput = 20000
  }]
  cosmos_index_autoscale_settings = [{
    max_throughput = 4000
  }]
  cosmos_audit_autoscale_settings = [{
    max_throughput = 4000
  }]
}
