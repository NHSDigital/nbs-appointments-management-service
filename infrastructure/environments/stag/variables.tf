variable "NHS_HOST_URL" {
  type      = string
  sensitive = false
}

variable "FUNC_APP_BASE_URI" {
  type      = string
  sensitive = false
}

variable "WEB_APP_BASE_URI" {
  type      = string
  sensitive = false
}

variable "FUNC_APP_SLOT_BASE_URI" {
  type      = string
  sensitive = false
}

variable "WEB_APP_SLOT_BASE_URI" {
  type      = string
  sensitive = false
}

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

variable "AUTH_PROVIDER_CLIENT_SECRET" {
  type      = string
  sensitive = true
}

variable "GOV_NOTIFY_BASE_URI" {
  type = string
}

variable "GOV_NOTIFY_API_KEY" {
  type      = string
  sensitive = true
}

variable "BOOKING_REMINDERS_CRON_SCHEDULE" {
  type      = string
  sensitive = false
}

variable "UNCONFIRMED_PROVISIONAL_BOOKINGS_CRON_SCHEDULE" {
  type      = string
  sensitive = false
}

variable "SPLUNK_HOST_URL" {
  type      = string
  sensitive = false
}

variable "SPLUNK_HEC_TOKEN" {
  type      = string
  sensitive = true
}

variable "AUTOSCALE_NOTIFICATION_EMAIL_ADDRESS" {
  type = string
}
