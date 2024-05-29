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

variable "auth_provider_provider_uri" {
  type      = string
  sensitive = false
}

variable "auth_provider_issuer" {
  type      = string
  sensitive = false
}

variable "auth_provider_authorize_path" {
  type      = string
  sensitive = false
}

variable "auth_provider_token_path" {
  type      = string
  sensitive = false
}

variable "auth_provider_jwks_path" {
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

variable "apim_uri" {
  type      = string
  sensitive = false
}

variable "apim_subscription_key" {
  type      = string
  sensitive = true
}