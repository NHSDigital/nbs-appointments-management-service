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

variable "auth_provider_issuer" {
  type      = string
  sensitive = false
}

variable "auth_provider_authorize_uri" {
  type      = string
  sensitive = false
}

variable "auth_provider_token_uri" {
  type      = string
  sensitive = false
}

variable "auth_provider_client_code_exchange_uri" {
  type = string
}

variable "auth_provider_jwks_uri" {
  type      = string
  sensitive = false
}

variable "auth_provider_challenge_phrase" {
  type      = string
  sensitive = false
}

variable "auth_provider_client_id" {
  type      = string
  sensitive = false
}

variable "auth_provider_return_uri" {
  type      = string
  sensitive = false
}

variable "gov_notify_api_key" {
  type      = string
  sensitive = true
}

variable "unconfirmed_provisional_bookings_cron_schedule" {
  type      = string
  sensitive = false
}

variable "booking_reminders_cron_schedule" {
  type      = string
  sensitive = false
}

variable "splunk_hec_token" {
  type      = string
  sensitive = true
}

variable "splunk_host_url" {
  type      = string
  sensitive = false
}
