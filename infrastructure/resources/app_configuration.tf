resource "azurerm_app_configuration" "nbs_mya_app_configuration" {
  name                = "${var.application_short}appcon${var.environment}${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
}