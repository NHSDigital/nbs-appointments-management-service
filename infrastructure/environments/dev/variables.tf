variable "AUTH_PROVIDER_ISSUER" {
  type      = string
  sensitive = false
}

variable "AUTH_PROVIDER_AUTHORIZE_URI" {
  type      = string
  sensitive = false
}

variable "AUTH_PROVIDER_TOKEN_URI" {
  type      = string
  sensitive = false
}

variable "AUTH_PROVIDER_JWKS_URI" {
  type      = string
  sensitive = false
}

variable "AUTH_PROVIDER_CHALLENGE_PHRASE" {
  type      = string
  sensitive = false
}

variable "AUTH_PROVIDER_CLIENT_ID" {
  type      = string
  sensitive = false
}

variable "AUTH_PROVIDER_RETURN_URI" {
  type      = string
  sensitive = false
}

variable "API_KEYS" {
  type      = string
  sensitive = true
}

variable "GOV_NOTIFY_API_KEY" {
  type      = string
  sensitive = true
}

variable "BOOKING_REMINDERS_CRON_SCHEDULE" {
  type      = string
  sensitive = false
}