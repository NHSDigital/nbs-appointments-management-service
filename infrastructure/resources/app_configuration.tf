resource "azurerm_app_configuration" "nbs_mya_app_configuration" {
  count               = var.create_app_config ? 1 : 0
  name                = "${var.application}-config-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = var.location
  sku                 = "standard"

  replica {
    name     = "replicaukw"
    location = "UK West"
  }
}
