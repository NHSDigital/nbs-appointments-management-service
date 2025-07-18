## Application insights

resource "azurerm_log_analytics_workspace" "nbs_mya_log_analytics_workspace" {
  name                = "${var.application}-law-${var.environment}-${var.loc}"
  resource_group_name = local.resource_group_name
  location            = var.location
  sku                 = "PerGB2018"
  retention_in_days   = 90
}

resource "azurerm_application_insights" "nbs_mya_application_insights" {
  name                = "${var.application}-ai-${var.environment}-${var.loc}"
  resource_group_name = local.resource_group_name
  location            = var.location
  application_type    = "web"
  sampling_percentage = var.app_insights_sampling_percentage
  workspace_id        = azurerm_log_analytics_workspace.nbs_mya_log_analytics_workspace.id
}
