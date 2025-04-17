terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.5.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "nbs-mya-rg-prod-uks"
    storage_account_name = "myatfproduks"
    container_name       = "tfstate"
    key                  = "prod.tfstate"
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

module "mya_application_prod" {
  environment                                    = "prod"
  location                                       = "uksouth"
  loc                                            = "uks"
  build_number                                   = var.BUILD_NUMBER
  source                                         = "../../resources"
  nhs_mail_issuer                                = var.NHS_MAIL_ISSUER
  nhs_mail_authorize_uri                         = var.NHS_MAIL_AUTHORIZE_URI
  nhs_mail_token_uri                             = var.NHS_MAIL_TOKEN_URI
  nhs_mail_jwks_uri                              = var.NHS_MAIL_JWKS_URI
  nhs_mail_client_id                             = var.NHS_MAIL_CLIENT_ID
  nhs_mail_client_secret                         = var.NHS_MAIL_CLIENT_SECRET
  okta_issuer                                    = var.OKTA_ISSUER
  okta_authorize_uri                             = var.OKTA_AUTHORIZE_URI
  okta_token_uri                                 = var.OKTA_TOKEN_URI
  okta_jwks_uri                                  = var.OKTA_JWKS_URI
  okta_client_id                                 = var.OKTA_CLIENT_ID
  okta_client_secret                             = var.OKTA_CLIENT_SECRET
  okta_domain                                    = var.OKTA_ISSUER
  okta_management_id                             = var.OKTA_MANAGEMENT_ID
  okta_private_key_kid                           = var.OKTA_PRIVATE_KEY_KID
  okta_pem                                       = var.OKTA_PEM
  auth_provider_challenge_phrase                 = var.AUTH_PROVIDER_CHALLENGE_PHRASE
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
  create_cosmos_db                               = true
  create_app_config                              = true
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
  cosmos_synapse_enabled                         = false
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
    max_throughput = 60000
  }]
  cosmos_core_autoscale_settings = [{
    max_throughput = 25000
  }]
  cosmos_index_autoscale_settings = [{
    max_throughput = 4000
  }]
  cosmos_audit_autoscale_settings = [{
    max_throughput = 2000
  }]
}
