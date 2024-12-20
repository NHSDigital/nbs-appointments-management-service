terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.5.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "nbs-mya-rg-dev-uks"
    storage_account_name = "myatfdevuks"
    container_name       = "tfstate"
    key                  = "dev.tfstate"
  }

  required_version = ">= 1.6.5"
}

provider "azurerm" {
  features {}
}

module "api" {
  environment                                    = "dev"
  source                                         = "../../resources"
  auth_provider_issuer                           = var.AUTH_PROVIDER_ISSUER
  auth_provider_authorize_uri                    = var.AUTH_PROVIDER_AUTHORIZE_URI
  auth_provider_token_uri                        = var.AUTH_PROVIDER_TOKEN_URI
  auth_provider_jwks_uri                         = var.AUTH_PROVIDER_JWKS_URI
  auth_provider_challenge_phrase                 = var.AUTH_PROVIDER_CHALLENGE_PHRASE
  auth_provider_client_id                        = var.AUTH_PROVIDER_CLIENT_ID
  auth_provider_client_secret                    = var.AUTH_PROVIDER_CLIENT_SECRET
  func_app_base_uri                              = var.FUNC_APP_BASE_URI
  web_app_base_uri                               = var.WEB_APP_BASE_URI
  gov_notify_base_uri                            = var.GOV_NOTIFY_BASE_URI
  gov_notify_api_key                             = var.GOV_NOTIFY_API_KEY
  booking_reminders_cron_schedule                = var.BOOKING_REMINDERS_CRON_SCHEDULE
  unconfirmed_provisional_bookings_cron_schedule = var.UNCONFIRMED_PROVISIONAL_BOOKINGS_CRON_SCHEDULE
  splunk_hec_token                               = var.SPLUNK_HEC_TOKEN
  splunk_host_url                                = var.SPLUNK_HOST_URL
  web_app_service_sku                            = "B1"
  web_app_service_plan_default_worker_count      = 1
  app_service_plan_zone_redundancy_enabled       = false
  do_create_autoscale_settings                   = false
  do_create_swap_slot                            = false
  app_insights_sampling_percentage               = 100
  storage_account_replication_type               = "LRS"
  cosmos_automatic_failover_enabled              = false
  cosmos_geo_locations = [{
    location          = "uksouth"
    failover_priority = 0
    zone_redundant    = false
  }]
  cosmos_capabilities = [{
    name = "EnableServerless"
  }]
}
