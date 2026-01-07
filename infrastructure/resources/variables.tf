variable "application" {
  type    = string
  default = "nbs-mya"
}

variable "application_short" {
  type    = string
  default = "nbsmya"
}

variable "environment" {
  type = string
}

variable "location" {
  type    = string
}

variable "loc" {
  type    = string
}

variable "nhs_host_url" {
  type    = string
  default = ""
}
variable "func_app_base_uri" {
  type = string
}

variable "web_app_base_uri" {
  type = string
}

variable "func_app_slot_base_uri" {
  type    = string
  default = ""
}

variable "web_app_slot_base_uri" {
  type    = string
  default = ""
}

variable "nhs_mail_issuer" {
  type = string
}

variable "nhs_mail_authorize_uri" {
  type = string
}

variable "nhs_mail_token_uri" {
  type = string
}

variable "nhs_mail_jwks_uri" {
  type = string
}

variable "auth_provider_challenge_phrase" {
  type = string
}

variable "nhs_mail_client_id" {
  type = string
}

variable "nhs_mail_client_secret" {
  type      = string
  sensitive = true
  default   = ""
}

variable "okta_issuer" {
  type = string
}

variable "okta_authorize_uri" {
  type = string
}

variable "okta_token_uri" {
  type = string
}

variable "okta_jwks_uri" {
  type = string
}

variable "okta_client_id" {
  type = string
}

variable "okta_client_secret" {
  type      = string
  sensitive = true
  default   = ""
}

variable "okta_domain" {
  type = string
}

variable "okta_management_id" {
  type = string
}

variable "okta_private_key_kid" {
  type      = string
  sensitive = true
}

variable "okta_pem" {
  type      = string
  sensitive = true
}

variable "gov_notify_base_uri" {
  type = string
}

variable "gov_notify_api_key" {
  type      = string
  sensitive = true
}

variable "gov_notify_retry_options_max_retries" {
  type      = string
}

variable "gov_notify_retry_options_initial_delay_ms" {
  type      = string
}

variable "gov_notify_retry_options_backoff_factor" {
  type      = string
}

variable "unconfirmed_provisional_bookings_cron_schedule" {
  type = string
}

variable "booking_reminders_cron_schedule" {
  type = string
}

variable "daily_site_summary_aggregation_cron_schedule" {
  type = string
}

variable "site_summary_days_forward" {
  type = string
}

variable "site_summary_days_chunk_size" {
  type = string
}

variable "site_summary_first_run_date" {
  type = string
}

variable "splunk_hec_token" {
  type      = string
  sensitive = true
}

variable "splunk_host_url" {
  type = string
}

variable "cosmos_geo_locations" {
  type    = list(any)
  default = []
}
variable "cosmos_capabilities" {
  type    = list(any)
  default = []
}

variable "cosmos_booking_autoscale_settings" {
  type    = list(any)
  default = []
}

variable "cosmos_aggregation_autoscale_settings" {
  type    = list(any)
  default = []
}

variable "cosmos_core_autoscale_settings" {
  type    = list(any)
  default = []
}

variable "cosmos_index_autoscale_settings" {
  type    = list(any)
  default = []
}

variable "cosmos_audit_autoscale_settings" {
  type    = list(any)
  default = []
}

variable "web_app_service_sku" {
  type = string
}

variable "web_app_service_plan_default_worker_count" {
  type = number
}

variable "app_service_plan_zone_redundancy_enabled" {
  type = bool
}

variable "web_app_service_plan_min_worker_count" {
  type    = number
  default = 1
}

variable "web_app_service_plan_max_worker_count" {
  type    = number
  default = 1
}

variable "web_app_service_plan_scale_out_worker_count_max" {
  type    = number
  default = 1
}

variable "web_app_service_plan_scale_out_worker_count_min" {
  type    = number
  default = 1
}

variable "web_app_service_plan_scale_in_worker_count" {
  type    = number
  default = 1
}

variable "autoscale_notification_email_address" {
  type    = string
  default = ""
}

variable "storage_account_replication_type" {
  type = string
}

variable "cosmos_automatic_failover_enabled" {
  type = bool
}

variable "app_insights_sampling_percentage" {
  type = number
}

variable "disable_query_availability_function" {
  type = bool
}

variable "create_high_load_function_app" {
  type = bool
}

variable "create_autoscale_settings" {
  type = bool
}

variable "create_app_slot" {
  type = bool
}

variable "create_frontdoor" {
  type = bool
}

variable "create_cosmos_db" {
  type = bool
}

variable "create_app_config" {
  type = bool
}

variable "build_number" {
  type = string
}

variable "cosmos_endpoint" {
  type = string
  default = ""
}

variable "cosmos_token" {
  type      = string
  default = ""
  sensitive = true
}

variable "app_config_connection" {
  type = string
  default = ""
  sensitive = true
}

variable "disable_bulk_import_function" {
  type = bool
}

variable "container_registry_server_url" {
  type = string
}

variable "container_registry_username" {
  type = string
}

variable "container_registry_password" {
  type = string
  sensitive = true
}

variable "create_data_extracts" {
  type = bool
}

variable "data_extract_timeout" {
  type = number
  default = 7200
}

variable "data_extract_retry_limit" {
  type = number
  default = 0
}

variable "data_extract_schedule" {
  type = string
  default = "0 2 * * *"
}

variable "data_extract_file_sender_options_type" {
  type = string
}

variable "mesh_mailbox_destination" {
  type = string
  default = ""
}

variable "mesh_mailbox_workflow_capacity" {
  type = string
  default = ""
}

variable "mesh_mailbox_workflow_booking" {
  type = string
  default = ""
}

variable "mesh_client_options_base_url" {
  type = string
  default = ""
}

variable "mesh_authorization_options_mailbox_id" {
  type = string
  default = ""
}

variable "mesh_authorization_options_certificate_name" {
  type = string
  default = ""
}

variable "keyvault_Name" {
  type = string
  default = ""
}

variable "keyvault_tenant_id" {
  type = string
  default = ""
}

variable "keyvault_client_id" {
  type = string
  default = ""
}

variable "keyvault_client_secret" {
  type = string
  default = "UNSET"
  sensitive = true
}

variable "auto_cancelled_bookings_cron_schedule" {
  type = string
}

variable "splunk_data_channel" {
  type = string
}

variable "splunk_skip_verify_insecure" {
  type = bool
}

variable "splunk_otel_image_version" {
  type = string
}

variable "auto_cancelled_bookings_disabled" {
  type = bool
}

variable "site_supports_service_sliding_cache_slide_threshold_seconds" {
  type = string
}

variable "site_supports_service_sliding_cache_absolute_expiration_seconds" {
  type = string
}

variable "auditor_enable" {
  type = bool
}

variable "auditor_lease_container_name" {
  type = string
}

variable "auditor_worker_containers" {
  type = list(string)
}

variable "auditor_sink_exclusions" {
  type = list(object({
    source         = string
    excluded_paths = list(string)
  }))
}

variable "auditor_archive_after_days" {
  type = number
  default = 1
}