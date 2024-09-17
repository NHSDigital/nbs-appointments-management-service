variable "application" {
  type    = string
  default = "nbs-appts"
}

variable "application_short" {
  type    = string
  default = "nbsappts"
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

variable "api_keys" {
  type      = string
  sensitive = true
}

variable "hmac_signing_key" {
  type      = string
  sensitive = true
}

variable "user_roles_changed_email_template_id" {
  type      = string
  sensitive = true
}

variable "gov_notify_api_key" {
  type      = string
  sensitive = true
}

variable "booking_made_email_template_id" {
  type      = string
  sensitive = true
}

variable "booking_made_sms_template_id" {
  type      = string
  sensitive = true
}