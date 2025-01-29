data "azurerm_resource_group" "nbs_mya_resource_group" {
  name = "${var.application}-rg-${var.environment}-${var.loc}"
}
