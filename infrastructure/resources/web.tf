
## App service plan and app service for web app

resource "azurerm_service_plan" "nbs_mya_web_app_service_plan" {
  name                = "${var.application}-asp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_linux_web_app" "nbs_mya_web_app_service" {
  name                = "${var.application}-app-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
  service_plan_id     = azurerm_service_plan.nbs_mya_web_app_service_plan.id
  https_only          = true

  site_config {
    app_command_line = "node standalone/server.js"
    application_stack {
      node_version = "20-lts"
    }
  }

  app_settings = {
    NBS_API_BASE_URL = "https://${azurerm_windows_function_app.nbs_mya_func_app.default_hostname}"
    AUTH_HOST        = "https://${azurerm_windows_function_app.nbs_mya_func_app.default_hostname}"
  }

  identity {
    type = "SystemAssigned"
  }
}
