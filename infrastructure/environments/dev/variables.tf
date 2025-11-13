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

variable "GOV_NOTIFY_RETRY_OPTIONS_MAX_RETRIES" {
  type      = string
}

variable "GOV_NOTIFY_RETRY_OPTIONS_INITIAL_DELAY_MS" {
  type      = string
}

variable "GOV_NOTIFY_RETRY_OPTIONS_BACKOFF_FACTOR" {
  type      = string
}

variable "BOOKING_REMINDERS_CRON_SCHEDULE" {
  type      = string
  sensitive = false
}

variable "UNCONFIRMED_PROVISIONAL_BOOKINGS_CRON_SCHEDULE" {
  type      = string
  sensitive = false
}

variable "DAILY_SITE_SUMMARY_AGGREGATION_CRON_SCHEDULE" {
  type      = string
  sensitive = false
}

variable "SITE_SUMMARY_DAYS_FORWARD" {
  type      = string
  sensitive = false
}

variable "SITE_SUMMARY_DAYS_CHUNK_SIZE" {
  type      = string
  sensitive = false
}

variable "SITE_SUMMARY_FIRST_RUN_DATE" {
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

variable "CONTAINER_REGISTRY_SERVER_URL" {
  type = string
}

variable "CONTAINER_REGISTRY_USERNAME" {
  type = string
}

variable "CONTAINER_REGISTRY_PASSWORD" {
  type = string
  sensitive = true
}

variable "AUTO_CANCELLED_BOOKINGS_CRON_SCHEDULE" {
  type = string
  sensitive = false
}

variable "KEYVAULT_CLIENT_ID" {
  type = string
}

variable "KEYVAULT_CLIENT_SECRET" {
  type = string
  sensitive = true
}

variable "CLEANUP_BATCH_SIZE" {
  type        = string
}

variable "CLEANUP_DEGREE_OF_PARALLELISM" {
  type        = string
}
