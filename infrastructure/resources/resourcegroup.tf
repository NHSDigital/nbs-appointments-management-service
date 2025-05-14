data "azurerm_resource_group" "nbs_mya_resource_group" {
  name = "${var.application}${local.is_perf ? var.application : ""}-rg-${local.is_perf ? "stag" : var.environment}-${var.loc}"
}
