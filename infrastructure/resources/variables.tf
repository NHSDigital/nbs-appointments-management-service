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
  default = "uksouth"
}

variable "loc" {
  type    = string
  default = "uks"
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

variable "auth_provider_issuer" {
  type = string
}

variable "auth_provider_authorize_uri" {
  type = string
}

variable "auth_provider_token_uri" {
  type = string
}

variable "auth_provider_jwks_uri" {
  type = string
}

variable "auth_provider_challenge_phrase" {
  type = string
}

variable "auth_provider_client_id" {
  type = string
}

variable "auth_provider_client_secret" {
  type      = string
  sensitive = true
  default   = ""
}

variable "gov_notify_base_uri" {
  type = string
}

variable "gov_notify_api_key" {
  type      = string
  sensitive = true
}

variable "unconfirmed_provisional_bookings_cron_schedule" {
  type = string
}

variable "booking_reminders_cron_schedule" {
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

variable "cosmos_core_autoscale_settings" {
  type    = list(any)
  default = []
}

variable "cosmos_index_autoscale_settings" {
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

variable "create_autoscale_settings" {
  type    = bool
  default = false
}

variable "web_app_service_plan_min_worker_count" {
  type    = number
  default = 1
}

variable "web_app_service_plan_max_worker_count" {
  type    = number
  default = 1
}

variable "web_app_service_plan_scale_out_worker_count" {
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

variable "create_app_slot" {
  type    = bool
  default = false
}

variable "create_frontdoor" {
  type    = bool
  default = false
}
