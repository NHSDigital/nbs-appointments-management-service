terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.0.1"
    }
  }

  backend "azurerm" {
    resource_group_name  = "nbs-mya-rg-dev-uks"
    storage_account_name = "nbsmyatfdevuks"
    container_name       = "tfstate"
    key                  = "dev.tfstate"
  }

  required_version = ">= 1.6.5"
}

provider "azurerm" {
  features {}
}

module "api" {
  environment                    = "dev"
  source                         = "../../resources"
  auth_provider_issuer           = var.AUTH_PROVIDER_ISSUER
  auth_provider_authorize_uri    = var.AUTH_PROVIDER_AUTHORIZE_URI
  auth_provider_token_uri        = var.AUTH_PROVIDER_TOKEN_URI
  auth_provider_jwks_uri         = var.AUTH_PROVIDER_JWKS_URI
  auth_provider_challenge_phrase = var.AUTH_PROVIDER_CHALLENGE_PHRASE
  auth_provider_client_id        = var.AUTH_PROVIDER_CLIENT_ID
  auth_provider_return_uri       = var.AUTH_PROVIDER_RETURN_URI
  api_keys                       = var.API_KEYS
  hmac_signing_key               = var.HMAC_SIGNING_KEY
  gov_notify_api_key             = var.GOV_NOTIFY_API_KEY
}