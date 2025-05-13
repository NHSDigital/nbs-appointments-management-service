locals {
  is_env_standard = contains(["dev", "int", "stag", "prod"], var.environment)
}

data "azurerm_resource_group" "nbs_mya_resource_group" {
  name = local.is_env_standard ? "${var.application}-rg-${var.environment}-${var.loc}" : "${var.application}${var.environment}-rg-stag-${var.loc}"
}
