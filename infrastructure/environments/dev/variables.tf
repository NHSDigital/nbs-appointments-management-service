variable "FUNC_APP_BASE_URI" {
  type      = string
  sensitive = false
}

variable "WEB_APP_BASE_URI" {
  type      = string
  sensitive = false
}

variable "NHS_MAIL_ISSUER" {
  type      = string
  sensitive = false
}

variable "NHS_MAIL_AUTHORIZE_URI" {
  type      = string
  sensitive = false
}

variable "NHS_MAIL_TOKEN_URI" {
  type      = string
  sensitive = false
}

variable "NHS_MAIL_JWKS_URI" {
  type      = string
  sensitive = false
}

variable "NHS_MAIL_CLIENT_ID" {
  type      = string
  sensitive = false
}

variable "NHS_MAIL_CLIENT_SECRET" {
  type      = string
  sensitive = true
}

variable "OKTA_ISSUER" {
  type      = string
  sensitive = false
}

variable "OKTA_AUTHORIZE_URI" {
  type      = string
  sensitive = false
}

variable "OKTA_TOKEN_URI" {
  type      = string
  sensitive = false
}

variable "OKTA_JWKS_URI" {
  type      = string
  sensitive = false
}

variable "OKTA_CLIENT_ID" {
  type      = string
  sensitive = false
}

variable "OKTA_CLIENT_SECRET" {
  type      = string
  sensitive = true
}

variable "OKTA_MANAGEMENT_ID" {
  type      = string
  sensitive = false
}

variable "OKTA_PRIVATE_KEY_KID" {
  type      = string
  sensitive = true
}

variable "OKTA_PEM" {
  type      = string
  sensitive = true
}

variable "AUTH_PROVIDER_CHALLENGE_PHRASE" {
  type      = string
  sensitive = false
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

variable "BUILD_NUMBER" {
  type = string
}
