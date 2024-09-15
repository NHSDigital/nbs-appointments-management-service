terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.0.1"
    }
  }

  backend "azurerm" {
    resource_group_name  = "nbs-appts-rg-exp-uks"
    storage_account_name = "nbsapptstfexpuks"
    container_name       = "tfstate"
    key                  = "exp.tfstate"
  }

  required_version = ">= 1.6.5"
}

provider "azurerm" {
  features {}
}

module "api" {
  environment                    = "exp"
  source                         = "../../resources"
  auth_provider_issuer           = var.AUTH_PROVIDER_ISSUER
  auth_provider_authorize_uri    = var.AUTH_PROVIDER_AUTHORIZE_URI
  auth_provider_token_uri        = var.AUTH_PROVIDER_TOKEN_URI
  auth_provider_jwks_uri         = var.AUTH_PROVIDER_JWKS_URI
  auth_provider_challenge_phrase = var.AUTH_PROVIDER_CHALLENGE_PHRASE
  auth_provider_client_id        = var.AUTH_PROVIDER_CLIENT_ID
  auth_provider_return_uri       = var.AUTH_PROVIDER_RETURN_URI
  api_keys                       = var.API_KEYS
  apim_uri                       = var.APIM_URI
  apim_subscription_key          = var.APIM_SUBSCRIPTION_KEY
  hmac_signing_key               = var.HMAC_SIGNING_KEY
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

variable "AUTH_PROVIDER_RETURN_URI" {
  type      = string
  sensitive = false
}

variable "API_KEYS" {
  type      = string
  sensitive = true
}

variable "APIM_URI" {
  type      = string
  sensitive = false
}

variable "APIM_SUBSCRIPTION_KEY" {
  type      = string
  sensitive = true
}

variable "HMAC_SIGNING_KEY" {
  type      = string
  sensitive = true
}
