## App service plan and app service for web app
resource "azurerm_service_plan" "nbs_mya_web_app_service_plan" {
  name                   = "${var.application}-asp-${var.environment}-${var.loc}"
  resource_group_name    = data.azurerm_resource_group.nbs_mya_resource_group.name
  location               = data.azurerm_resource_group.nbs_mya_resource_group.location
  os_type                = "Linux"
  sku_name               = var.web_app_service_sku
  worker_count           = var.web_app_service_plan_default_worker_count
  zone_balancing_enabled = var.app_service_plan_zone_redundancy_enabled
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

  sticky_settings {
    app_setting_names = ["NBS_API_BASE_URL", "AUTH_HOST"]
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_linux_web_app_slot" "nbs_mya_web_app_preview" {
  count          = var.do_create_swap_slot ? 1 : 0
  name           = "preview"
  app_service_id = azurerm_linux_web_app.nbs_mya_web_app_service.id

  site_config {
    app_command_line = "node standalone/server.js"
    application_stack {
      node_version = "20-lts"
    }
  }

  app_settings = {
    NBS_API_BASE_URL = "https://${azurerm_windows_function_app_slot.nbs_mya_func_app_preview[0].default_hostname}"
    AUTH_HOST        = "https://${azurerm_windows_function_app_slot.nbs_mya_func_app_preview[0].default_hostname}"
  }

  identity {
    type = "SystemAssigned"
  }
}

# App service plan autoscale settings
resource "azurerm_monitor_autoscale_setting" "nbs_mya_web_app_service_autoscale_settings" {
  count               = var.do_create_autoscale_settings ? 1 : 0
  name                = "NbsMyaWebAppAutoscaleSetting"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  location            = data.azurerm_resource_group.nbs_mya_resource_group.location
  target_resource_id  = azurerm_service_plan.nbs_mya_web_app_service_plan.id

  profile {
    name = "defaultProfile"

    # Default capacity
    capacity {
      default = var.web_app_service_plan_default_worker_count
      minimum = var.web_app_service_plan_min_worker_count
      maximum = var.web_app_service_plan_max_worker_count
    }

    # CPU auto scale rule
    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.nbs_mya_web_app_service_plan.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "GreaterThan"
        threshold          = 75
        metric_namespace   = "microsoft.web/serverfarms"
      }

      scale_action {
        direction = "Increase"
        type      = "ChangeCount"
        value     = var.web_app_service_plan_scale_out_worker_count
        cooldown  = "PT5M"
      }
    }

    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.nbs_mya_web_app_service_plan.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "LessThan"
        threshold          = 30
        metric_namespace   = "microsoft.web/serverfarms"
      }

      scale_action {
        direction = "Decrease"
        type      = "ChangeCount"
        value     = var.web_app_service_plan_scale_in_worker_count
        cooldown  = "PT10M"
      }
    }

    # Memory auto scale rule
    rule {
      metric_trigger {
        metric_name        = "MemoryPercentage"
        metric_resource_id = azurerm_service_plan.nbs_mya_web_app_service_plan.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "GreaterThan"
        threshold          = 70
        metric_namespace   = "microsoft.web/serverfarms"
      }

      scale_action {
        direction = "Increase"
        type      = "ChangeCount"
        value     = var.web_app_service_plan_scale_out_worker_count
        cooldown  = "PT5M"
      }
    }


    rule {
      metric_trigger {
        metric_name        = "MemoryPercentage"
        metric_resource_id = azurerm_service_plan.nbs_mya_web_app_service_plan.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "LessThan"
        threshold          = 50
        metric_namespace   = "microsoft.web/serverfarms"
      }

      scale_action {
        direction = "Decrease"
        type      = "ChangeCount"
        value     = var.web_app_service_plan_scale_in_worker_count
        cooldown  = "PT10M"
      }
    }

    # HttpQueueLength auto scale rule
    rule {
      metric_trigger {
        metric_name        = "HttpQueueLength"
        metric_resource_id = azurerm_service_plan.nbs_mya_web_app_service_plan.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "GreaterThan"
        threshold          = 20
        metric_namespace   = "microsoft.web/serverfarms"
      }

      scale_action {
        direction = "Increase"
        type      = "ChangeCount"
        value     = var.web_app_service_plan_scale_out_worker_count
        cooldown  = "PT5M"
      }
    }


    rule {
      metric_trigger {
        metric_name        = "HttpQueueLength"
        metric_resource_id = azurerm_service_plan.nbs_mya_web_app_service_plan.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "LessThan"
        threshold          = 10
        metric_namespace   = "microsoft.web/serverfarms"
      }

      scale_action {
        direction = "Decrease"
        type      = "ChangeCount"
        value     = var.web_app_service_plan_scale_in_worker_count
        cooldown  = "PT10M"
      }
    }
  }

  notification {
    email {
      custom_emails = [var.autoscale_notification_email_address]
    }
  }
}
