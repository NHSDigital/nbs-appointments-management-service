
## App service plan and app service for web app

resource "azurerm_service_plan" "nbs_appts_web_service_plan" {
  name                = "${var.application}-asp-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_appts_rg.name
  location            = data.azurerm_resource_group.nbs_appts_rg.location
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_linux_web_app" "nbs_appts_web_app_service" {
  name                = "${var.application}-wa-${var.environment}-${var.loc}"
  resource_group_name = data.azurerm_resource_group.nbs_appts_rg.name
  location            = data.azurerm_resource_group.nbs_appts_rg.location
  service_plan_id     = azurerm_service_plan.nbs_appts_web_service_plan.id
  https_only          = true

  site_config {
    app_command_line = "node standalone/server.js"
    application_stack {
      node_version = "20-lts"
    }
  }

  app_settings = {
    NBS_API_BASE_URL = "https://${azurerm_windows_function_app.nbs_appts_func_app.default_hostname}"
    AUTH_HOST        = "https://${azurerm_windows_function_app.nbs_appts_func_app.default_hostname}"
  }

  identity {
    type = "SystemAssigned"
  }
}
