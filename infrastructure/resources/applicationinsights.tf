## Application insights
resource "azurerm_application_insights" "nbs_mya_application_insights" {
  name                = "${var.application}-ai-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = var.location
  application_type    = "web"
  sampling_percentage = var.app_insights_sampling_percentage
  workspace_id        = var.app_insights_workspace_id
}
