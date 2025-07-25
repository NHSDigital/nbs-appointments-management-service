locals {
  mya_function_app_url     = var.environment == "stag" || var.environment == "prod" ? "${var.nhs_host_url}/manage-your-appointments" : var.func_app_base_uri
  auth_provider_return_uri = var.environment == "stag" || var.environment == "prod" ? "${var.nhs_host_url}/manage-your-appointments/api/auth-return" : "${var.func_app_base_uri}/api/auth-return"
  client_code_exchange_uri = var.environment == "stag" || var.environment == "prod" ? "${var.nhs_host_url}/manage-your-appointments/auth/set-cookie" : "${var.web_app_base_uri}/manage-your-appointments/auth/set-cookie"
  resource_group_name = var.environment == "pen"  ? azurerm_resource_group.nbs_mya_resource_group[0].name : var.environment == "perf" ? "${var.application}perf-rg-stag-${var.loc}" : var.environment == "dev" ? "${var.application}dev-rg-int-${var.loc}" : "${var.application}-rg-${var.environment}-${var.loc}"
}
