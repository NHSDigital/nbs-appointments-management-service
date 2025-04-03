resource "azurerm_app_configuration" "nbs_mya_app_configuration" {
  name                = "${var.application}-config-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = var.location
  sku                 = "standard"
  soft_delete_retention_days = 1
}
