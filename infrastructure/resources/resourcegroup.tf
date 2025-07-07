data "azurerm_resource_group" "nbs_mya_resource_group" {
  name = "${var.application}${local.is_dev ? var.environment : ""}-rg-${local.is_dev ? "int" : var.environment}-${var.loc}"
}
