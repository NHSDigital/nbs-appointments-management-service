resource "azurerm_resource_group" "nbs_mya_resource_group" {
  count    = var.environment == "pen" ? 1 : 0
  name     = "${var.application}-rg-${var.environment}-${var.loc}"
  location = "uksouth"
}