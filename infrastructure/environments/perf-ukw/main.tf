terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.37.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "nbs-myaperf-rg-stag-ukw"
    storage_account_name = "myaperftfstagukw"
    container_name       = "tfstate"
    key                  = "perf.tfstate"
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

module "mya_application_perf" {
  environment                                     = "perf"
  location                                        = "ukwest"
  loc                                             = "ukw"
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
  nhs_host_url                                    = var.NHS_HOST_URL
  func_app_base_uri                               = var.FUNC_APP_BASE_URI
  func_app_slot_base_uri                          = var.FUNC_APP_SLOT_BASE_URI
  web_app_base_uri                                = var.WEB_APP_BASE_URI
  web_app_slot_base_uri                           = var.WEB_APP_SLOT_BASE_URI
  gov_notify_base_uri                             = var.GOV_NOTIFY_BASE_URI
  gov_notify_api_key                              = var.GOV_NOTIFY_API_KEY
  gov_notify_retry_options_max_retries            = var.GOV_NOTIFY_RETRY_OPTIONS_MAX_RETRIES
  gov_notify_retry_options_initial_delay_ms       = var.GOV_NOTIFY_RETRY_OPTIONS_INITIAL_DELAY_MS    
  gov_notify_retry_options_backoff_factor         = var.GOV_NOTIFY_RETRY_OPTIONS_BACKOFF_FACTOR
  booking_reminders_cron_schedule                 = var.BOOKING_REMINDERS_CRON_SCHEDULE
  unconfirmed_provisional_bookings_cron_schedule  = var.UNCONFIRMED_PROVISIONAL_BOOKINGS_CRON_SCHEDULE
  daily_site_summary_aggregation_cron_schedule    = var.DAILY_SITE_SUMMARY_AGGREGATION_CRON_SCHEDULE
  site_summary_days_forward                       = var.SITE_SUMMARY_DAYS_FORWARD
  site_summary_days_chunk_size                    = var.SITE_SUMMARY_DAYS_CHUNK_SIZE
  site_summary_first_run_date                     = var.SITE_SUMMARY_FIRST_RUN_DATE
  splunk_hec_token                                = var.SPLUNK_HEC_TOKEN
  splunk_host_url                                 = var.SPLUNK_HOST_URL
  autoscale_notification_email_address            = var.AUTOSCALE_NOTIFICATION_EMAIL_ADDRESS
  cosmos_endpoint                                 = var.COSMOS_ENDPOINT
  cosmos_token                                    = var.COSMOS_TOKEN
  app_config_connection                           = var.APP_CONFIG_CONNECTION
  create_data_extracts                            = true
  container_registry_server_url                   = var.CONTAINER_REGISTRY_SERVER_URL
  container_registry_username                     = var.CONTAINER_REGISTRY_USERNAME
  container_registry_password                     = var.CONTAINER_REGISTRY_PASSWORD
  auto_cancelled_bookings_cron_schedule           = var.AUTO_CANCELLED_BOOKINGS_CRON_SCHEDULE
  site_supports_service_sliding_cache_absolute_expiration_seconds = var.SITE_SUPPORTS_SERVICE_SLIDING_CACHE_ABSOLUTE_EXPIRATION_SECONDS
  site_supports_service_sliding_cache_slide_threshold_seconds     = var.SITE_SUPPORTS_SERVICE_SLIDING_CACHE_SLIDE_THRESHOLD_SECONDS
  site_supports_service_batch_multiplier          = var.SITE_SUPPORTS_SERVICE_BATCH_MULTIPLIER
  data_extract_file_sender_options_type           = "blob"
  disable_query_availability_function             = false
  create_high_load_function_app                   = true
  create_app_slot                                 = false
  create_autoscale_settings                       = false
  create_frontdoor                                = false
  create_cosmos_db                                = false
  create_app_config                               = false
  web_app_service_sku                             = "P1v3"
  web_app_service_plan_default_worker_count       = 6
  app_service_plan_zone_redundancy_enabled        = false
  web_app_service_plan_min_worker_count           = 4
  web_app_service_plan_max_worker_count           = 20
  web_app_service_plan_scale_out_worker_count_max = 1
  web_app_service_plan_scale_out_worker_count_min = 1
  web_app_service_plan_scale_in_worker_count      = 1
  app_insights_sampling_percentage                = 12.5
  storage_account_replication_type                = "LRS"
  audit_storage_account_replication_type          = "LRS"
  cosmos_automatic_failover_enabled               = true
  disable_bulk_import_function                    = true
  splunk_skip_verify_insecure                     = false
  splunk_data_channel                             = "5DCE2AC4-31D8-4A77-93C1-06515670AFD6"
  splunk_otel_image_version                       = "2.0"
  auto_cancelled_bookings_disabled                = false
  auditor_enable                                  = var.AUDITOR_ENABLE
  auditor_lease_container_name                    = var.AUDITOR_LEASE_CONTAINER_NAME
  auditor_worker_containers                       = var.AUDITOR_WORKER_CONTAINERS
  auditor_sink_exclusions                         = var.AUDITOR_SINK_EXCLUSIONS
  cosmos_booking_autoscale_settings = [{
    max_throughput = 60000
  }]
  cosmos_aggregation_autoscale_settings = [{
    max_throughput = 10000
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
